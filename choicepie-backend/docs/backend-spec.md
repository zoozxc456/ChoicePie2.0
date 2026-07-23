# ChoicePie 後端規格文件

> 依據 `choicepie-frontend` 現有頁面、Pinia store、型別定義與 mock 資料反推出的後端資料模型與 API 規格。
> 前端目前所有資料皆為 mock（`app/mocks/*.ts`）或直接寫死在頁面上，尚未串接真實後端；本文件目的是把「前端已經預期會有」的資料形狀與行為整理成後端可以直接對齊的規格。

## 0. 產品概觀

ChoicePie（🥧）是一個即時多人搶答遊戲平台（類似 Kahoot）：

- **Host（主持人）**：需登入帳號，建立題庫（手動輸入或 AI 生成），開房間、控制遊戲節奏。
- **Player（玩家）**：不需帳號，輸入房間碼 + 暱稱即可加入，透過手機作答；也可搭配大螢幕（Bigscreen）投影。

技術線索：
- 前端用 `@microsoft/signalr` 連線 `/api/gamehub`，代表即時對戰功能預期由 **SignalR Hub**（ASP.NET Core）提供。
- 一般 CRUD 用 `$fetch` 呼叫 `/api/...` REST 端點。
- 目前前端未設定 `NUXT_PUBLIC_API_BASE`，API 呼叫皆為相對路徑，代表後端與前端可能同源部署，或中間會加一層 BFF/proxy。

---

## 1. 資料模型（Data Models）

### 1.1 User（使用者 / Host 帳號）

來源：`app/types/user.ts`、`app/pages/sign-in/index.vue`、`app/pages/login.vue`、`app/middleware/auth.ts`

| 欄位 | 型別 | 說明 |
|---|---|---|
| id | string | 使用者 ID |
| email | string | 登入信箱，需唯一 |
| name | string | 顯示名稱，註冊時 2–20 字 |
| avatar | string? | 頭像 URL，可選 |
| isVerified | boolean | 是否已驗證（email 驗證或 OAuth 帳號） |
| createdAt | string (ISO datetime) | 建立時間 |

補充：
- 密碼欄位不會出現在回傳的 `User` 物件中（僅後端內部使用雜湊儲存）。
- 註冊表單另外收集 `password`（≥8 碼）與 `confirmPassword`（前端驗證兩者相同，後端仍需驗證一次）。
- **Player 沒有帳號**：只在加入房間時提供 `nickname`，透過 SignalR `connectionId` 識別，不對應任何 `User` 紀錄。這是重要的不對稱設計——只有 Host 需要 `User` 實體。

### 1.2 Quiz（題庫）

來源：`app/types/quiz.ts`、`app/mocks/quiz.ts`

```ts
type Difficulty = 'beginner' | 'intermediate' | 'expert'

interface Question {
  id: string
  text: string
  options: string[]       // 目前 UI 固定 4 個選項
  answerIndex: number      // 正確答案的 index
  explanation: string      // 公布答案後顯示的解析
}

interface Quiz {
  id: string
  title: string
  description?: string
  coverEmoji: string           // 封面 emoji，例如 '⚓'
  coverGradient: string        // CSS gradient 字串，例如 'background: linear-gradient(135deg,#0f3460,#533483);'
  difficulty: Difficulty
  questionCount: number        // = questions.length，可為 computed 欄位
  challengeCount: number       // 累計被拿去開過幾次房間 / 挑戰次數
  passRate: number             // 0–100，歷史平均及格率
  creatorId: string
  creatorName: string
  creatorAvatar?: string
  questions: Question[]
  tags: string[]
  isPublic: boolean
  createdAt: string
  updatedAt: string
}
```

備註：
- `questionCount`、`challengeCount`、`passRate` 是「衍生統計欄位」，應由後端根據歷史遊戲紀錄計算或定期更新，而非使用者輸入。
- Library 列表頁的 tag 篩選目前是前端寫死的清單（`全部/Kubernetes/React/AWS/SQL/TypeScript/Go/System Design`），建議後端提供「取得所有已使用 tag」的端點，或至少讓 tag 為自由文字、前端改為動態抓取。

### 1.3 Comment（題庫留言）— 前端已有 UI 但尚未定義型別

來源：`app/pages/library/[id].vue`（第 364–373 行，目前為純前端本地 state，未串 API）

```ts
interface Comment {
  id: string
  quizId: string
  userId: string
  userName: string
  userAvatarInitial?: string   // UI 目前用名字首字母當頭像
  text: string
  createdAt: string
}
```

對應 API：`GET /api/quizzes/{id}/comments`、`POST /api/quizzes/{id}/comments`（詳見 3.6）。

### 1.4 Favorite / Follow — 前端已有 UI 但尚未定義型別

來源：`app/pages/library/[id].vue`（`isFavorite`、`isFollowing`，目前僅為前端 local state，重新整理即消失）

- **QuizFavorite**：`{ userId, quizId, createdAt }` — 使用者收藏的題庫。
- **CreatorFollow**：`{ followerId, creatorId, createdAt }` — 使用者追蹤的出題者。

這兩者目前完全沒有持久化，屬於「UI 已就緒、後端尚未存在」的功能，建議列入下一階段 API。

### 1.5 GameRoom（遊戲房間，即時、短生命週期）

來源：`app/types/gameRoom.ts`、`app/stores/game.ts`

```ts
type RoomStatus = 'waiting' | 'playing' | 'ended'
type GamePhase = 'idle' | 'waiting' | 'question' | 'result' | 'ended'

interface Player {
  connectionId: string       // SignalR connection id，房間內識別玩家的 key
  nickname: string
  score: number
  rank: number
  hasAnswered: boolean
  selectedOptionIndex?: number  // 僅 Host 視角可見，公布答案前不可推給其他玩家
}

interface GameRoom {
  roomCode: string           // 6 碼房號，大寫英數
  quizId?: string            // 若使用題庫建立則有值；手動/AI 出題後直接建房也可能為 undefined
  quizTitle: string
  status: RoomStatus
  players: Player[]
  currentQuestionIndex: number
  totalQuestions: number
  hostConnectionId: string
}
```

生命週期規則（來自 `how-to-use` 頁 FAQ 文案，`i18n/locales/zh-TW.json`）：
- 房間建立後 **24 小時**內有效可玩。
- 遊戲結束後進入唯讀狀態（仍可查看題目與排名）。
- **48 小時**後自動清除（建議用排程 job 或 TTL 機制實作）。
- MVP 版本單房間上限 **30 人**同時在線。

計分規則（來自 `how-to-use` 頁，`i18n/locales/zh-TW.json:376-385`）：

| 作答時間 | 得分 |
|---|---|
| 0–3 秒 | 1000 |
| 3–6 秒 | 800 |
| 6–10 秒 | 600 |
| 10 秒以上（含逾時前） | 400 |
| 答錯 | 0 |

- 每題預設時限 20 秒；建立房間時可選 10 / 20 / 30 / 60 秒（`library/[id].vue` 的開房 modal）。
- 這套計分邏輯必須放在後端（SignalR Hub 收到 `SubmitAnswer` 時計算），不可信任前端回傳的分數。

### 1.6 HostedGameSummary / PlayedGameSummary（遊戲歷史紀錄）

來源：`app/types/gameRoom.ts`、`app/mocks/history.ts`

```ts
interface RankEntry {
  rank: number
  nickname: string
  score: number
  delta?: number   // 該題得分增量，僅即時對局中使用
}

// Host 視角的一場歷史遊戲摘要
interface HostedGameSummary {
  id: string
  quizId: string
  quizTitle: string
  coverEmoji: string
  coverGradient: string
  playerCount: number
  questionCount: number
  topPlayerName: string
  topPlayerScore: number
  playedAt: string
  rankings: RankEntry[]
}

// Player 答錯題目的複習資料
interface WrongAnswerReview {
  questionText: string
  options: string[]
  myAnswerIndex: number
  correctAnswerIndex: number
  explanation: string
}

// Player 視角的一場歷史遊戲摘要
interface PlayedGameSummary {
  id: string
  quizTitle: string
  coverEmoji: string
  coverGradient: string
  playerCount: number
  questionCount: number
  myRank: number
  myScore: number
  playedAt: string
  rankings: RankEntry[]
  wrongAnswers: WrongAnswerReview[]
}
```

備註：
- 這代表後端需要一張「已結束遊戲場次」的持久化資料表（`GameSession` / `GameHistory`），與短生命週期的 `GameRoom`（即時狀態）分開儲存。房間結束時把最終結果落地成歷史紀錄。
- Player 端目前用 `nickname` 對應「我」（mock 資料中固定用 `'你'` 標記），沒有帳號系統。若要讓玩家跨裝置查詢自己的歷史戰績，需要額外的匿名裝置識別（例如 cookie/localStorage token）或要求玩家也登入——這是目前設計中尚未解決的落差，建議提出來跟前端/PM 討論。
- Host 端的歷史詳細頁目前是「敬請期待」的 placeholder（`history/[id].vue` 第 110–124 行），代表 Host 版的深入數據分析 API 可以晚一點做。

---

## 2. 即時通訊：SignalR Hub（`/api/gamehub`）

來源：`app/composables/useGameRoom.ts`

### 2.1 Client → Server（`hubConnection.invoke`）

| 方法 | 參數 | 呼叫者 | 說明 |
|---|---|---|---|
| `CreateRoom` | `{ quizId?, questionIds: string[], timeLimit?: number }` | Host | 建立新房間；成功後觸發 `RoomCreated` |
| `StartGame` | `roomCode: string` | Host | 開始遊戲，觸發 `GameStarted` + 第一題 `QuestionStart` |
| `SkipQuestion` | `roomCode: string` | Host | 跳過目前題目，提前進入該題結果 |
| `PauseGame` | `roomCode: string` | Host | 暫停遊戲（計時器） |
| `RejoinRoom` | `roomCode: string` | Host | 重新整理/重新連線後，向 Host 回傳完整房間快照（`RoomStateSync`） |
| `JoinRoom` | `roomCode: string, nickname: string` | Player | 玩家加入房間，觸發 `PlayerJoined` |
| `SubmitAnswer` | `roomCode: string, answerIndex: number` | Player | 玩家送出答案；後端需計算得分並回傳 `AnswerResult` |

### 2.2 Server → Client（`hubConnection.on`）

| 事件 | Payload | 接收者 | 說明 |
|---|---|---|---|
| `RoomCreated` | `roomCode: string` | Host | 房間建立成功 |
| `PlayerJoined` | `Player` | Host（房內所有人） | 有新玩家加入 |
| `PlayerLeft` | `connectionId: string` | Host | 玩家離線/離開 |
| `AnswerProgress` | `AnswerProgressPayload` | Host only | 即時作答進度（含所選選項，用於即時統計） |
| `GameStarted` | — | Player | 遊戲開始 |
| `QuestionStart` | `QuestionPayload` | Player + Bigscreen | 推送新題目（**不含正確答案**） |
| `AnswerResult` | `AnswerResultPayload` | 個別 Player | 每位玩家自己的作答結果（是否正確、得分） |
| `QuestionEnd` | `QuestionEndPayload` | 所有人 | 本題結束，公布正確答案、解析、排名 |
| `GameEnd` | `RankEntry[]` | 所有人 | 遊戲結束，最終排名 |
| `RoomStateSync` | `RoomStateSyncPayload` | 重新連線者 | 完整房間狀態快照，供重新整理/斷線重連使用 |

### 2.3 Payload 型別（`app/types/other.ts`）

```ts
interface QuestionPayload {
  index: number
  total: number
  text: string
  options: string[]
  timeLimit: number
}

interface QuestionEndPayload {
  answerIndex: number
  explanation: string
  rankings: RankEntry[]
}

interface AnswerResultPayload {
  isCorrect: boolean
  correctAnswerIndex: number
  pointsEarned: number
}

interface AnswerProgressPayload {
  answered: number
  total: number
  connectionId: string
  selectedOptionIndex: number
}

interface RoomStateSyncPayload {
  phase: GamePhase
  room: GameRoom
  currentQuestion?: QuestionPayload
  answeredCount?: number
  totalCount?: number
  questionEnd?: QuestionEndPayload
  rankings?: RankEntry[]
}
```

### 2.4 安全 / 一致性注意事項

- **正確答案不可提前外流**：`QuestionStart` 推給玩家/大螢幕的 `QuestionPayload` 不含 `answerIndex`／`explanation`，只有 `QuestionEnd` 才能公布。後端 Hub 邏輯要確保這點，不能因為方便直接把整個 `Question` 物件序列化出去。
- **`selectedOptionIndex` 只給 Host**：`Player.selectedOptionIndex` 註解明確寫「僅 Host 可見，公布答案前不應揭露給其他玩家」，後端推播給一般玩家的 `Player[]` 列表要過濾掉這個欄位（或只在 `AnswerProgress` 這種 host-only 事件才帶出來）。
- **分數需在伺服器端計算**：`SubmitAnswer` 只送 `answerIndex`，得分完全由後端依照 1.5 節的時間制計分表計算，避免作弊。
- **房間容量限制**：`JoinRoom` 需檢查目前人數 < 30（MVP 上限），超過則回傳錯誤。
- **房間過期檢查**：`JoinRoom` / `RejoinRoom` 需檢查房間是否已超過 24h（唯讀）或 48h（已清除），並回應對應狀態給前端（對照 `host/room/[code].vue` 的「rejoinFailed」畫面）。

---

## 3. REST API

### 3.1 Auth

| Method | Path | Body | Response | 說明 |
|---|---|---|---|---|
| POST | `/api/auth/register` | `{ name, email, password }` | `User` | 註冊新 Host 帳號 |
| POST | `/api/auth/login` | `{ email, password }` | `{ user: User, token: string }` | Email 登入 |
| GET | `/api/auth/google` | — | 302 redirect | 開始 Google OAuth 流程 |
| POST | `/api/auth/logout` | — | 204 | 登出，清除 session/cookie |
| GET | `/api/auth/me` | — | `User` | 取得目前登入使用者 |

驗證規則（來自表單 zod schema）：
- `name`：≥ 2 字。
- `email`：合法 email 格式。
- `password`：註冊時 ≥ 8 碼；登入時前端僅檢查 ≥ 6 碼（後端應以註冊時的規則為準，登入端不需要再次校驗長度，只需比對雜湊）。

⚠️ **待確認/待修正事項**：`app/stores/auth.ts` 第 21–25 行呼叫 `/api/auth/login` 後只把 `data.user` 存進 store，`data.token` 目前被忽略、沒有存放或帶到後續請求的 header 中。若後端採 Bearer Token 機制，前端需要補上儲存與夾帶 token 的邏輯；若後端改用 httpOnly cookie session，則此問題不存在。建議跟後端一起確認採用哪種機制。

### 3.2 Quiz（題庫 CRUD）

| Method | Path | Query/Body | Response | 說明 |
|---|---|---|---|---|
| GET | `/api/quizzes` | `?tag=&search=` | `Quiz[]` | 題庫列表，支援 tag 篩選、標題模糊搜尋 |
| GET | `/api/quizzes/{id}` | — | `Quiz` | 題庫詳情 |
| POST | `/api/quizzes` | `{ questions, title, difficulty }` | `Quiz` | 建立新題庫（手動或 AI 編輯完成後儲存） |

建議額外補上（目前前端未使用，但屬合理的題庫管理需求）：
- `PATCH /api/quizzes/{id}` — 編輯已建立的題庫（標題/題目/是否公開）。
- `DELETE /api/quizzes/{id}` — 刪除題庫。
- `GET /api/quizzes/tags` — 取得所有使用中的 tag（取代前端目前寫死的 tag 清單）。

### 3.3 AI 出題

| Method | Path | Body | Response | 說明 |
|---|---|---|---|---|
| POST | `/api/quizzes/generate`（建議路徑，前端尚未定案） | `GenerateQuestionsRequest` | `GenerateQuestionsResponse` | 依內容/網址生成題目 |

```ts
interface GenerateQuestionsRequest {
  content: string             // 貼上的文字，或 http(s):// 開頭的網址
  questionCount: 3 | 5 | 10
  difficulty: 'beginner' | 'intermediate' | 'expert'
}

interface GenerateQuestionsResponse {
  questions: Question[]
  tokensUsed: number
}
```

規則（`app/pages/library/new/ai.vue`、`app/stores/quiz.ts`）：
- `content` 長度限制：最少 30 字，最多 5000 字；若為網址則後端需自行擷取網頁內容再生成。
- **每位使用者每日限用 1 次**（目前是前端 `localStorage` 純前端計數，形同虛設，必須改成伺服器端依 `userId` + 日期強制限流，否則使用者清 localStorage 或換裝置就能無限使用）。
- 生成失敗、額度用完需回傳明確錯誤訊息（前端會顯示 `error` 文案）。

### 3.4 Game Room（房間建立前置 / 非即時部分）

房間的「建立」與「加入」流程實際上是透過 SignalR `CreateRoom` / `JoinRoom` 完成（見第 2 節），並非傳統 REST。但以下輔助端點建議仍以 REST 提供：

| Method | Path | Body | Response | 說明 |
|---|---|---|---|---|
| GET | `/api/rooms/{code}` | — | `GameRoom`（唯讀） | 供 Bigscreen 或連結分享時，SSR 階段先確認房間是否存在/是否過期 |

### 3.5 History（歷史紀錄）

| Method | Path | Response | 說明 |
|---|---|---|---|
| GET | `/api/history/hosted` | `HostedGameSummary[]` | 目前登入 Host 主持過的所有場次 |
| GET | `/api/history/played` | `PlayedGameSummary[]` | 目前使用者（或裝置）玩過的所有場次 |
| GET | `/api/history/{id}` | `HostedGameSummary \| PlayedGameSummary` | 單場歷史詳情（含完整排名/錯題複習） |

⚠️ 如 1.6 節所述，`played` 歷史查詢的身分識別方式（匿名 Player 如何跨裝置找回自己的紀錄）目前前端尚未定義清楚，建議實作前先跟前端/PM 對齊方案（例如：要求 Player 也選擇性登入才能保存歷史）。

### 3.6 Comment（題庫留言）— 建議新增

| Method | Path | Body | Response | 說明 |
|---|---|---|---|---|
| GET | `/api/quizzes/{id}/comments` | — | `Comment[]` | 取得題庫留言列表 |
| POST | `/api/quizzes/{id}/comments` | `{ text }` | `Comment` | 新增留言（需登入） |

### 3.7 Favorite / Follow — 建議新增

| Method | Path | Response | 說明 |
|---|---|---|---|
| PUT | `/api/quizzes/{id}/favorite` | 204 | 收藏題庫 |
| DELETE | `/api/quizzes/{id}/favorite` | 204 | 取消收藏 |
| PUT | `/api/creators/{id}/follow` | 204 | 追蹤出題者 |
| DELETE | `/api/creators/{id}/follow` | 204 | 取消追蹤 |
| GET | `/api/creators/{id}` | `{ id, name, avatar, quizCount, challengeCount, isFollowing }` | 創作者公開檔案（`library/[id].vue` 右側創作者卡片用） |

### 3.8 通用回應格式

`app/types/other.ts` 定義了一個通用包裝格式，若後端採用需全站統一：

```ts
interface ApiResponse<T> {
  data: T
  success: boolean
  message?: string
}
```

註：目前 store 中的 `$fetch` 呼叫（如 `fetchQuizzes`）是直接期待 `Quiz[]`，並未套用這層 `ApiResponse` 包裝，代表這個型別可能是「預留但尚未實際使用」，建議跟前端確認最終要不要統一走這個格式，避免兩種回應風格並存。

---

## 4. 使用情境（User Flows）對照

### 4.1 Host 建立並主持一場遊戲

1. 登入（`/api/auth/login` 或 Google OAuth）。
2. 進入 `/library/new`，選擇「手動出題」或「AI 出題」。
   - AI 出題：貼文字/網址 → `POST /api/quizzes/generate` → 取得草稿題目 → 可編輯每題 → 確認。
   - 手動出題：直接在頁面上新增/編輯題目。
3.（可選）`POST /api/quizzes` 儲存為題庫，供之後重複使用。
4. 透過 SignalR `CreateRoom` 建立房間 → 收到 `RoomCreated(roomCode)` → 導向 `/host/room/{code}`。
5. 等待玩家加入（`PlayerJoined` 事件即時更新玩家列表），可開 `/bigscreen/{code}` 給大螢幕投影。
6. 呼叫 `StartGame` → 逐題進行，期間可 `SkipQuestion` / `PauseGame`。
7. 每題結束後前端顯示 `QuestionEnd` 送來的排名；全部題目跑完後收到 `GameEnd`，顯示最終頒獎台。
8. 遊戲結束後，該場次應落地寫入 `HostedGameSummary`（含完整 `RankEntry[]`），可在 `/history` 的 Host 分頁查看。

### 4.2 Player 加入並遊玩

1. 造訪 `/join`，輸入房號 + 暱稱（或掃 QR code 帶入房號），或直接開啟 `/join/{code}` 連結。
2. SignalR `JoinRoom(roomCode, nickname)` → 房間人數與過期狀態需在此驗證。
3. 收到 `QuestionStart` 後作答，`SubmitAnswer(roomCode, answerIndex)`。
4. 收到自己的 `AnswerResult`（對錯 + 得分），接著全房收到 `QuestionEnd`（正解 + 解析 + 排名）。
5. 全部題目結束收到 `GameEnd`，顯示自己最終名次。
6. 遊戲紀錄應落地為該玩家的 `PlayedGameSummary`，含本場答錯題目的 `WrongAnswerReview[]`，供之後在 `/history/{id}` 複習。

### 4.3 重新整理 / 斷線重連

- Host 或 Player 若在遊戲進行中重新整理頁面，前端會呼叫 `RejoinRoom(roomCode)`（Host）或需要類似機制（Player），伺服器需能依 `roomCode` + 原本的 `connectionId`（或某種可恢復的識別）重建連線，並回傳 `RoomStateSync` 讓前端還原到正確的 `GamePhase`。
- 若房間已過期或不存在，需明確回應失敗，前端會顯示「重新連線失敗」畫面並導回 `/library`。

### 4.4 瀏覽題庫（Library）

1. `GET /api/quizzes?tag=&search=` 取得列表，前端分「精選」（前 4 筆）「最新」兩區塊呈現（目前純前端切片，實務上建議由後端提供 `featured` 篩選或排序欄位）。
2. 點入 `GET /api/quizzes/{id}` 詳情頁：可查看題目列表、留言、創作者資訊、相關題庫推薦；可收藏、追蹤創作者、留言（3.6、3.7）。
3. 點「開始遊戲」開啟設定 modal（選擇時限）→ 走 4.1 的建房流程。

### 4.5 查看歷史紀錄

1. `/history` 頁面依角色（Host/Player）分頁呈現列表卡片（`GET /api/history/hosted` / `GET /api/history/played`）。
2. 點入 `/history/{id}` 查看詳情：
   - 兩種角色都看得到頒獎台 + 完整排名長條圖。
   - Player 額外看到錯題複習（`wrongAnswers`）。
   - Host 目前只有「敬請期待」的分析 placeholder，可晚點再做。

---

## 5. 待釐清事項（Open Questions）

整理過程中發現幾個前端尚未定案、建議在後端開發前先與前端/PM 對齊的問題：

1. **Auth token 儲存方式**：登入回應含 `token`，但前端目前沒有存放或使用它（見 3.1 附註）。需確認是 Bearer token 還是 cookie-based session。
2. **AI 出題每日額度**：目前完全靠前端 localStorage 計數，容易被繞過，正式上線前必須改成伺服器端強制限流（依登入使用者 ID + UTC 或當地日期）。
3. **Player 歷史紀錄的身分識別**：Player 不需帳號即可遊玩，但 `/history` 的 Player 分頁預期能看到「我玩過的場次」——需要決定用什麼機制跨裝置/跨 session 識別同一位匿名玩家（例如：允許但非強制登入、或用長效 cookie/device id）。
4. **收藏 / 追蹤 / 留言目前無持久化**：UI 已經存在（`library/[id].vue`），但都是純前端 local state，重新整理就消失，需要後端補上對應資料表與 API（3.6、3.7）。
5. **Tag 清單目前寫死在前端**：`全部/Kubernetes/React/AWS/SQL/TypeScript/Go/System Design` 是寫死陣列，建議改為後端動態提供（依實際題庫使用到的 tag 聚合）。
6. **`ApiResponse<T>` 包裝格式是否要全站採用**：型別已定義但目前實際呼叫並未使用，需要跟前端確認最終格式。
7. **房間 24h/48h 生命週期與 30 人上限**：這些規則目前只寫在行銷頁 FAQ 文案裡，並非強制邏輯，需要後端排程/驗證機制真正落實。
