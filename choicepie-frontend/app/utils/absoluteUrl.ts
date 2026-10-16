/** 將可能為相對路徑的 URL 轉為絕對 URL（OG / Twitter 預覽圖需要絕對網址）；無效輸入回傳 undefined。 */
export const toAbsoluteUrl = (url: string | null | undefined, origin: string): string | undefined => {
  if (!url) return undefined
  try {
    return new URL(url, origin).href
  } catch {
    return undefined
  }
}
