# Todo

## 題庫詳情頁 (`/library/[id]`) — 待補後端 API 的功能

2026-07-12 起草，2026-07-23 更新現況：`app/pages/library/[id].vue` 原本用假資料（mock）呈現以下功能，因當時後端完全沒有對應 API，已先從 UI 移除。以下大部分已補上 API 並重新接回前端。

已完成（後端 API + 前端串接皆已上線）：

- **收藏/最愛（favorite/bookmark）**：`GET/PUT/DELETE /api/v1/quizzes/{id}/favorite`，`app/services/quiz/client.ts` 的 `fetchFavoriteStatus/addFavorite/removeFavorite`。
- **追蹤創作者（follow creator）**：`GET /api/v1/creators/{id}`、`PUT/DELETE /api/v1/creators/{id}/follow`，`app/services/creator/client.ts` + `app/stores/creator.ts`。
- **留言/評論（comments）**：`GET/POST /api/v1/quizzes/{id}/comments`（僅新增/查詢，無編輯/刪除，查詢無分頁）。
- **創作者統計**：已併入 `GET /api/v1/creators/{id}` 回傳的 `CreatorProfileDto`（`quizCount`、`challengeCount`），非獨立 endpoint。

尚未實作：

- **相關題庫推薦（related quizzes）**：目前只有一般題庫列表 API（`GET /api/v1/quizzes`，支援 `tag`/`search`/`ownerId`），沒有 related/similar 專屬邏輯，頁面上也還沒有對應 UI 區塊。
- **分享功能（share）**：無分享連結產生或分享次數統計相關 API。

已確認存在、可正常使用真實資料的欄位：`QuizDto`/`QuizSummaryDto` 的 `challengeCount`、`passRate`、`creatorName`、`questions` 等。
