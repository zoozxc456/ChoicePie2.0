import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import type { StartAttemptResultDto, QuizAttemptResultDto } from '~/types/api'
import { quizAttemptClientMock } from './mocks/quizAttemptClient.mock'

const { startAttempt, submitAnswer, completeAttempt, fetchAttemptById } = quizAttemptClientMock

const { useQuizAttemptStore } = await import('~/stores/quizAttempt')

const startResult: StartAttemptResultDto = {
  attemptId: 'attempt-1',
  quiz: {
    quizId: 'quiz-1',
    title: 'Sample Quiz',
    questions: []
  } as unknown as StartAttemptResultDto['quiz']
}

const attemptResult: QuizAttemptResultDto = {
  id: 'attempt-1',
  quizId: 'quiz-1',
  quizTitle: 'Sample Quiz',
  memberId: 'member-1',
  score: 80,
  passed: true,
  startedAt: '2026-01-01T00:00:00Z',
  completedAt: '2026-01-01T00:05:00Z',
  answers: []
}

describe('useQuizAttemptStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    vi.spyOn(console, 'error').mockImplementation(() => {})
  })

  describe('startAttempt', () => {
    it('成功時儲存 currentAttempt 並回傳資料', async () => {
      startAttempt.mockResolvedValue(startResult)
      const store = useQuizAttemptStore()

      const result = await store.startAttempt('quiz-1')

      expect(startAttempt).toHaveBeenCalledWith('quiz-1')
      expect(result).toEqual(startResult)
      expect(store.currentAttempt).toEqual(startResult)
      expect(store.isLoading).toBe(false)
      expect(store.error).toBeNull()
    })

    it('失敗時設定 error 訊息並拋出例外', async () => {
      startAttempt.mockRejectedValue(new Error('network error'))
      const store = useQuizAttemptStore()

      await expect(store.startAttempt('quiz-1')).rejects.toThrow('network error')

      expect(store.error).toBe('無法開始作答，請稍後再試')
      expect(store.isLoading).toBe(false)
      expect(store.currentAttempt).toBeNull()
    })

    it('請求期間 isLoading 為 true', async () => {
      let resolvePromise: (v: StartAttemptResultDto) => void
      startAttempt.mockReturnValue(new Promise((resolve) => {
        resolvePromise = resolve
      }))
      const store = useQuizAttemptStore()

      const pending = store.startAttempt('quiz-1')
      expect(store.isLoading).toBe(true)

      resolvePromise!(startResult)
      await pending

      expect(store.isLoading).toBe(false)
    })
  })

  describe('submitAnswer', () => {
    it('呼叫 client 並傳入正確參數', async () => {
      submitAnswer.mockResolvedValue(undefined)
      const store = useQuizAttemptStore()

      await store.submitAnswer('attempt-1', 'question-1', 2)

      expect(submitAnswer).toHaveBeenCalledWith('attempt-1', 'question-1', 2)
    })
  })

  describe('completeAttempt', () => {
    it('成功時儲存 result', async () => {
      completeAttempt.mockResolvedValue(attemptResult)
      const store = useQuizAttemptStore()

      const result = await store.completeAttempt('attempt-1')

      expect(result).toEqual(attemptResult)
      expect(store.result).toEqual(attemptResult)
    })

    it('失敗時設定對應的 error 訊息', async () => {
      completeAttempt.mockRejectedValue(new Error('boom'))
      const store = useQuizAttemptStore()

      await expect(store.completeAttempt('attempt-1')).rejects.toThrow('boom')

      expect(store.error).toBe('無法完成作答，請稍後再試')
    })
  })

  describe('fetchAttemptById', () => {
    it('成功時儲存 result', async () => {
      fetchAttemptById.mockResolvedValue(attemptResult)
      const store = useQuizAttemptStore()

      const result = await store.fetchAttemptById('attempt-1')

      expect(result).toEqual(attemptResult)
      expect(store.result).toEqual(attemptResult)
    })

    it('失敗時設定對應的 error 訊息', async () => {
      fetchAttemptById.mockRejectedValue(new Error('boom'))
      const store = useQuizAttemptStore()

      await expect(store.fetchAttemptById('attempt-1')).rejects.toThrow('boom')

      expect(store.error).toBe('無法載入作答結果')
    })
  })

  describe('reset', () => {
    it('清空 currentAttempt、result、error', async () => {
      startAttempt.mockResolvedValue(startResult)
      completeAttempt.mockResolvedValue(attemptResult)
      const store = useQuizAttemptStore()
      await store.startAttempt('quiz-1')
      await store.completeAttempt('attempt-1')

      store.reset()

      expect(store.currentAttempt).toBeNull()
      expect(store.result).toBeNull()
      expect(store.error).toBeNull()
    })
  })
})
