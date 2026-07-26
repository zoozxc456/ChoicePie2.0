import type { ApiEnvelope } from '~/types/api'

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

const AUTH_REFRESH_PATH = '/api/v1/auth/refresh'
const AUTH_LOGOUT_PATH = '/api/v1/auth/logout'
const AUTH_ME_PATH = '/api/v1/auth/me'
const ADMIN_AUTH_REFRESH_PATH = '/api/v1/admin/auth/refresh'
const ADMIN_AUTH_LOGOUT_PATH = '/api/v1/admin/auth/logout'
const ADMIN_AUTH_ME_PATH = '/api/v1/admin/auth/me'
const ADMIN_PATH_PREFIX = '/api/v1/admin/'

// 401 時先嘗試 refresh 一次，多個請求同時 401 只會觸發一次 refresh。
// admin 與會員各自獨立的 in-flight refresh，避免兩邊 session 互相干擾。
let refreshPromise: Promise<boolean> | null = null
let adminRefreshPromise: Promise<boolean> | null = null

/** 呼叫後端 ChoicePie.Backend.WebApi，統一帶入 baseURL 與 cookie 憑證，並解開 ApiResponse 信封 */
export const useApi = () => {
  const config = useRuntimeConfig()
  const nuxtApp = useNuxtApp()
  const route = useRoute()

  const toApiError = (e: unknown) => {
    const fetchError = e as { data?: ApiEnvelope<unknown>, status?: number, message?: string }
    return new ApiError(
      fetchError.data?.message ?? fetchError.message ?? '發生未知錯誤',
      fetchError.data?.code ?? 'UNKNOWN',
      fetchError.status ?? 0
    )
  }

  const fetchEnvelope = <T>(path: string, options: Parameters<typeof $fetch>[1] = {}) =>
    $fetch<ApiEnvelope<T>>(path, {
      baseURL: config.public.apiBaseUrl,
      credentials: 'include',
      // SSR 沒有瀏覽器 cookie jar，需手動把 incoming request 的 cookie 轉發給後端才能帶出登入狀態。
      headers: import.meta.server ? useRequestHeaders(['cookie']) : undefined,
      ...options
    })

  const request = async <T>(path: string, options: Parameters<typeof $fetch>[1] = {}, isRetry = false): Promise<T> => {
    try {
      const envelope = await fetchEnvelope<T>(path, options)
      return envelope.data as T
    } catch (e) {
      const apiError = toApiError(e)

      const isAdminPath = path.startsWith(ADMIN_PATH_PREFIX)
      // /me 由 store 的 fetchMe() 自己處理 401 → fallback refresh，不透過這裡的自動重試機制，
      // 否則 fetchMe() 內部呼叫 /me 失敗時會在這裡又觸發一次 fetchMe()，多繞一圈邏輯。
      const isAuthEndpoint = isAdminPath
        ? path === ADMIN_AUTH_REFRESH_PATH || path === ADMIN_AUTH_LOGOUT_PATH || path === ADMIN_AUTH_ME_PATH
        : path === AUTH_REFRESH_PATH || path === AUTH_LOGOUT_PATH || path === AUTH_ME_PATH

      if (apiError.status === 401 && !isRetry && !isAuthEndpoint) {
        if (isAdminPath) {
          adminRefreshPromise ??= useAdminAuthStore().fetchMe().finally(() => {
            adminRefreshPromise = null
          })
          const refreshed = await adminRefreshPromise

          if (refreshed) {
            return request<T>(path, options, true)
          }

          useAdminAuthStore().clearSession()
          // navigateTo 需要 Nuxt context，在此同步呼叫點以外的 await 之後可能遺失，故用 runWithContext 還原。
          // SSR 時 navigateTo 回傳的 redirect signal 必須被 return 出去才會真正觸發導頁，否則會被下面的 throw 蓋掉。
          // 帶上 redirect query，讓登入成功後導回原本要訪問的頁面（與 admin-auth middleware 行為一致）。
          return await nuxtApp.runWithContext(() =>
            navigateTo(`/admin/login?redirect=${encodeURIComponent(route.fullPath)}`)) as T
        } else {
          refreshPromise ??= useAuthStore().fetchMe().finally(() => {
            refreshPromise = null
          })
          const refreshed = await refreshPromise

          if (refreshed) {
            return request<T>(path, options, true)
          }

          useAuthStore().clearSession()
          // 帶上 redirect query，讓登入成功後導回原本要訪問的頁面（與 auth middleware 行為一致）。
          return await nuxtApp.runWithContext(() =>
            navigateTo(`/login?redirect=${encodeURIComponent(route.fullPath)}`)) as T
        }
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
      request<T>(path, { method: 'DELETE' })
  }
}
