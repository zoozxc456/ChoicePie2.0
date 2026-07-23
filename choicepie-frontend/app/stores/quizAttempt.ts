import { defineStore } from 'pinia'
import { useQuizAttemptClientApi } from '~/services/quizAttempt/client'
import type { StartAttemptResultDto, QuizAttemptResultDto } from '~/types/api'

export const useQuizAttemptStore = defineStore('quizAttempt', () => {
  const quizAttemptApi = useQuizAttemptClientApi()

  const currentAttempt = ref<StartAttemptResultDto | null>(null)
  const result = ref<QuizAttemptResultDto | null>(null)
  const isLoading = ref(false)
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

  const reset = () => {
    currentAttempt.value = null
    result.value = null
    error.value = null
  }

  return {
    currentAttempt, result, isLoading, error,
    startAttempt, submitAnswer, completeAttempt, fetchAttemptById, reset
  }
})
