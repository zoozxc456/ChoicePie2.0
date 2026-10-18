# 已知限制與風險

> 撰寫 [architecture.md](./architecture.md) 時，從程式碼中觀察到的架構風險，以及文件與程式碼的落差。
> 這裡**只記錄事實與影響，不提出修正方案**。嚴重度依「發生機率 × 影響」主觀評估；標註「待驗證」的項目是讀程式碼推論出來的，尚未實際重現。
> 最後對照的 commit：`275be17`（develop）。

| # | 項目 | 嚴重度 |
|---|---|---|
| R1 | 未指派方案的會員沒有 AI 每日額度 | 高 |
| R2 | 建房時未檢查題庫狀態與擁有者 | 中 |
| R3 | 會員 SSR 的 401 重試沒有帶上新 token（待驗證） | 中 |
| R4 | 遊戲結束時先寫 Postgres 再寫 Redis，可能重複產生 GameSession | 中 |
| R5 | 快取失效是 best-effort | 低 |
| R6 | `GameEndedDomainEvent` 被觸發但永遠不會派送 | 低 |
| R7 | 建房時房號唯一性的檢查與寫入不是原子操作 | 低 |
| R8 | 房間鎖租約固定 15 秒且不會續約 | 低 |
| R9 | DataProtection 金鑰只存在 Redis | 低 |
| R10 | `MemberActivityChecker` 使用同步 DB 查詢 | 低 |
| R11 | BFF 的 refresh 去重只在單一 process 內有效 | 低 |
| R12 | 遊戲推進完全依賴 Host 瀏覽器 | 低（設計取捨） |
| D1 | README 仍寫「只支援單一後端實例」 | 文件落差 |
| D2 | 前端 CLAUDE.md 對 401 重試的描述與程式碼不符 | 文件落差 |
| D3 | `backend-spec.md` 是實作前的規格 | 文件落差 |
| D4 | 未使用的 `AntiforgeryMiddlewareExtensions` | 死碼 |

---

## R1｜未指派方案的會員沒有 AI 每日額度 — 高

- **現象**：AI 出題的每日次數與 token 額度檢查包在 `if (tier is not null)` 裡。`Member.TierId` 是 nullable，註冊流程也沒有指派方案；`MembershipTier.IsDefault` 除了 DTO 投影之外，沒有任何業務邏輯讀取它。
- **證據**：
  - `choicepie-backend/ChoicePie.Backend/ChoicePie.Backend.Application/Quizzes/Commands/GenerateQuizQuestionsCommandHandler.cs:67`
  - `ChoicePie.Backend.Domain/Aggregates/Member/Member.cs:16`（`Guid? TierId`），只有 `AssignTier`（:72）會寫入
  - `ChoicePie.Backend.Domain/Aggregates/MembershipTier/MembershipTier.cs:13`（`IsDefault` 沒有使用端）
- **影響**：管理員沒手動指派方案的會員，唯一的限制是速率限制 `Ai` 5 次/分鐘，加上「同一時間只能有一個請求」的鎖，理論上每人每天可以呼叫約 7,200 次付費 AI API。README 寫的「membership tiers drive the daily AI quota」只對已指派方案的會員成立。若「無方案 = 無限制」是刻意的設計，這項可降為文件落差。

## R2｜建房時未檢查題庫狀態與擁有者 — 中

- **現象**：`CreateRoomCommandHandler` 用 `quizRepository.GetByIdAsync` 載入題庫後，直接建立 `GameRoom`，沒有檢查 `QuizStatus`，也沒有檢查呼叫者是否為擁有者。相比之下，`GetQuizByIdQueryHandler` 不會把非 Published 的題庫給非擁有者看，`StartQuizAttemptCommandHandler` 也要求 `Published`。
- **證據**：`ChoicePie.Backend.Application/GameRooms/Commands/CreateRoomCommandHandler.cs:17-33`；對照 `Application/Quizzes/Queries/GetQuizByIdQueryHandler.cs:23`、`Application/QuizAttempts/Commands/StartQuizAttemptCommandHandler.cs:33`
- **影響**：任何會員只要知道題庫 ID，就能對 Draft / Archived / **TakenDown**（被管理員下架）的題庫開房。題目會出現在 `QuestionStart`，答案與解析會出現在 `QuestionEnd`。被下架題庫的 ID 先前公開過，所以很容易取得。能否從 Repository 讀到 `Deleted` 的題庫，取決於 EF 的 query filter（未驗證）。

## R3｜會員 SSR 的 401 重試沒有帶上新 token — 中（待驗證）

- **現象**：`useApi` 在 admin 路徑重試時，會用 `x-fresh-access-token` 帶上剛 refresh 到的 token；member 路徑重試時卻沒帶（`request<T>(path, options, true)`），而且 `authStore.fetchMe()` 只回傳 `boolean`，拿不到新 token。依 `[...path].ts` 的註解，SSR 時的重試是全新的 sub-request，讀到的仍是這次 SSR 最外層請求帶進來的**舊** cookie。
- **證據**：`choicepie-frontend/app/composables/useApi.ts:113`（member 重試）對照 `:96-99`（admin 重試）；`choicepie-frontend/server/api/v1/[...path].ts`
- **影響**：access token 剛過期的使用者重新整理頁面時，SSR 內的 API 呼叫可能 refresh 成功，但重試仍然 401，結果拋出錯誤（log 會出現 `retry ALSO 401`）。CSR 不受影響，因為瀏覽器會帶上新 cookie。

## R4｜遊戲結束時先寫 Postgres 再寫 Redis，可能重複產生 GameSession — 中

- **現象**：最後一題的 `SkipQuestion` 流程是 `AdvanceToNextQuestion`（Phase=Ended）→ INSERT `GameSession` 並 `SaveChanges` → `gameRoomRepository.SaveAsync`（Redis）。兩次寫入之間沒有交易，也沒有冪等鍵。
- **證據**：`ChoicePie.Backend.Application/GameRooms/Commands/SkipQuestionCommandHandler.cs:58-60` 與 `:29`
- **影響**：如果 Postgres 已 commit、但寫 Redis 失敗（或超過鎖租約，見 R8），Redis 裡的房間仍停在 `Reveal`。Host 前端再呼叫一次 `SkipQuestion` 時，會再寫一筆 `GameSession`，造成歷史紀錄重複。

## R5｜快取失效是 best-effort — 低

- **現象**：清題庫快取的 handler 訂閱的是 in-process 的 `DomainEventNotification<T>`，在 commit **之後**才在同一個請求內 Publish，沒有重試。
- **證據**：`ChoicePie.Backend.Application/Quizzes/EventHandlers/InvalidateQuizCacheHandlers.cs`；`ChoicePie.Backend.Shared.Infrastructure.Persistence/Repositories/EfUnitOfWork.cs`（`DispatchDomainEventsAsync`）
- **影響**：如果清快取失敗（Redis 暫時不可用、process 在 commit 後中止），舊資料最多會留到 TTL 到期（題庫 5 分鐘、標籤 10 分鐘）。fail-safe 機制也可能在來源出錯時延長舊值的壽命（最多 2 小時）。另外，`DispatchDomainEventsAsync` 沒有 try/catch，清快取失敗的例外會往上拋，讓**已經 commit 的請求**回傳錯誤給用戶端，用戶端可能因此重送。

## R6｜`GameEndedDomainEvent` 被觸發但永遠不會派送 — 低

- **現象**：`GameRoom.AdvanceToNextQuestion` 會 `AddDomainEvent(new GameEndedDomainEvent(...))`，但 `GameRoom` 存在 Redis、不經過 EF ChangeTracker，`EfUnitOfWork` 收不到這個事件；而且整個方案裡也沒有任何 handler 訂閱它。`GameSession` 是由 handler 直接寫入的。
- **證據**：`ChoicePie.Backend.Domain/Aggregates/GameRoom/GameRoom.cs:194`；全專案搜尋 `GameEndedDomainEvent` 只有定義與觸發兩處
- **影響**：目前沒有功能受影響，但閱讀程式碼時容易誤以為遊戲結束會觸發事件流程。日後若有人加了 handler，事件也不會被派送。

## R7｜建房時房號唯一性的檢查與寫入不是原子操作 — 低

- **現象**：`GenerateUniqueRoomCodeAsync` 先用 `KeyExistsAsync` 檢查，再用無條件的 `StringSetAsync` 寫入（不是 `When.NotExists`）；`CreateRoomCommand` 也不是 `IGameRoomCommand`，所以不受房間鎖保護。
- **證據**：`ChoicePie.Backend.Application/GameRooms/Commands/CreateRoomCommandHandler.cs:50`；`ChoicePie.Backend.Infrastructure/Caching/Repositories/RedisGameRoomRepository.cs:31`
- **影響**：兩個建房請求如果同時產生相同的房號，後寫的會**覆蓋**先建立的房間。機率取決於房號空間大小與同時存在的房間數量，目前規模下極低。

## R8｜房間鎖租約固定 15 秒且不會續約 — 低

- **現象**：`RedisDistributedLock` 的預設 `LeaseDuration` 是 15 秒，沒有續約機制。
- **證據**：`ChoicePie.Backend.Shared.Infrastructure.Caching/Services/RedisDistributedLock.cs:10`
- **影響**：如果某個 `IGameRoomCommand` 執行超過 15 秒（例如 R4 的 Postgres 寫入卡住），鎖會提前過期，其他節點可能同時「讀 → 改 → 寫回」同一個房間，後寫的會覆蓋先寫的。

## R9｜DataProtection 金鑰只存在 Redis — 低

- **現象**：outbox payload 用 DataProtection 加密，金鑰環只存在 Redis 的 `ChoicePie:DataProtection-Keys`。
- **證據**：`ChoicePie.Backend.WebApi/Program.cs:126`
- **影響**：如果 Redis 資料遺失（沒有持久化的部署、flush、更換實例），尚未派送的 outbox 訊息會無法解密，重試 10 次後成為 dead letter，例如驗證信、重設密碼信就不會寄出。

## R10｜`MemberActivityChecker` 使用同步 DB 查詢 — 低

- **現象**：快取 miss 時，`QueryIsActive` 用同步的 `.FirstOrDefault()` 查詢 Postgres。
- **證據**：`ChoicePie.Backend.Infrastructure/Identity/MemberActivityChecker.cs:42`
- **影響**：每個 `MemberOnly` 請求都會經過這裡。快取 miss 時會阻塞一個執行緒池執行緒，高併發時可能造成 thread pool starvation。

## R11｜BFF 的 refresh 去重只在單一 process 內有效 — 低

- **現象**：`refreshDedupe.ts` 使用模組層級的 `Map`。
- **證據**：`choicepie-frontend/server/utils/refreshDedupe.ts:10`
- **影響**：前端跑多個 Nuxt 實例時，同一顆 refresh token 仍可能被並行送到後端。目前靠後端 `RefreshTokenCommandHandler` 的寬限期兜底，所以影響有限；但「BFF 保證只送一次」這個假設在多實例下不成立。

## R12｜遊戲推進完全依賴 Host 瀏覽器 — 低（設計取捨）

- **現象**：後端不計時，也不自動推進；倒數歸零、全員作答、Reveal 停留 5 秒後的 `SkipQuestion`，都由 `host/room/[code].vue` 的 watcher 觸發。
- **證據**：`choicepie-frontend/app/pages/host/room/[code].vue:34-46`；`choicepie-frontend/app/stores/game.ts:28`
- **影響**：Host 分頁進入背景（瀏覽器節流計時器）、網路不穩、或同一位 Host 開了多個分頁時，遊戲節奏可能卡住或重複推進。重複呼叫會因為 phase 檢查拋出 `InvalidGamePhaseException`，不會破壞狀態。Host 斷線時會自動暫停，這部分已經處理。

---

## D1｜README 仍寫「只支援單一後端實例」

- `README.md:72` 寫的是「`GameRoomLock` is an in-process lock, so only a single backend instance is supported」。
- 實際上已經改成 `GameRoomLockBehavior` + `RedisDistributedLock`，並加上 SignalR Redis backplane、FusionCache backplane、共用 DataProtection 金鑰，`e2e/` 也用雙節點驗證。本次 README 架構章節的改寫已一併修正。

## D2｜前端 CLAUDE.md 對 401 重試的描述與程式碼不符

- `choicepie-frontend/CLAUDE.md:103` 寫的是 member 與 admin 的重試都會「passing the fresh token via the `x-fresh-access-token` header」，但程式碼只有 admin 路徑這麼做（見 R3）。

## D3｜`backend-spec.md` 是實作前的規格

- `choicepie-backend/docs/backend-spec.md` 仍有「前端已有 UI 但尚未定義型別」、「建議新增」、「待釐清事項」等段落，Comment / Favorite / Follow 等功能都已實作完成。它反映的是當初的需求規格，不是現況，請以 `architecture.md` 與程式碼為準（依約定，本次不修改該檔）。

## D4｜未使用的 `AntiforgeryMiddlewareExtensions`

- `ChoicePie.Backend.Shared.Hosting/Extensions/AntiforgeryMiddlewareExtensions.cs` 定義了 `UseAntiforgeryTokenCookie`，但沒有任何地方呼叫，也沒有 `AddAntiforgery`。這代表 CSRF 防護目前依賴 SameSite cookie（BFF 的 cookie 是 `Lax`；後端 cookie 在設定了 `Auth:CookieDomain` 的跨子網域部署時是 `SameSite=None`）。要注意的是，後端 JwtBearer 的 `OnMessageReceived` 對**所有**請求都會讀 `access_token` cookie，不只 SignalR，所以直接打後端 REST 也能用 cookie 通過驗證。目前 JSON body 的端點需要 CORS preflight，擋住了大部分跨站請求；但不需要 preflight 的請求型態（例如 multipart 上傳、沒有 body 的 POST/DELETE）是否也會被擋下，沒有驗證過。
