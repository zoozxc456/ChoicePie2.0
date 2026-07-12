# Todo

## 題庫詳情頁 (`/library/[id]`) — 待補後端 API 的功能

2026-07-12：`app/pages/library/[id].vue` 原本用假資料（mock）呈現以下功能，因後端目前完全沒有對應 API，已先從 UI 移除。待後端補上 API 後再重新實作前端。

- **收藏/最愛（favorite/bookmark）**：需要 `POST/DELETE /api/v1/quizzes/{id}/favorite` 之類的 endpoint，並在 quiz 詳情回傳當前使用者是否已收藏。
- **追蹤創作者（follow creator）**：需要 Member 追蹤關係的 API（follow/unfollow + 是否已追蹤狀態）。
- **留言/評論（comments）**：需要 quiz 留言的 CRUD API（`GET/POST /api/v1/quizzes/{id}/comments`）。
- **創作者統計**：創作者總題庫數、累積被挑戰次數，目前只能靠 `GET /api/v1/quizzes?ownerId={id}` 前端自行加總，建議後端提供專屬的 creator-stats endpoint。
- **相關題庫推薦（related quizzes）**：目前只有一般題庫列表 API（`GET /api/v1/quizzes`，支援 `tag`/`search`/`ownerId`），沒有 related/similar 專屬邏輯。
- **分享功能（share）**：無分享連結產生或分享次數統計相關 API。

已確認存在、可正常使用真實資料的欄位：`QuizDto`/`QuizSummaryDto` 的 `challengeCount`、`passRate`、`creatorName`、`questions` 等。
