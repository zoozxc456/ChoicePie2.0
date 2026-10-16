export type DateFormatStyle = 'short' | 'medium'

/**
 * 依目前 i18n 語系格式化 ISO 日期字串。
 * - short：瀏覽器預設的數字日期（後台、紀錄列表）
 * - medium：含縮寫月份的日期（遊戲歷史）；非 en 語系一律以 zh-TW 呈現
 */
export const formatLocaleDate = (iso: string, locale: string, style: DateFormatStyle = 'short') => {
  const date = new Date(iso)
  if (style === 'short') return date.toLocaleDateString(locale)
  return date.toLocaleDateString(locale === 'en' ? 'en-US' : 'zh-TW', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })
}
