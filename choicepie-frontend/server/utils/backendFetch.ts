import type { H3Event } from 'h3'

interface BackendFetchOptions {
  method?: string
  query?: Record<string, unknown>
  body?: unknown
  /** 轉發給後端的 Authorization bearer token（member/admin access token） */
  accessToken?: string
  /** 轉發給後端的 refresh token（僅 refresh/logout 需要，後端從 X-Refresh-Token header 讀取） */
  refreshToken?: string
  /**
   * 把後端回應的 Set-Cookie 原樣轉發給瀏覽器 —— 後端在 login/refresh/logout 時仍會用
   * Response.SetAuthCookies()/ClearAuthCookies() 對自己的 access_token/refresh_token cookie
   * 做 set/rotate/clear，SignalR 是瀏覽器直連後端的 WebSocket，只認這顆 cookie（見
   * useGameRoom.ts）。BFF 與後端間是 server-to-server 呼叫，這顆 Set-Cookie 預設不會自動
   * 到達瀏覽器，必須在這裡明確轉發，否則後端 cookie 只在登入當下設一次、永遠不會被輪替，
   * 60 秒後 access token 過期 SignalR 連線就會開始 401。只有 auth 相關 route 需要這麼做。
   */
  forwardSetCookie?: boolean
}

/**
 * 呼叫真正的 .NET 後端。後端一律從 header 讀 token，不吃 cookie：
 * access token 走標準 Authorization: Bearer header（JWT 中介層既有的 fallback 行為，非本次新增）；
 * refresh token 走自訂的 X-Refresh-Token header（本次後端配合改成 header-only）。
 * Nitro 與後端是 server-to-server 呼叫，沒有共用的瀏覽器 cookie jar，所以完全不需要轉發 cookie
 * 給後端；但後端回應的 Set-Cookie 視 forwardSetCookie 決定要不要轉發回瀏覽器（見上方註解）。
 */
export const backendFetch = async <T>(event: H3Event, path: string, options: BackendFetchOptions = {}) => {
  const config = useRuntimeConfig(event)

  const headers: Record<string, string> = {}
  if (options.accessToken) headers.authorization = `Bearer ${options.accessToken}`
  if (options.refreshToken) headers['x-refresh-token'] = options.refreshToken

  const method = options.method ?? 'GET'

  // 除錯用：印出打給後端的完整 request 與收到的 response。只在 Nitro server 的 terminal
  // 輸出（不會進到瀏覽器 console），但仍含有 token 明碼，僅供本機除錯，不要留在正式環境。
  console.warn('[backendFetch] →', method, `${config.backendApiUrl}${path}`, {
    query: options.query,
    headers,
    body: options.body
  })

  const response = await $fetch.raw<T>(path, {
    baseURL: config.backendApiUrl,
    method: method as 'GET',
    query: options.query,
    body: options.body,
    headers,
    ignoreResponseError: true
  })

  console.warn('[backendFetch] ←', response.status, method, `${config.backendApiUrl}${path}`, {
    headers: Object.fromEntries(response.headers.entries()),
    body: response._data
  })

  if (options.forwardSetCookie) {
    for (const cookie of response.headers.getSetCookie()) {
      appendResponseHeader(event, 'set-cookie', cookie)
    }
  }

  return response
}

/**
 * 把後端 $fetch.raw 回應（envelope + status code）原樣轉發給呼叫端，
 * 讓 useApi.ts 既有的 401/錯誤解析邏輯不需要改動。
 */
export const relayBackendResponse = <T>(event: H3Event, response: Awaited<ReturnType<typeof backendFetch<T>>>) => {
  setResponseStatus(event, response.status, response.statusText)
  return response._data
}
