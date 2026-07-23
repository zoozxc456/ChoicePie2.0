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

## 單人練習 (`/attempt/[id]`) — 續作限制決策已完成，其餘待補功能

2026-07-24 新增：核心作答流程（開始挑戰 → 逐題作答 → 完成計分 → 結果頁逐題檢討）已完整串接前後端，
可正常遊玩，但以下功能尚未實作：

- **計時功能**：完全沒有。`QuizAttempt.cs` 有記錄 `StartedAt`/`CompletedAt`，但前端沒有倒數 UI，
  後端也沒有任何時限機制或逾時計分邏輯。
- **無作答歷史記錄**：沒有任何頁面或 API 可以列出會員在某份題庫的過去挑戰紀錄（分數、時間）。

2026-07-24：**中途離開無法續作 / 無重複挑戰限制**已一併處理完成，決策為「同一題庫同一會員最多只有一個
進行中的 attempt，重新開始會續用而非產生新記錄；已完成的挑戰不限制次數」（練習性質題庫，允許重複挑戰
以求進步，但避免中途離開造成的 `InProgress` 記錄無限累積）。實作：

- 後端新增 `QuizAttemptInProgressByQuizAndMemberSpecification`
  （`ChoicePie.Backend.Domain/Aggregates/QuizAttempt/Specifications/`），`StartQuizAttemptCommandHandler`
  改為先查詢是否已有進行中的 attempt，若有則直接複用（回傳同一個 `attemptId` 與完整題目資料），
  沒有才建立新的 `QuizAttempt`。
- 修正一併發現的安全性問題：`GetQuizAttemptByIdQuery`（`GET /api/v1/quiz-attempts/{id}`）先前對進行中的
  attempt 也會回傳每題的 `CorrectOptionIndex`/`IsCorrect`/`Explanation`，等於作答途中就能從 API 回應看到正確答案。
  現在 `QuizAttemptQueryService.GetByIdAsync` 會在 `Status == InProgress` 時將這三個欄位遮蔽為 `null`/`false`
  （`QuizAttemptAnswerResultDto` 的 `CorrectOptionIndex`/`Explanation` 因此改為可為 `null`）。
- 前端 `app/pages/attempt/[id].vue`：直接連到 `/attempt/[id]`（重新整理或分享連結）且 store 內沒有對應
  `currentAttempt` 時，改為呼叫 `fetchAttemptById` 判斷是否為進行中的 attempt，若是則呼叫 `startAttempt`
  取回完整題目資料以續作（同一 attempt id，不會建立新記錄），並依已作答數量跳到對應題目；
  `quizAttempt.ts` store 的 `fetchAttemptById` 也只在 `completedAt` 存在時才寫入 `result`，避免進行中的
  attempt 誤觸發結果頁畫面。

## 題庫檢舉功能 — 待規劃，需從零開始

2026-07-24 新增：完全沒有實作，前後端皆無任何相關程式碼。`app/pages/library/[id].vue` 沒有檢舉按鈕，
後端沒有 Report/Flag 相關的 aggregate、command、query、controller 或 migration。

需要規劃：檢舉原因分類、檢舉後的處理流程（進入 Admin 審核佇列？直接下架？）、
與已規劃中的「Admin：題庫下架 / 會員管理」是否共用同一套審核機制、前端檢舉按鈕與表單 UI。

## 分享連結功能補強 — 已完成

2026-07-24：決定範圍為「分享代碼但不做真正短網址」——沿用 `/library/{id}` 原頁面網址，
不另外產生短碼/redirect endpoint。已完成：

- **分享來源追蹤代碼**：所有分享連結改為附加 `?ref={channel}` 查詢參數（`copy`/`line`/`facebook`/`x`），
  可從網址判斷使用者是從哪個管道點進來，但不落地成後端分析資料表——後端 `RecordQuizShareCommandHandler`
  維持原本的 `Quiz.ShareCount` 純計數，不記錄管道。
- **社群分享整合**：題庫詳情頁的分享按鈕改為下拉選單（`app/components/library/ShareMenu.vue`，
  使用 `UDropdownMenu`），提供複製連結／分享到 Line／分享到 Facebook／分享到 X 四個選項，
  各平台使用官方 web share intent 網址（`social-plugins.line.me/lineit/share`、
  `facebook.com/sharer/sharer.php`、`twitter.com/intent/tweet`）開新視窗分享。圖示沿用專案既有的
  `@iconify-json/lucide`（未安裝 `simple-icons` 之類的品牌圖示集，故用近似的 lucide 圖示代替）。
- **OG meta 標籤**：`app/pages/library/[id].vue` 新增 `useSeoMeta`（`title`/`ogTitle`/`description`/
  `ogDescription`/`ogType`），分享到聊天軟體或社群平台時會顯示題庫標題與描述。**沒有加上 `ogImage`**——
  題庫本身沒有真正的封面圖片欄位（只有 `coverEmoji` + `coverGradient` CSS 漸層，不是圖片網址），
  專案目前也只有一個 favicon，沒有符合 OG 建議尺寸（1200×630）的靜態圖，之後若要做圖片預覽需要另外
  設計（例如伺服器端產生的動態卡片圖）。

`app/stores/quiz.ts` 的 `recordShare` action 與後端 API 皆未變動。
