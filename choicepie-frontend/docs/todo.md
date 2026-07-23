# Todo

## 題庫詳情頁 (`/library/[id]`) — 待補後端 API 的功能

2026-07-12 起草，2026-07-23 更新現況：`app/pages/library/[id].vue` 原本用假資料（mock）呈現以下功能，因當時後端完全沒有對應 API，已先從 UI 移除。全部已補上 API 並重新接回前端。

已完成（後端 API + 前端串接皆已上線）：

- **收藏/最愛（favorite/bookmark）**：`GET/PUT/DELETE /api/v1/quizzes/{id}/favorite`，`app/services/quiz/client.ts` 的 `fetchFavoriteStatus/addFavorite/removeFavorite`。
- **追蹤創作者（follow creator）**：`GET /api/v1/creators/{id}`、`PUT/DELETE /api/v1/creators/{id}/follow`，`app/services/creator/client.ts` + `app/stores/creator.ts`。
- **留言/評論（comments）**：`GET/POST /api/v1/quizzes/{id}/comments`（僅新增/查詢，無編輯/刪除，查詢無分頁）。
- **創作者統計**：已併入 `GET /api/v1/creators/{id}` 回傳的 `CreatorProfileDto`（`quizCount`、`challengeCount`），非獨立 endpoint。
- **相關題庫推薦（related quizzes）**：`GET /api/v1/quizzes/{id}/related`，依共同標籤比對已發布題庫並依挑戰次數排序，`app/services/quiz/client.ts` 的 `fetchRelatedQuizzes`。
- **分享功能（share）**：`POST /api/v1/quizzes/{id}/share` 累加 `Quiz.ShareCount`（無需登入），前端點擊分享按鈕會複製題庫連結到剪貼簿並呼叫此 API，`app/services/quiz/client.ts` 的 `recordShare`。目前僅記錄次數，未產生短連結或自訂分享頁。

已確認存在、可正常使用真實資料的欄位：`QuizDto`/`QuizSummaryDto` 的 `challengeCount`、`passRate`、`creatorName`、`questions` 等。

## 主持人賽後數據分析 (`/history/[id]`) — 已完成

2026-07-23：`history/[id].vue` 主持人視角的逐題數據分析（答對率、每個選項被選次數）已完成並接回真實資料。

2026-07-24：**每題平均作答時間**已實作並接回前端。後端 `GameRoom.SubmitAnswer` 原本就會計算 `elapsed`（作答耗時）
但只用於計分後即丟棄，現已將其以毫秒形式（`AnswerTimeMs`）一路帶入 `PlayerAnswer` → `GameSessionAnswerLogEntry`
（`ChoicePie.Backend.Domain/Aggregates/GameSession/ValueObjects/GameSessionAnswerLogEntry.cs`，新增欄位帶預設值 `0`
以相容既有 jsonb 資料，無需 EF migration）。`GameSessionQuestionBreakdownDto` 新增 `AverageAnswerTimeMs`（`double?`，
無人作答時為 `null`），由 `GameSessionQueryService.BuildQuestionBreakdown()` 計算平均值。前端逐題分析卡片
（`app/pages/history/[id].vue`）在答對率旁補上「平均作答 X 秒」顯示。

## AI 出題 — 只差真實 LLM provider

2026-07-24 更新：前端頁面（`library/new/ai.vue`）、額度控管、controller、DTO 端到端流程都已接好，**但後端 `PlaceholderQuizGenerationService`
（`ChoicePie.Backend.Infrastructure/ExternalServices/Quizzes/PlaceholderQuizGenerationService.cs`）目前只回傳寫死的
「Placeholder question N」，沒有呼叫任何真實 LLM**（`TokensUsed` 也固定回 0）。需要：
1. 選定並串接真實 LLM provider，取代 `PlaceholderQuizGenerationService` 的實作
2. 確認現有輸入介面（主題/關鍵字/難度）、產生題目後的審核與編輯流程是否足夠，不需要另外規劃
3. 成本控管（token 用量、額度限制）——確認現有額度邏輯是否已涵蓋真實 API 呼叫成本

## Google 註冊登入 — 待規劃

尚未開始。目前 `login.vue`/`sign-in/index.vue` 僅支援 email/password 登入，`app/stores/auth.ts` 的
`loginWithGoogle()` 是一行 `// TODO: redirect to /api/auth/google` 的前端空殼，後端完全沒有對應的 OAuth route。需要規劃：
後端串接 Google OAuth（取得 email 建立/綁定既有帳號）、前端登入頁加上「使用 Google 繼續」按鈕、既有 email 帳號與 Google 帳號的綁定/合併規則。

## 留言功能補強 — 編輯/刪除已完成，分頁仍待規劃

2026-07-24：留言的編輯/刪除已完成並接回前端。後端新增 `Comment.UpdateText()`/`EnsureModifiableBy()` 網域方法
（僅留言作者可編輯/刪除，比照 `Quiz.EnsureModifiableBy` 的模式，違反時丟出 `CommentForbiddenException`），
新增 `UpdateCommentCommand`/`DeleteCommentCommand` 與對應 handler，controller 補上
`PUT/DELETE /api/v1/quizzes/{id}/comments/{commentId}`（`[Authorize(Policy = "MemberOnly")]`）。刪除採沿用
`AuditableEntity.Delete(userId)` 的軟刪除。前端將留言區塊從 `library/[id].vue` 抽出為
`app/components/library/CommentList.vue` + `CommentItem.vue`，僅留言本人（`auth.user.id === comment.userId`）
會看到編輯/刪除按鈕，`app/stores/quiz.ts` 新增 `updateComment`/`deleteComment` actions。

**仍待規劃**：查詢目前仍無分頁（`ListCommentsByQuizIdQuery` 回傳完整 `IReadOnlyList<CommentDto>`），留言數量多時
需要補上分頁或無限捲動。

## Admin：題庫下架 / 會員管理 — 待規劃，需從零開始

2026-07-24 新增：Admin 面板目前只完成登入 + 儀表板空殼（見上方 Admin 相關 commit），題庫下架與會員管理是刻意
留到下一輪的功能。目前後端完全沒有相關支援：

- 沒有 Report/Moderation/Takedown/Ban/Suspend 相關的 aggregate、repository、command、query、controller
- `Quiz.Status`（`Draft/Published/Archived/Deleted`）沒有 admin 觸發的狀態轉換機制，也沒有會員停權概念
- 前端 admin 除了登入頁與空殼儀表板（`app/pages/admin/index.vue`）外沒有任何管理頁面

需要規劃：資料模型（下架原因？停權期限或永久？誰能複權？）、command/query/controller 一整層、對應的 admin UI（題庫列表+下架操作、會員列表+停權操作）。

## 忘記密碼 / Email 驗證 / Email 通知 — 待規劃，需先補寄信基礎設施

2026-07-24 新增：三項功能前後端皆完全沒有實作，且共同卡在同一個前提——**後端目前沒有任何寄信功能**
（搜尋 SendGrid/SmtpClient/MailKit/IEmailSender 等全部零命中，只有 `Email` value object 負責驗證/儲存信箱格式，
沒有任何 outbound mail transport）。要做任何一項都必須先補上寄信基礎設施。

- **忘記密碼/重設密碼**：`AuthController`/`app/services/auth.ts` 只有 register/login/refresh/logout，
  沒有 `ForgotPasswordCommand`/`ResetPasswordCommand`，前端也沒有忘記密碼頁面或連結。
- **Email 驗證**：`AuthAccount.cs`/`AdminAuthAccount.cs` 有 `IsVerified` 欄位，但建立時永遠寫死 `false`，
  全專案沒有任何地方會把它改成 `true`——是個沒有驅動邏輯的死欄位（`MemberDto`/`AdminUserDto` 只是原樣輸出）。
  沒有 `VerifyEmailCommand`、驗證 token 產生、或驗證頁面。
  沒有寄信邏輯，也沒有「請至信箱完成驗證」之類的前端提示/i18n 文案。

需要規劃：選定寄信服務（SendGrid/SMTP 等）、忘記密碼的 token 產生與過期機制、email 驗證流程與觸發時機
（註冊時自動寄送？）、對應前端頁面（忘記密碼表單、重設密碼表單、「請查收信箱」提示頁）。

## 單人練習 (`/attempt/[id]`) — 待補功能

2026-07-24 新增：核心作答流程（開始挑戰 → 逐題作答 → 完成計分 → 結果頁逐題檢討）已完整串接前後端，
可正常遊玩，但以下功能尚未實作：

- **計時功能**：完全沒有。`QuizAttempt.cs` 有記錄 `StartedAt`/`CompletedAt`，但前端沒有倒數 UI，
  後端也沒有任何時限機制或逾時計分邏輯。
- **中途離開無法續作**：作答進度只存在前端本地 `ref`，重新整理頁面即遺失。後端 `GetQuizAttemptByIdQuery`
  對 `Status == InProgress` 的 attempt 沒有回傳「目前作答到第幾題/已選答案」的可續作狀態，
  `app/pages/attempt/[id].vue` 對這種情況直接 fall through 到找不到頁面。需要規劃續作用的 DTO 與前端邏輯。
- **無作答歷史記錄**：沒有任何頁面或 API 可以列出會員在某份題庫的過去挑戰紀錄（分數、時間）。
- **無重複挑戰限制**：`StartQuizAttemptCommandHandler` 沒有檢查是否已挑戰過，同一份題庫可無限次重新開始，
  每次都建立獨立的 `QuizAttempt` 記錄。需要規劃是否要限制次數/加上冷卻時間，或維持現況為刻意設計。

## 題庫檢舉功能 — 待規劃，需從零開始

2026-07-24 新增：完全沒有實作，前後端皆無任何相關程式碼。`app/pages/library/[id].vue` 沒有檢舉按鈕，
後端沒有 Report/Flag 相關的 aggregate、command、query、controller 或 migration。

需要規劃：檢舉原因分類、檢舉後的處理流程（進入 Admin 審核佇列？直接下架？）、
與已規劃中的「Admin：題庫下架 / 會員管理」是否共用同一套審核機制、前端檢舉按鈕與表單 UI。

## 分享連結功能補強 — 待規劃

2026-07-24 新增：目前分享功能（見上方題庫詳情頁段落）只是複製題庫本身的頁面網址並累加 `Quiz.ShareCount`
（`QuizzesController.cs` `POST /api/v1/quizzes/{id}/share` → `RecordQuizShareCommandHandler`），沒有：

- **獨立的短連結/分享專用連結**：目前直接重用 `/library/{id}` 原頁面網址，沒有產生短連結或可追蹤的分享代碼。
- **社群分享整合**：沒有分享到 Line/Facebook/X 等平台的預填文字/連結按鈕，只有一個複製到剪貼簿的按鈕。
- **連結預覽 OG meta 標籤**：`app/pages/library/*.vue` 與 `nuxt.config.ts` 都沒有設定 `useSeoMeta`/`useHead`/
  Open Graph 標籤，分享到聊天軟體或社群平台時不會有自訂標題/圖片/描述預覽。

需要規劃：是否要產生短連結（含追蹤來源用的分享代碼）、社群分享按鈕要接哪些平台、
題庫詳情頁補上 OG meta 標籤（標題、描述、封面圖）。
