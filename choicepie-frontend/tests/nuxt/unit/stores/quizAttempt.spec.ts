import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import type { StartAttemptResultDto, QuizAttemptResultDto, QuizAttemptHistoryItemDto } from '~/types/api'
import { quizAttemptClientMock } from './mocks/quizAttemptClient.mock'

const { startAttempt, submitAnswer, completeAttempt, fetchAttemptById, fetchAttemptHistory } = quizAttemptClientMock

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

    it('作答中（尚未 completedAt）時不寫入 result，避免顯示過期的結果畫面', async () => {
      const inProgressResult: QuizAttemptResultDto = { ...attemptResult, completedAt: null }
      fetchAttemptById.mockResolvedValue(inProgressResult)
      const store = useQuizAttemptStore()

      const result = await store.fetchAttemptById('attempt-1')

      expect(result).toEqual(inProgressResult)
      expect(store.result).toBeNull()
    })

    it('失敗時設定對應的 error 訊息', async () => {
      fetchAttemptById.mockRejectedValue(new Error('boom'))
      const store = useQuizAttemptStore()

      await expect(store.fetchAttemptById('attempt-1')).rejects.toThrow('boom')

      expect(store.error).toBe('無法載入作答結果')
    })
  })

  describe('resumeAttempt', () => {
    const answer = (questionId: string, selectedOptionIndex: number | null) =>
      ({ questionId, selectedOptionIndex }) as unknown as QuizAttemptResultDto['answers'][number]

    it('已完成的作答直接回傳 null 並寫入 result，不重新開始', async () => {
      fetchAttemptById.mockResolvedValue(attemptResult)
      const store = useQuizAttemptStore()

      const index = await store.resumeAttempt('attempt-1')

      expect(index).toBeNull()
      expect(startAttempt).not.toHaveBeenCalled()
      expect(store.result).toEqual(attemptResult)
    })

    // 以 id 取得的作答中紀錄缺少選項資料，須用 startAttempt（後端會沿用同一筆作答）取回完整題目。
    it('作答中時以 quizId 重新 startAttempt，並回傳下一題（已作答題數）的索引', async () => {
      fetchAttemptById.mockResolvedValue({
        ...attemptResult,
        completedAt: null,
        answers: [answer('q1', 0), answer('q2', 2), answer('q3', null), answer('q4', null)]
      })
      startAttempt.mockResolvedValue(startResult)
      const store = useQuizAttemptStore()

      const index = await store.resumeAttempt('attempt-1')

      expect(startAttempt).toHaveBeenCalledWith('quiz-1')
      expect(store.currentAttempt).toEqual(startResult)
      expect(index).toBe(2)
    })

    it('全部題目都已作答但尚未完成時停在最後一題', async () => {
      fetchAttemptById.mockResolvedValue({
        ...attemptResult,
        completedAt: null,
        answers: [answer('q1', 0), answer('q2', 1)]
      })
      startAttempt.mockResolvedValue(startResult)
      const store = useQuizAttemptStore()

      expect(await store.resumeAttempt('attempt-1')).toBe(1)
    })

    it('取得作答失敗時拋出例外', async () => {
      fetchAttemptById.mockRejectedValue(new Error('not found'))
      const store = useQuizAttemptStore()

      await expect(store.resumeAttempt('attempt-1')).rejects.toThrow('not found')
      expect(startAttempt).not.toHaveBeenCalled()
    })
  })

  describe('fetchAttemptHistory', () => {
    const historyItems: QuizAttemptHistoryItemDto[] = [
      { id: 'attempt-1', score: 80, passed: true, startedAt: '2026-01-01T00:00:00Z', completedAt: '2026-01-01T00:05:00Z', durationMs: 12000 }
    ]

    it('成功時儲存 history 並回傳資料', async () => {
      fetchAttemptHistory.mockResolvedValue(historyItems)
      const store = useQuizAttemptStore()

      const result = await store.fetchAttemptHistory('quiz-1')

      expect(fetchAttemptHistory).toHaveBeenCalledWith('quiz-1')
      expect(result).toEqual(historyItems)
      expect(store.history).toEqual(historyItems)
      expect(store.isLoadingHistory).toBe(false)
    })

    it('失敗時往外拋出例外', async () => {
      fetchAttemptHistory.mockRejectedValue(new Error('boom'))
      const store = useQuizAttemptStore()

      await expect(store.fetchAttemptHistory('quiz-1')).rejects.toThrow('boom')

      expect(store.isLoadingHistory).toBe(false)
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
