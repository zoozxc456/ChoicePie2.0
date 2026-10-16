const REDACTED = '[REDACTED]'
const SENSITIVE_HEADERS = new Set(['authorization', 'x-refresh-token', 'cookie', 'set-cookie'])
const SENSITIVE_BODY_KEY = /password|token|secret|credential/i

/**
 * 決定要轉發給後端的「真實使用者 IP」，讓後端的速率限制能分辨不同使用者
 * （否則後端看到的永遠是 BFF 自己的 IP）。
 *
 * 預設只信任 socket 位址；只有在設定宣告 Nuxt 本身位於可信任的負載平衡器後方
 * （trustProxyHeaders）時，才採用進來的 X-Forwarded-For，取「最右邊」那一筆 ——
 * 即最靠近我們、由可信任 LB 自己附加的位址，最左邊的值可被使用者任意偽造。
 */
export const resolveClientIp = (params: {
  socketIp?: string
  forwardedFor?: string
  trustProxyHeaders: boolean
}): string | undefined => {
  if (params.trustProxyHeaders && params.forwardedFor) {
    const hops = params.forwardedFor.split(',').map(hop => hop.trim()).filter(Boolean)
    const last = hops[hops.length - 1]
    if (last) return last
  }
  return params.socketIp || undefined
}

export const redactHeaders = (headers: Record<string, string>): Record<string, string> =>
  Object.fromEntries(
    Object.entries(headers).map(([key, value]) => [key, SENSITIVE_HEADERS.has(key.toLowerCase()) ? REDACTED : value])
  )

/** 遞迴遮蔽 body 內名稱含 password / token / secret 的欄位；FormData（含檔案）不展開內容。 */
export const redactBody = (body: unknown): unknown => {
  if (body === null || body === undefined || typeof body !== 'object') return body
  if (typeof FormData !== 'undefined' && body instanceof FormData) return '[FormData]'
  if (Array.isArray(body)) return body.map(redactBody)

  return Object.fromEntries(
    Object.entries(body as Record<string, unknown>).map(([key, value]) => [
      key,
      SENSITIVE_BODY_KEY.test(key) ? REDACTED : redactBody(value)
    ])
  )
}
