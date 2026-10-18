import { appendResponseHeader } from 'h3'
import type { NuxtApp } from '#app'
import type { ApiEnvelope } from '~/types/api'
import { applySetCookiesToCookieHeader } from '~/utils/cookieHeader'

export class ApiError extends Error {
  code: string
  status: number

  constructor(message: string, code: string, status: number) {
    super(message)
    this.name = 'ApiError'
    this.code = code
    this.status = status
  }
}

const AUTH_LOGIN_PATH = '/api/v1/auth/login'
const AUTH_GOOGLE_PATH = '/api/v1/auth/google'
const AUTH_REGISTER_PATH = '/api/v1/auth/register'
const AUTH_REFRESH_PATH = '/api/v1/auth/refresh'
const AUTH_LOGOUT_PATH = '/api/v1/auth/logout'
const AUTH_ME_PATH = '/api/v1/auth/me'
const ADMIN_AUTH_LOGIN_PATH = '/api/v1/admin/auth/login'
const ADMIN_AUTH_REFRESH_PATH = '/api/v1/admin/auth/refresh'
const ADMIN_AUTH_LOGOUT_PATH = '/api/v1/admin/auth/logout'
const ADMIN_AUTH_ME_PATH = '/api/v1/admin/auth/me'
const ADMIN_PATH_PREFIX = '/api/v1/admin/'

// 401 時先嘗試 refresh 一次，多個請求同時 401 只會觸發一次 refresh。
// admin 與會員各自獨立的 in-flight refresh，避免兩邊 session 互相干擾。
// 以 Nuxt app 為 key 而不是放在 module 層級：server 端的 module 是所有 incoming request 共用的，
// 放在 module 層級會讓不同使用者同時進行的 SSR 等到別人的 refresh 結果。
interface InFlightRefresh {
  member?: Promise<boolean>
  admin?: Promise<boolean>
}
const inFlightRefreshes = new WeakMap<NuxtApp, InFlightRefresh>()

/**
 * 呼叫本站 Nuxt server（server/api/**）而非直接打後端 —— 後端呼叫改由 Nitro 以
 * Authorization bearer header 代打，SSR 與 CSR 都是同源請求，不再需要手動轉發 cookie。
 */
export const useApi = () => {
  const nuxtApp = useNuxtApp()
  // 不用 useRoute()：這個 composable 可能在 middleware 內被間接呼叫（例如 fetchMe() 401 時），
  // 此時呼叫 useRoute() 會觸發 Nuxt 警告且結果不可靠；改讀 router 的 currentRoute，在任何地方呼叫都安全。

  // SSR 時一定要用 useRequestFetch()，不能用全域 $fetch：全域 $fetch 在 server 端就是
  // globalThis.$fetch，不會帶上目前這次 incoming request 的 cookie，打本站 server/api/**
  // 時 session cookie 讀不到，會讓每個 SSR 請求都被當成未登入。useRequestFetch() 在 server 端
  // 回傳的是綁定當前 event 的 $fetch（h3 fetchWithEvent），才會自動轉發 cookie；client 端則
  // 等同全域 $fetch，行為不變。
  const requestFetch = useRequestFetch()

  // SSR 時 useRequestFetch() 打本站 server route 是 sub-request：它的 Set-Cookie 不會自己傳回瀏覽器，
  // 之後的 sub-request 也只會複製這次 incoming request 的 Cookie header（h3 getProxyRequestHeaders）。
  // 所以 refresh 換到新 token 後，要把 Set-Cookie 轉到最外層 response 讓瀏覽器拿到，
  // 並改寫 incoming request 的 Cookie header，讓 retry 與這次 render 內之後的請求都帶上新 token。
  const ssrEvent = import.meta.server ? useRequestEvent() : undefined
  const forwardSetCookiesToSsrResponse = ({ response }: { response: Response }) => {
    if (!ssrEvent) return
    const setCookies = response.headers.getSetCookie()
    if (setCookies.length === 0) return

    for (const setCookie of setCookies) {
      appendResponseHeader(ssrEvent, 'set-cookie', setCookie)
    }
    ssrEvent.node.req.headers.cookie = applySetCookiesToCookieHeader(ssrEvent.node.req.headers.cookie, setCookies)
  }

  const toApiError = (e: unknown) => {
    const fetchError = e as { data?: ApiEnvelope<unknown>, status?: number, message?: string }
    // 429 可能不是後端 envelope（例如被反向代理直接擋下），此時改用固定的友善訊息而非 "Too Many Requests"。
    const rateLimitedMessage = fetchError.status === 429 ? '請求過於頻繁，請稍後再試' : undefined
    return new ApiError(
      fetchError.data?.message ?? rateLimitedMessage ?? fetchError.message ?? '發生未知錯誤',
      fetchError.data?.code ?? 'UNKNOWN',
      fetchError.status ?? 0
    )
  }

  const fetchEnvelope = <T>(path: string, options: Parameters<typeof $fetch>[1] = {}) =>
    requestFetch<ApiEnvelope<T>>(path, {
      credentials: 'include',
      ...options,
      ...(ssrEvent && { onResponse: forwardSetCookiesToSsrResponse })
    })

  const refreshOnce = (kind: keyof InFlightRefresh, fetchMe: () => Promise<boolean>) => {
    let inFlight = inFlightRefreshes.get(nuxtApp)
    if (!inFlight) {
      inFlight = {}
      inFlightRefreshes.set(nuxtApp, inFlight)
    }
    inFlight[kind] ??= fetchMe().finally(() => {
      inFlight[kind] = undefined
    })
    return inFlight[kind]
  }

  const request = async <T>(path: string, options: Parameters<typeof $fetch>[1] = {}, isRetry = false): Promise<T> => {
    try {
      const envelope = await fetchEnvelope<T>(path, options)
      return envelope.data as T
    } catch (e) {
      const apiError = toApiError(e)

      const isAdminPath = path.startsWith(ADMIN_PATH_PREFIX)
      // /me、/refresh、/logout 由 store 的 fetchMe() 自己處理 401 → fallback refresh，不透過這裡的自動重試機制，
      // 否則 fetchMe() 內部呼叫 /me 失敗時會在這裡又觸發一次 fetchMe()，多繞一圈邏輯。
      // /login、/google、/register 401 代表帳密錯誤，不是 access token 過期，不該觸發 refresh，
      // 否則登入失敗時會被誤判成「已登入但 token 過期」而嘗試 refresh，refresh 也失敗後又強制導回登入頁，
      // 蓋掉原本該顯示給使用者的「帳號密碼錯誤」訊息。
      const isAuthEndpoint = isAdminPath
        ? path === ADMIN_AUTH_LOGIN_PATH || path === ADMIN_AUTH_REFRESH_PATH || path === ADMIN_AUTH_LOGOUT_PATH || path === ADMIN_AUTH_ME_PATH
        : path === AUTH_LOGIN_PATH || path === AUTH_GOOGLE_PATH || path === AUTH_REGISTER_PATH || path === AUTH_REFRESH_PATH || path === AUTH_LOGOUT_PATH || path === AUTH_ME_PATH

      if (apiError.status === 401 && !isRetry && !isAuthEndpoint) {
        if (isAdminPath) {
          const refreshed = await refreshOnce('admin', () => useAdminAuthStore().fetchMe())

          if (refreshed) {
            return request<T>(path, options, true)
          }

          useAdminAuthStore().clearSession()
          // navigateTo 需要 Nuxt context，在此同步呼叫點以外的 await 之後可能遺失，故用 runWithContext 還原。
          // SSR 時 navigateTo 回傳的 redirect signal 必須被 return 出去才會真正觸發導頁，否則會被下面的 throw 蓋掉。
          // 帶上 redirect query，讓登入成功後導回原本要訪問的頁面（與 admin-auth middleware 行為一致）。
          return await nuxtApp.runWithContext(() =>
            navigateTo(`/admin/login?redirect=${encodeURIComponent(nuxtApp.$router.currentRoute.value.fullPath)}`)) as T
        } else {
          const refreshed = await refreshOnce('member', () => useAuthStore().fetchMe())

          if (refreshed) {
            return request<T>(path, options, true)
          }

          useAuthStore().clearSession()
          // 帶上 redirect query，讓登入成功後導回原本要訪問的頁面（與 auth middleware 行為一致）。
          return await nuxtApp.runWithContext(() =>
            navigateTo(`/login?redirect=${encodeURIComponent(nuxtApp.$router.currentRoute.value.fullPath)}`)) as T
        }
      }

      if (apiError.status === 401 && isRetry) {
        console.warn('[useApi] retry ALSO 401 on', path, '- giving up, new cookie was not applied')
      }

      throw apiError
    }
  }

  return {
    get: <T>(path: string, query?: Record<string, unknown>) =>
      request<T>(path, { method: 'GET', query }),
    post: <T>(path: string, body?: object) =>
      request<T>(path, { method: 'POST', body }),
    put: <T>(path: string, body?: object) =>
      request<T>(path, { method: 'PUT', body }),
    del: <T>(path: string) =>
      request<T>(path, { method: 'DELETE' }),
    // FormData body：ofetch 會自動偵測並跳過 JSON 序列化、正確設定 multipart boundary。
    postForm: <T>(path: string, formData: FormData) =>
      request<T>(path, { method: 'POST', body: formData })
  }
}
