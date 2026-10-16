import { describe, it, expect } from 'vitest'
import { totalPagesOf } from '~/utils/pagination'

const paged = (overrides: Record<string, unknown> = {}) => ({ pageNumber: 1, pageSize: 20, totalCount: 45, items: [], ...overrides })

describe('totalPagesOf', () => {
  it('後端有給 totalPages 時直接使用', () => {
    expect(totalPagesOf(paged({ totalPages: 7 }))).toBe(7)
  })

  it('沒有 totalPages 時以 totalCount / pageSize 無條件進位', () => {
    expect(totalPagesOf(paged())).toBe(3)
  })

  it('pageSize 為 0 時不會除以零', () => {
    expect(totalPagesOf(paged({ pageSize: 0, totalCount: 5 }))).toBe(5)
  })

  it('尚未載入（null / undefined）時為 0', () => {
    expect(totalPagesOf(null)).toBe(0)
    expect(totalPagesOf(undefined)).toBe(0)
  })
})
