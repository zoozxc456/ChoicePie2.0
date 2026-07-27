import type { H3Event } from 'h3'

const MEMBER_ACCESS_COOKIE = 'cp_access_token'
const MEMBER_REFRESH_COOKIE = 'cp_refresh_token'
const ADMIN_ACCESS_COOKIE = 'cp_admin_access_token'
const ADMIN_REFRESH_COOKIE = 'cp_admin_refresh_token'

// setCookie(event, ...) 只會寫進這次 response 的 Set-Cookie header，不會回頭改寫 event 對
// 「這次 incoming request 帶了什麼 cookie」的認知（getCookie 讀的是
// event.node.req.headers.cookie，setCookie 寫的是 event.node.res，兩者互不影響）。
// 若同一次 SSR 內先 401 → refresh 換到新 access token，緊接著的 retry 若是透過
// useRequestFetch() 打自己的 server route，Nitro 對每次呼叫都會建立全新的 H3Event
// （event.context 不共享），無法用 event.context 暫存新 token 讓 retry 讀到——
// 新 token 改由呼叫端（useApi.ts）顯式帶在 x-fresh-access-token header 上，
// 見 server/api/v1/[...path].ts。
const REFRESH_TOKEN_MAX_AGE_SECONDS = 60 * 60 * 24 * 30

interface SessionTokens {
  accessToken: string
  refreshToken: string
}

const cookieOptions = (maxAge: number) => ({
  httpOnly: true,
  // dev server 本身就是 https（自簽憑證，見 nuxt.config.ts devServer.https），沒有理由用非 secure
  // cookie；且 secure cookie 只能在 https 頁面被送出/接收，若這裡跟著 import.meta.dev 關掉，
  // dev 環境下寫入的 cookie 反而會因為 Secure 屬性缺失而被部分瀏覽器安全策略拒絕或降級處理。
  secure: true,
  sameSite: 'lax' as const,
  path: '/',
  maxAge
})

// access token cookie 的 Max-Age 刻意跟 refresh token 一樣長，不跟著 JWT 實際壽命（60 秒）走：
// cookie 是 httpOnly，前端 JS 本來就讀不到內容，拉長壽命沒有額外的安全風險。JWT 是否真的過期
// 完全交給後端驗證 exp claim（401）判斷，由 useApi.ts 的 401 → refresh 流程被動處理；
// 如果讓 cookie 自己在 60 秒後被瀏覽器主動清除，會在「JWT 其實還沒過期，只是 cookie 沒了」
// 這個窗口內，讓並發請求各自誤判成 401 並各自觸發 refresh，一次性 refresh token 被重複
// 送出而互相打架，導致整個 session 被清空。
const setSession = (
  event: H3Event,
  { accessToken, refreshToken }: SessionTokens,
  accessCookieName: string,
  refreshCookieName: string
) => {
  setCookie(event, accessCookieName, accessToken, cookieOptions(REFRESH_TOKEN_MAX_AGE_SECONDS))
  setCookie(event, refreshCookieName, refreshToken, cookieOptions(REFRESH_TOKEN_MAX_AGE_SECONDS))
}

const clearSession = (event: H3Event, accessCookieName: string, refreshCookieName: string) => {
  deleteCookie(event, accessCookieName, { path: '/' })
  deleteCookie(event, refreshCookieName, { path: '/' })
}

export const getMemberAccessToken = (event: H3Event) => getCookie(event, MEMBER_ACCESS_COOKIE)
export const getMemberRefreshToken = (event: H3Event) => getCookie(event, MEMBER_REFRESH_COOKIE)

export const setMemberSession = (event: H3Event, tokens: SessionTokens) =>
  setSession(event, tokens, MEMBER_ACCESS_COOKIE, MEMBER_REFRESH_COOKIE)

export const clearMemberSession = (event: H3Event) =>
  clearSession(event, MEMBER_ACCESS_COOKIE, MEMBER_REFRESH_COOKIE)

export const getAdminAccessToken = (event: H3Event) => getCookie(event, ADMIN_ACCESS_COOKIE)
export const getAdminRefreshToken = (event: H3Event) => getCookie(event, ADMIN_REFRESH_COOKIE)

export const setAdminSession = (event: H3Event, tokens: SessionTokens) =>
  setSession(event, tokens, ADMIN_ACCESS_COOKIE, ADMIN_REFRESH_COOKIE)

export const clearAdminSession = (event: H3Event) =>
  clearSession(event, ADMIN_ACCESS_COOKIE, ADMIN_REFRESH_COOKIE)
