import { describe, it, expect } from 'vitest'
import { buildPageItems, totalPagesOf } from '~/utils/pagination'

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

describe('buildPageItems', () => {
  it('只有 1 頁時只顯示 1', () => {
    expect(buildPageItems(1, 1)).toEqual([1])
  })

  it('頁數少時全部列出不收合', () => {
    expect(buildPageItems(2, 4)).toEqual([1, 2, 3, 4])
  })

  it('在第一頁時只收合後段', () => {
    expect(buildPageItems(1, 10)).toEqual([1, 2, 'ellipsis', 10])
  })

  it('在中間頁時顯示前後 ± 1 頁並兩側收合', () => {
    expect(buildPageItems(5, 10)).toEqual([1, 'ellipsis', 4, 5, 6, 'ellipsis', 10])
  })

  it('在最後一頁時只收合前段', () => {
    expect(buildPageItems(10, 10)).toEqual([1, 'ellipsis', 9, 10])
  })

  it('與首尾頁相鄰時不顯示刪節號', () => {
    expect(buildPageItems(3, 5)).toEqual([1, 2, 3, 4, 5])
  })
})
