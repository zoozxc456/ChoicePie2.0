import type { PagedResult } from '~/types/api'

/** 總頁數：優先使用後端給的 totalPages，否則以 totalCount / pageSize 推算；尚未載入時為 0 */
export const totalPagesOf = (paged: Omit<PagedResult<unknown>, 'items'> | null | undefined) => {
  if (!paged) return 0
  return paged.totalPages ?? Math.ceil(paged.totalCount / (paged.pageSize || 1))
}

export type PageItem = number | 'ellipsis'

/** 頁數多時只顯示目前頁附近 ± 1 頁、首尾頁，其餘用刪節號收合，避免頁碼過多擠爆版面。 */
export const buildPageItems = (current: number, total: number, siblings = 1): PageItem[] => {
  const items: PageItem[] = []

  const rangeStart = Math.max(2, current - siblings)
  const rangeEnd = Math.min(total - 1, current + siblings)

  items.push(1)
  if (rangeStart > 2) {
    items.push('ellipsis')
  }
  for (let page = rangeStart; page <= rangeEnd; page++) {
    items.push(page)
  }
  if (rangeEnd < total - 1) {
    items.push('ellipsis')
  }
  if (total > 1) {
    items.push(total)
  }

  return items
}
