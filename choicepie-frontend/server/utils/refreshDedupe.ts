// 後端的 refresh token 是一次性輪替：refresh 成功時舊 token 立刻被撤銷、換發一顆新的。
// 多個並發請求各自 401 時，可能會同時各自呼叫 refresh、各自帶著同一顆舊 token 打向後端——
// 這些呼叫是幾乎同時發出的獨立 HTTP 請求，誰的網路 I/O 先完成完全不保證，用「成功後才記錄
// 快取」的寬容期機制會有寫入晚於讀取的競態（後發起的 401 回應可能比先發起的 200 回應更早
// 抵達）。真正的解法是在請求發出「之前」就依 token 去重：同一顆 refresh token 的並發呼叫
// 全部 await 同一個 in-flight promise，永遠只真正打一次後端，不管網路完成順序為何都拿到
// 同一組結果。
type RefreshCall<T> = () => Promise<T>

const memberInFlight = new Map<string, Promise<unknown>>()
const adminInFlight = new Map<string, Promise<unknown>>()

const dedupe = async <T>(cache: Map<string, Promise<unknown>>, token: string, call: RefreshCall<T>): Promise<T> => {
  const existing = cache.get(token) as Promise<T> | undefined
  if (existing) return existing

  const promise = call().finally(() => {
    cache.delete(token)
  })
  cache.set(token, promise)
  return promise
}

export const dedupeMemberRefresh = <T>(refreshToken: string, call: RefreshCall<T>) =>
  dedupe(memberInFlight, refreshToken, call)

export const dedupeAdminRefresh = <T>(refreshToken: string, call: RefreshCall<T>) =>
  dedupe(adminInFlight, refreshToken, call)
