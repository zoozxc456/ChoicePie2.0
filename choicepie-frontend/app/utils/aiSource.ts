/** AI 出題素材的最少字數（網址不受限） */
export const AI_SOURCE_MIN_CHARS = 30

/** 與後端一致：單一、不含空白的 http(s) 網址才會被當成要抓取的網頁 */
export const isUrlSource = (text: string) => /^https?:\/\/\S+$/i.test(text.trim())

export const hasEnoughSource = (text: string) =>
  isUrlSource(text) || text.trim().length >= AI_SOURCE_MIN_CHARS

/** 已開始輸入但字數仍不足，用來顯示提醒 */
export const isSourceTooShort = (text: string) =>
  text.length > 0 && text.length < AI_SOURCE_MIN_CHARS && !isUrlSource(text)
