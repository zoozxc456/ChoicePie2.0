import { defineStore } from 'pinia'
import type { Quiz, Question, Difficulty } from '~/types/quiz'

export const useQuizStore = defineStore('quiz', () => {
  const quizzes = ref<Quiz[]>([])
  const currentQuiz = ref<Quiz | null>(null)
  const generatedQuestions = ref<Question[]>([])
  const isGenerating = ref(false)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  // ── Library ──

  const fetchQuizzes = async (params?: { tag?: string, search?: string }) => {
    isLoading.value = true
    error.value = null
    try {
      const data = await $fetch<Quiz[]>('/api/quizzes', { query: params })
      quizzes.value = data
    } catch (e) {
      error.value = '載入題庫失敗，請稍後再試'
      console.error(e)
    } finally {
      isLoading.value = false
    }
  }

  const fetchQuizById = async (id: string) => {
    isLoading.value = true
    error.value = null
    try {
      const data = await $fetch<Quiz>(`/api/quizzes/${id}`)
      currentQuiz.value = data
    } catch (e) {
      error.value = '無法載入此題庫'
      console.error(e)
    } finally {
      isLoading.value = false
    }
  }

  // ── AI 出題 ──

  const generateQuestions = async (
    content: string,
    questionCount: 3 | 5 | 10,
    difficulty: Difficulty
  ) => {
    isGenerating.value = true
    error.value = null
    try {
      const data = await $fetch<{ questions: Question[] }>('/api/ai/generate', {
        method: 'POST',
        body: { content, questionCount, difficulty }
      })
      generatedQuestions.value = data.questions
    } catch (e) {
      error.value = 'AI 出題失敗，請稍後再試'
      console.error(e)
      throw e
    } finally {
      isGenerating.value = false
    }
  }

  const saveQuiz = async (questions: Question[], title: string, difficulty: Difficulty) => {
    const data = await $fetch<Quiz>('/api/quizzes', {
      method: 'POST',
      body: { questions, title, difficulty }
    })
    quizzes.value.unshift(data)
    return data
  }

  // ── Helpers ──

  const setCurrentQuiz = (quiz: Quiz) => {
    currentQuiz.value = quiz
  }

  const updateGeneratedQuestion = (index: number, updated: Partial<Question>) => {
    if (!generatedQuestions.value[index]) return
    generatedQuestions.value[index] = { ...generatedQuestions.value[index], ...updated }
  }

  const clearGenerated = () => {
    generatedQuestions.value = []
  }

  return {
    quizzes, currentQuiz, generatedQuestions,
    isGenerating, isLoading, error,
    fetchQuizzes, fetchQuizById,
    generateQuestions, saveQuiz,
    setCurrentQuiz, updateGeneratedQuestion, clearGenerated
  }
})
