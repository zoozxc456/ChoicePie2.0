# 尚未接上 UI 的 API

範圍：後端這些 endpoint **都已完整實作**，前端 Pinia store 方法也已經寫好，但目前沒有任何頁面/元件呼叫它們。

所有回應皆包在共用信封裡：

```ts
interface ApiEnvelope<T> {
  code: string
  status: boolean
  data: T | null
  message: string
}
```

`useApi()`（[app/composables/useApi.ts](../app/composables/useApi.ts)）已自動解開信封，store 方法拿到的就是 `T`。REST 請求經由 Nuxt server BFF（`server/api/v1/**`）轉發給後端。

---

## 目前狀態：清單已清空

2026-10-16 逐一比對後端所有 controller 的 endpoint 與前端 `app/services/**` 的呼叫，**沒有任何後端 REST endpoint 缺少前端呼叫者**
（含 `AccountController`、`MeController`、`CreatorsController`、各 `Admin*Controller`、`UploadsController`）。
`DELETE /api/v1/account` 因 `useApi.del` 不支援 request body，由前端透過 BFF 的 `POST /api/v1/account/delete` 轉成後端 DELETE 呼叫。

以下功能原本列於本文件，現已全部接上 UI：

- **題庫管理**（更新/刪除題庫、新增/更新/刪除題目、發布/取消發布/封存）——
  `38cbce8 feat(library): fetch quiz detail from real API and add publish/unpublish`
  接上 `app/pages/library/mine/index.vue`、`app/pages/library/mine/[id]/edit.vue`。
- **單人作答流程**（開始作答、提交答案、完成結算、查詢結果、作答預覽）——
  `60275c0 feat(library): add My Quizzes management and quiz attempt result page`
  接上 `app/pages/library/[id].vue`（「📝 單人練習」進入點）、`app/pages/attempt/[id].vue`、
  `app/components/attempt/AttemptResult.vue`。

---

## 刻意排除（非本清單範圍）

- **Admin Auth**（`AdminAuthController`）：原先暫不接，現已接上（`app/services/admin/auth.ts`、`app/stores/adminAuth.ts`、`/admin/login`），不再屬於排除項目。
- **SignalR `GameHub`**：已接上（`useGameRoom.ts`），不屬於「未接 REST API」範圍。
- **健康檢查**（`/health/live`、`/health/ready`）：供部署探針使用，不在 `/api/v1` 之下，不需要前端呼叫。
