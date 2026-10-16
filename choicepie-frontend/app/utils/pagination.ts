import type { PagedResult } from '~/types/api'

/** 總頁數：優先使用後端給的 totalPages，否則以 totalCount / pageSize 推算；尚未載入時為 0 */
export const totalPagesOf = (paged: Omit<PagedResult<unknown>, 'items'> | null | undefined) => {
  if (!paged) return 0
  return paged.totalPages ?? Math.ceil(paged.totalCount / (paged.pageSize || 1))
}
