import { defineStore } from 'pinia'
import { useQuizClientApi } from '~/services/quiz/client'
import { toQuizFromSummary } from '~/stores/quiz'

export const useFavoritesStore = defineStore('favorites', () => {
  const quizApi = useQuizClientApi()

  const list = createPagedList(
    (page, pageSize) => quizApi.fetchMyFavorites(page, pageSize),
    toQuizFromSummary
  )
  const removingId = ref<string | null>(null)
  const removeError = ref(false)

  // 取消收藏後立即從清單移除（失敗則保留並標記錯誤）
  const unfavorite = async (quizId: string) => {
    removingId.value = quizId
    removeError.value = false
    try {
      await quizApi.removeFavorite(quizId)
      list.removeWhere(q => q.id === quizId)
    } catch (e) {
      removeError.value = true
      console.error(e)
    } finally {
      removingId.value = null
    }
  }

  return {
    quizzes: list.items,
    totalCount: list.totalCount,
    hasNext: list.hasNext,
    isLoading: list.isLoading,
    isLoadingMore: list.isLoadingMore,
    hasError: list.hasError,
    removingId,
    removeError,
    fetchFavorites: list.load,
    loadMore: list.loadMore,
    unfavorite
  }
})
