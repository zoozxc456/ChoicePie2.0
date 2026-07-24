import { useAdminQuizClientApi } from '~/services/admin/quiz'
import type { AdminListQuizzesQuery, PagedResult, QuizSummaryDto } from '~/types/api'

export const useAdminQuizStore = defineStore('adminQuiz', () => {
  const adminQuizApi = useAdminQuizClientApi()

  const quizzes = ref<PagedResult<QuizSummaryDto> | null>(null)
  const isLoading = ref(false)
  const isTakingDown = ref(false)
  const isRestoring = ref(false)
  const error = ref<string | null>(null)

  const fetchQuizzes = async (query?: AdminListQuizzesQuery) => {
    isLoading.value = true
    error.value = null
    try {
      quizzes.value = await adminQuizApi.fetchQuizzes(query)
      return quizzes.value
    } catch (e) {
      error.value = '無法載入題庫列表'
      console.error(e)
      throw e
    } finally {
      isLoading.value = false
    }
  }

  const takeDownQuiz = async (id: string, reason: string) => {
    isTakingDown.value = true
    error.value = null
    try {
      await adminQuizApi.takeDownQuiz(id, reason)
      if (quizzes.value) {
        quizzes.value = {
          ...quizzes.value,
          items: quizzes.value.items.map(q => q.id === id ? { ...q, status: 'takendown' } : q)
        }
      }
    } catch (e) {
      error.value = '下架題庫失敗，請稍後再試'
      console.error(e)
      throw e
    } finally {
      isTakingDown.value = false
    }
  }

  const restoreQuiz = async (id: string) => {
    isRestoring.value = true
    error.value = null
    try {
      await adminQuizApi.restoreQuiz(id)
      if (quizzes.value) {
        quizzes.value = {
          ...quizzes.value,
          items: quizzes.value.items.map(q => q.id === id ? { ...q, status: 'draft' } : q)
        }
      }
    } catch (e) {
      error.value = '還原題庫失敗，請稍後再試'
      console.error(e)
      throw e
    } finally {
      isRestoring.value = false
    }
  }

  return {
    quizzes,
    isLoading,
    isTakingDown,
    isRestoring,
    error,
    fetchQuizzes,
    takeDownQuiz,
    restoreQuiz
  }
})
