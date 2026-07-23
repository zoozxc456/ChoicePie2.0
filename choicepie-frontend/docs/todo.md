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

## 主持人賽後數據分析 (`/history/[id]`) — 待補功能

2026-07-23：`history/[id].vue` 主持人視角的逐題數據分析（答對率、每個選項被選次數）已完成並接回真實資料。

- **每題平均作答時間**：尚未實作。`GameSessionAnswerLogEntry`（`ChoicePie.Backend.Domain/Aggregates/GameSession/ValueObjects/GameSessionAnswerLogEntry.cs`）目前只有 `QuestionIndex`、`SelectedOptionIndex`、`IsCorrect`、`ScoreAwarded`，沒有任何時間戳或耗時欄位。需要：
  1. 在提交答案的即時流程（SignalR `SubmitAnswer` / GameHub）記錄作答耗時，新增 `AnsweredAtMs` 或 `DurationMs` 欄位到 `GameSessionAnswerLogEntry`
  2. `GameSessionQuestionBreakdownDto` 加上 `AverageAnswerTimeMs`（或類似欄位），`GameSessionQueryService.BuildQuestionBreakdown()` 一併計算
  3. 需要一支 EF Core migration（改動 owned type / JSON 欄位視實際儲存方式而定）
  4. 前端 `history/[id].vue` 逐題分析卡片補上平均作答時間顯示
