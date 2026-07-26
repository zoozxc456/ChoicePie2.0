import { useAdminQuizClientApi } from '~/services/admin/quiz'
import type { AdminListQuizzesQuery, AdminQuizDetailDto, CommentDto, PagedResult, QuizSummaryDto } from '~/types/api'

export const useAdminQuizStore = defineStore('adminQuiz', () => {
  const adminQuizApi = useAdminQuizClientApi()

  const quizzes = ref<PagedResult<QuizSummaryDto> | null>(null)
  const currentQuiz = ref<AdminQuizDetailDto | null>(null)
  const comments = ref<PagedResult<CommentDto> | null>(null)
  const isLoading = ref(false)
  const isLoadingDetail = ref(false)
  const isLoadingComments = ref(false)
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

  const fetchQuizById = async (id: string) => {
    isLoadingDetail.value = true
    error.value = null
    try {
      currentQuiz.value = await adminQuizApi.fetchQuizById(id)
      return currentQuiz.value
    } catch (e) {
      error.value = '無法載入題庫資料'
      console.error(e)
      throw e
    } finally {
      isLoadingDetail.value = false
    }
  }

  const fetchQuizComments = async (id: string, pageNumber = 1, pageSize = 20) => {
    isLoadingComments.value = true
    error.value = null
    try {
      comments.value = await adminQuizApi.fetchQuizComments(id, pageNumber, pageSize)
      return comments.value
    } catch (e) {
      error.value = '無法載入留言列表'
      console.error(e)
      throw e
    } finally {
      isLoadingComments.value = false
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
      if (currentQuiz.value?.id === id) {
        currentQuiz.value = { ...currentQuiz.value, status: 'takendown', takedownReason: reason }
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
      if (currentQuiz.value?.id === id) {
        currentQuiz.value = { ...currentQuiz.value, status: 'draft', takedownReason: null, takedownBy: null, takedownAt: null }
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
    currentQuiz,
    comments,
    isLoading,
    isLoadingDetail,
    isLoadingComments,
    isTakingDown,
    isRestoring,
    error,
    fetchQuizzes,
    fetchQuizById,
    fetchQuizComments,
    takeDownQuiz,
    restoreQuiz
  }
})
