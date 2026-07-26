# Debug 記錄：Admin 重新整理頁面時偶發被登出

2026-07-26

## 現象

Admin 後台頁面（例如 `/admin/members`）重新整理（SSR）時，偶爾會直接被導回 `/admin/login`，
即使 session 其實還有效。

## 根本原因

專案採用 BFF 架構：瀏覽器只打自己的 Nuxt server（`server/api/**`），不直接跨網域打 `.NET`
後端；Nuxt server 再用 `backendFetch()` 代打真正後端，並用 httpOnly cookie
（`cp_admin_access_token`/`cp_admin_refresh_token`）管理 session，見
`server/utils/session.ts`、`server/utils/backendFetch.ts`。

`useApi.ts` 的 401 handler 原本的設計：access token 過期 → 呼叫 `useAdminAuthStore().fetchMe()`
（先打 `/me`，失敗才 fallback 打 `/refresh`）→ 換到新 token 後 retry 原本的請求。

`session.ts` 原本想用 `event.context` 在同一次 SSR render 內快取 refresh 換到的新 access
token，讓「retry 請求」能讀到新值而不是 incoming request 帶進來的舊 cookie：

```ts
// 已移除的舊寫法
export const getAdminAccessToken = (event: H3Event) =>
  event.context[ADMIN_ACCESS_CONTEXT_KEY] ?? getCookie(event, ADMIN_ACCESS_COOKIE)
```

**這個假設不成立。** `useApi.ts` 在 SSR 端用 `useRequestFetch()` 呼叫自己的 `server/api/**`
（例如 retry 時重打 `/admin/members`），這在 Nitro/h3 底層會建立一個全新的 `H3Event`
（`fetchWithEvent` → `createSubRequest` → `event.app.fetch()` 重新 `~request()`）。新 event
的 `.context` 是全新的空物件，**不會繼承**呼叫方 event 的 `.context`。也就是說：

1. `/me` 401 → `/refresh` 200，`setAdminSession()` 把新 token 寫進「這次 refresh 呼叫」自己
   的 event.context，同時把新 cookie 放進這次 response 的 `Set-Cookie` header。
2. 緊接著的 retry 請求是**另一個全新的 sub-request**，讀不到步驟 1 寫的 `event.context`，
   也讀不到步驟 1 的 `Set-Cookie`（那是寫給瀏覽器的，不會反映在「這次 SSR 最外層 request
   一開始帶進來的 cookie header」上）。
3. retry 因此又用舊 access token 打後端，又 401，又觸發一次 `fetchMe()` → `/refresh`，
   而這次用的 refresh token 已經是舊值（一次性輪替機制下，上一輪 refresh 已把它作廢）。
4. 後端判定 refresh token invalid（`InvalidRefreshTokenException`）→ 401 → 前端清除 session、
   導向登入頁。

用 debug log 追蹤可以看到同一次 SSR 內反覆出現 `/me` 401 → `/refresh` 200 的循環，且
`accessToken exp claim: undefined`（代表 sub-request 的 cookie header 裡根本沒有
`cp_admin_access_token`，因為它是這次瀏覽器 request 一開始的舊 cookie 快照）。

## 為什麼不能簡單地「改用最新 cookie」

Sub-request 的 cookie 來源是「這次瀏覽器 request 一開始帶進來的 cookie header」的快照，
在同一次 SSR render 過程中不會因為中途 `setCookie(event, ...)` 而更新——`setCookie` 寫的是
outgoing response 的 `Set-Cookie`，`getCookie` 讀的是 incoming request 的 `cookie` header，
兩者是不同方向、互不影響的資料。

## 考慮過的替代方案：AsyncLocalStorage

曾評估用 Node 原生 `AsyncLocalStorage` 在最外層包住整個 request 生命週期（讓所有 sub-request
共用同一份 store），理論上能讓 refresh 換到的新 token 對之後所有 sub-request 都可見。

放棄原因：h3 的 middleware 鏈是靠一個同步 `for` 迴圈推進（`await layer.handler(event)`），
`AsyncLocalStorage.run()` 只能涵蓋「在 run() 呼叫期間新建立的 async 資源」，middleware
handler 一旦 `return`，h3 迴圈的下一輪 `await` 是呼叫端既有的 async 上下文延續，不會繼承
`als.run()` 的 store。要真正涵蓋後續整條鏈，必須 monkey-patch Nitro 的 request entry point
（`nitroApp.h3App.handler`/`localCall`），這不是公開 API，跨版本升級容易壞掉，風險與效益不成
比例，故不採用。

## 採用的修法：顯式傳遞新 token

- `server/api/v1/admin/auth/refresh.post.ts`：不再把回應的 `data` 覆寫成扁平的 `adminUser`，
  保留完整的 `{ adminUser, accessToken, refreshToken }`（對應型別
  `AdminRefreshResultDto`，`app/types/api.ts`），讓呼叫端能拿到新 `accessToken`。
- `app/stores/adminAuth.ts`：`fetchMe()` 回傳型別從 `Promise<boolean>` 改為
  `Promise<{ success: boolean, accessToken?: string }>`，refresh 成功時把新 token 一併帶出。
- `app/composables/useApi.ts`：401 handler 呼叫 `fetchMe()` 拿到新 `accessToken` 後，retry
  時透過自訂 header `x-fresh-access-token` 顯式帶上。
- `server/api/v1/[...path].ts`：優先讀取 `x-fresh-access-token`，沒有才 fallback 讀
  `getAdminAccessToken(event)`（cookie）。
- `server/utils/session.ts`：移除已確認無效的 `event.context` 快取邏輯。

## 已知取捨（未完全解決）

Admin middleware（`app/middleware/admin-auth.ts`）在每次進入受保護頁面時都會呼叫
`fetchMe()`。若 middleware 這次呼叫觸發了 refresh（access token 已過期），換到的新 token
只存在於 middleware 自己那次 sub-request 裡；緊接著頁面 setup 第一次呼叫業務 API（例如
`fetchMembers()`）是**全新的呼叫**、不是 retry，仍然會用 incoming request 的舊 cookie
發送、仍會 401，需要再走一次「401 → fetchMe() → refresh → 帶 x-fresh-access-token retry」
才能成功。

也就是說：access token 剛好在頁面重新整理時過期，會多花一輪 401 → refresh → retry 才能拿到
資料，但不會再導致誤判登出（因為這次 refresh 使用的 refresh token 是這次 SSR 一開始的原始
cookie 值，尚未被消耗過）。

真正徹底解決需要能跨 sub-request 共享 request-scoped 狀態的機制（見上方 AsyncLocalStorage
方案），評估後決定先接受這個較小的效能代價，不引入 monkey-patch 風險。

## 相關檔案

- `app/composables/useApi.ts`
- `app/stores/adminAuth.ts`
- `app/middleware/admin-auth.ts`
- `app/pages/admin/login.vue`
- `app/services/admin/auth.ts`
- `app/types/api.ts`（`AdminRefreshResultDto`）
- `server/api/v1/admin/auth/refresh.post.ts`
- `server/api/v1/[...path].ts`
- `server/utils/session.ts`
