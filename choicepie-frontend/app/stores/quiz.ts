import { defineStore } from 'pinia'
import type { Quiz, Question, Difficulty } from '~/types/quiz'

const AI_DAILY_LIMIT = 1

export const useQuizStore = defineStore('quiz', () => {
  const quizzes = ref<Quiz[]>([])
  const currentQuiz = ref<Quiz | null>(null)
  const generatedQuestions = ref<Question[]>([])
  const isGenerating = ref(false)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  // ── AI 每日額度 ──
  const aiUsedDate = ref<string | null>(null)
  const aiUsedCount = ref(0)

  const todayStr = () => new Date().toISOString().slice(0, 10)

  const aiUsesToday = computed(() => aiUsedDate.value === todayStr() ? aiUsedCount.value : 0)
  const canUseAiToday = computed(() => aiUsesToday.value < AI_DAILY_LIMIT)

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

  const recordAiUsage = () => {
    const today = todayStr()
    aiUsedCount.value = aiUsedDate.value === today ? aiUsedCount.value + 1 : 1
    aiUsedDate.value = today
  }

  const generateQuestions = async (
    content: string,
    questionCount: 3 | 5 | 10,
    difficulty: Difficulty
  ) => {
    if (!canUseAiToday.value) {
      error.value = 'AI 協助出題每天限用 1 次，今日額度已使用完畢'
      throw new Error(error.value)
    }

    isGenerating.value = true
    error.value = null
    try {
      // 暫時用假資料，之後接上真實 API 後移除
      await new Promise(resolve => setTimeout(resolve, 1200))
      generatedQuestions.value = Array.from({ length: questionCount }, (_, i) => ({
        id: `ai-q-${Date.now()}-${i}`,
        text: `根據你貼上的內容，AI 產生的範例題目 ${i + 1}？`,
        options: ['選項 A', '選項 B', '選項 C', '選項 D'],
        answerIndex: 0,
        explanation: `這是 AI 針對第 ${i + 1} 題產生的解析說明（難易度：${difficulty}）。`
      }))
      recordAiUsage()
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
    aiUsesToday, canUseAiToday,
    fetchQuizzes, fetchQuizById,
    generateQuestions, saveQuiz,
    setCurrentQuiz, updateGeneratedQuestion, clearGenerated
  }
}, {
  persist: {
    pick: ['aiUsedDate', 'aiUsedCount']
  }
})
