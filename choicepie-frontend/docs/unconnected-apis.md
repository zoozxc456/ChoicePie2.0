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

`useApi()`（[app/composables/useApi.ts](../app/composables/useApi.ts)）已自動解開信封，store 方法拿到的就是 `T`。

---

## 目前狀態：清單已清空

以下功能原本列於本文件，現已全部接上 UI：

- **題庫管理**（更新/刪除題庫、新增/更新/刪除題目、發布/取消發布/封存）——
  `38cbce8 feat(library): fetch quiz detail from real API and add publish/unpublish`
  接上 `app/pages/library/mine/index.vue`、`app/pages/library/mine/[id]/edit.vue`。
- **單人作答流程**（開始作答、提交答案、完成結算、查詢結果、作答預覽）——
  `60275c0 feat(library): add My Quizzes management and quiz attempt result page`
  接上 `app/pages/library/[id].vue`（「📝 單人練習」進入點）、`app/pages/attempt/[id].vue`、
  `app/components/attempt/AttemptResult.vue`。

已知瑕疵（非「未接 API」，而是既有串接的行為缺口，記錄於此以便追蹤）：

- `app/pages/attempt/[id].vue` 重新整理或直接開啟連結時，不會呼叫
  `quizAttemptStore.fetchAttemptById(attemptId)` 復原狀態，導致頁面空白（`currentAttempt`/`result`
  皆為 `null`）。且 `fetchAttemptById` 回傳的是 `QuizAttemptResultDto`（結算後的完整結果），並非
  `StartAttemptResultDto`，本身也無法用來復原「作答中」的題目進度，只能復原「已完成」的結果畫面。

---

## 刻意排除（非本清單範圍）

- **Admin Auth**（`AdminAuthController`，3 個 endpoint）：依先前決議暫不接。
- **SignalR `GameHub`**：已於另一輪工作接上（`useGameRoom.ts`），不屬於「未接 REST API」範圍。
