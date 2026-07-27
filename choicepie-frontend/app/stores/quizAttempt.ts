import { defineStore } from 'pinia'
import { useQuizAttemptClientApi } from '~/services/quizAttempt/client'
import type { StartAttemptResultDto, QuizAttemptResultDto, QuizAttemptHistoryItemDto } from '~/types/api'

export const useQuizAttemptStore = defineStore('quizAttempt', () => {
  const quizAttemptApi = useQuizAttemptClientApi()

  const currentAttempt = ref<StartAttemptResultDto | null>(null)
  const result = ref<QuizAttemptResultDto | null>(null)
  const history = ref<QuizAttemptHistoryItemDto[]>([])
  const isLoading = ref(false)
  const isLoadingHistory = ref(false)
  const error = ref<string | null>(null)

  const startAttempt = async (quizId: string) => {
    isLoading.value = true
    error.value = null
    try {
      const data = await quizAttemptApi.startAttempt(quizId)
      currentAttempt.value = data
      return data
    } catch (e) {
      error.value = '無法開始作答，請稍後再試'
      console.error(e)
      throw e
    } finally {
      isLoading.value = false
    }
  }

  const submitAnswer = async (attemptId: string, questionId: string, selectedOptionIndex: number) => {
    await quizAttemptApi.submitAnswer(attemptId, questionId, selectedOptionIndex)
  }

  const completeAttempt = async (attemptId: string) => {
    isLoading.value = true
    error.value = null
    try {
      const data = await quizAttemptApi.completeAttempt(attemptId)
      result.value = data
      return data
    } catch (e) {
      error.value = '無法完成作答，請稍後再試'
      console.error(e)
      throw e
    } finally {
      isLoading.value = false
    }
  }

  const fetchAttemptById = async (attemptId: string) => {
    isLoading.value = true
    error.value = null
    try {
      const data = await quizAttemptApi.fetchAttemptById(attemptId)
      if (data.completedAt) {
        result.value = data
      }
      return data
    } catch (e) {
      error.value = '無法載入作答結果'
      console.error(e)
      throw e
    } finally {
      isLoading.value = false
    }
  }

  const fetchAttemptHistory = async (quizId: string) => {
    isLoadingHistory.value = true
    try {
      history.value = await quizAttemptApi.fetchAttemptHistory(quizId)
      return history.value
    } catch (e) {
      console.error(e)
      throw e
    } finally {
      isLoadingHistory.value = false
    }
  }

  const reset = () => {
    currentAttempt.value = null
    result.value = null
    error.value = null
  }

  return {
    currentAttempt, result, history, isLoading, isLoadingHistory, error,
    startAttempt, submitAnswer, completeAttempt, fetchAttemptById, fetchAttemptHistory, reset
  }
})
