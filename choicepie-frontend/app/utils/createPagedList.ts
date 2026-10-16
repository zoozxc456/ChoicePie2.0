import { ref } from 'vue'
import type { Ref } from 'vue'
import type { PagedResult } from '~/types/api'

/**
 * 通用「載入更多」分頁清單狀態：第一頁覆蓋、之後的頁面累加。
 * 供收藏、追蹤、創作者作品等 store 共用。
 */
export const createPagedList = <TDto, T>(
  fetchPage: (page: number, pageSize: number) => Promise<PagedResult<TDto>>,
  map: (dto: TDto) => T,
  pageSize = 12
) => {
  const items = ref([]) as Ref<T[]>
  const page = ref(0)
  const totalCount = ref(0)
  const hasNext = ref(false)
  const isLoading = ref(false)
  const isLoadingMore = ref(false)
  const hasError = ref(false)

  const apply = (data: PagedResult<TDto>, append: boolean) => {
    const mapped = data.items.map(map)
    items.value = append ? [...items.value, ...mapped] : mapped
    page.value = data.pageNumber
    totalCount.value = data.totalCount
    hasNext.value = data.hasNextPage ?? data.pageNumber * data.pageSize < data.totalCount
  }

  const load = async () => {
    isLoading.value = true
    hasError.value = false
    try {
      apply(await fetchPage(1, pageSize), false)
    } catch (e) {
      hasError.value = true
      console.error(e)
    } finally {
      isLoading.value = false
    }
  }

  const loadMore = async () => {
    if (!hasNext.value || isLoading.value || isLoadingMore.value) return
    isLoadingMore.value = true
    hasError.value = false
    try {
      apply(await fetchPage(page.value + 1, pageSize), true)
    } catch (e) {
      hasError.value = true
      console.error(e)
    } finally {
      isLoadingMore.value = false
    }
  }

  const removeWhere = (predicate: (item: T) => boolean) => {
    const before = items.value.length
    items.value = items.value.filter(item => !predicate(item))
    totalCount.value = Math.max(0, totalCount.value - (before - items.value.length))
  }

  const reset = () => {
    items.value = []
    page.value = 0
    totalCount.value = 0
    hasNext.value = false
    hasError.value = false
  }

  return {
    items, page, totalCount, hasNext, isLoading, isLoadingMore, hasError,
    load, loadMore, removeWhere, reset
  }
}
