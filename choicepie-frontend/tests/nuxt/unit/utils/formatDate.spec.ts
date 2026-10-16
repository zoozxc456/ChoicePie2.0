import { describe, it, expect } from 'vitest'
import { formatLocaleDate } from '~/utils/formatDate'

// 取當天正午（UTC），避免執行環境時區把日期推到前一天或後一天
const ISO = '2026-01-15T12:00:00Z'

describe('formatLocaleDate', () => {
  it('short：沿用 toLocaleDateString(locale) 的預設格式', () => {
    expect(formatLocaleDate(ISO, 'zh-TW')).toBe(new Date(ISO).toLocaleDateString('zh-TW'))
    expect(formatLocaleDate(ISO, 'en', 'short')).toBe(new Date(ISO).toLocaleDateString('en'))
  })

  it('medium：英文顯示縮寫月份', () => {
    expect(formatLocaleDate(ISO, 'en', 'medium')).toBe('Jan 15, 2026')
  })

  it('medium：中文顯示年月日', () => {
    expect(formatLocaleDate(ISO, 'zh-TW', 'medium')).toBe('2026年1月15日')
  })

  it('medium：非 en 的語系一律以 zh-TW 格式呈現', () => {
    expect(formatLocaleDate(ISO, 'ja', 'medium')).toBe(formatLocaleDate(ISO, 'zh-TW', 'medium'))
  })
})
