import { describe, it, expect } from 'vitest'
import { sanitizeRedirect } from '~/utils/safeRedirect'

describe('sanitizeRedirect', () => {
  it('接受站內路徑（含 query 與 hash）', () => {
    expect(sanitizeRedirect('/library/abc')).toBe('/library/abc')
    expect(sanitizeRedirect('/library/abc?x=1#top')).toBe('/library/abc?x=1#top')
  })

  it('缺少或非字串時回傳預設值', () => {
    expect(sanitizeRedirect(undefined)).toBe('/library')
    expect(sanitizeRedirect(null)).toBe('/library')
    expect(sanitizeRedirect('')).toBe('/library')
    expect(sanitizeRedirect(42)).toBe('/library')
  })

  it('陣列時取第一個元素', () => {
    expect(sanitizeRedirect(['/a', '/b'])).toBe('/a')
    expect(sanitizeRedirect(['//evil.com'])).toBe('/library')
  })

  it('拒絕外部網址、protocol-relative、反斜線與控制字元', () => {
    expect(sanitizeRedirect('https://evil.com')).toBe('/library')
    expect(sanitizeRedirect('//evil.com')).toBe('/library')
    expect(sanitizeRedirect('/\\evil.com')).toBe('/library')
    expect(sanitizeRedirect('/a\nb')).toBe('/library')
    expect(sanitizeRedirect('javascript:alert(1)')).toBe('/library')
    expect(sanitizeRedirect('library')).toBe('/library')
  })

  it('可自訂 fallback', () => {
    expect(sanitizeRedirect('//evil.com', '/')).toBe('/')
  })
})
