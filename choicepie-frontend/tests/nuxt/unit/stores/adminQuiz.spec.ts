import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import type { PagedResult, QuizSummaryDto } from '~/types/api'
import { adminQuizClientMock } from './mocks/adminQuizClient.mock'

const { fetchQuizzes, takeDownQuiz, restoreQuiz } = adminQuizClientMock

const { useAdminQuizStore } = await import('~/stores/adminQuiz')

const makeQuiz = (overrides: Partial<QuizSummaryDto> = {}): QuizSummaryDto => ({
  id: 'quiz-1',
  title: 'Sample Quiz',
  description: null,
  coverEmoji: '🎯',
  coverGradient: 'from-red-500 to-orange-500',
  difficulty: 'beginner',
  status: 'published',
  questionCount: 5,
  challengeCount: 0,
  passRate: 0,
  creatorId: 'creator-1',
  creatorName: 'Creator',
  creatorAvatar: null,
  tags: [],
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: '2026-01-01T00:00:00Z',
  ...overrides
})

const pagedResult: PagedResult<QuizSummaryDto> = {
  pageNumber: 1,
  pageSize: 20,
  totalCount: 1,
  items: [makeQuiz()]
}

describe('useAdminQuizStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    vi.spyOn(console, 'error').mockImplementation(() => {})
  })

  describe('fetchQuizzes', () => {
    it('成功時儲存 quizzes 並回傳資料', async () => {
      fetchQuizzes.mockResolvedValue(pagedResult)
      const store = useAdminQuizStore()

      const result = await store.fetchQuizzes({ search: 'foo' })

      expect(fetchQuizzes).toHaveBeenCalledWith({ search: 'foo' })
      expect(result).toEqual(pagedResult)
      expect(store.quizzes).toEqual(pagedResult)
    })

    it('失敗時設定 error 並往外拋出', async () => {
      fetchQuizzes.mockRejectedValue(new Error('boom'))
      const store = useAdminQuizStore()

      await expect(store.fetchQuizzes()).rejects.toThrow('boom')

      expect(store.error).toBe('無法載入題庫列表')
      expect(store.isLoading).toBe(false)
    })
  })

  describe('takeDownQuiz', () => {
    it('成功時將該題庫狀態更新為 takendown', async () => {
      fetchQuizzes.mockResolvedValue(pagedResult)
      takeDownQuiz.mockResolvedValue(undefined)
      const store = useAdminQuizStore()
      await store.fetchQuizzes()

      await store.takeDownQuiz('quiz-1', 'inappropriate content')

      expect(takeDownQuiz).toHaveBeenCalledWith('quiz-1', 'inappropriate content')
      expect(store.quizzes?.items[0]?.status).toBe('takendown')
    })

    it('失敗時設定 error 並往外拋出', async () => {
      takeDownQuiz.mockRejectedValue(new Error('boom'))
      const store = useAdminQuizStore()

      await expect(store.takeDownQuiz('quiz-1', 'reason')).rejects.toThrow('boom')

      expect(store.error).toBe('下架題庫失敗，請稍後再試')
    })
  })

  describe('restoreQuiz', () => {
    it('成功時將該題庫狀態更新為 draft', async () => {
      fetchQuizzes.mockResolvedValue({
        ...pagedResult,
        items: [makeQuiz({ status: 'takendown' })]
      })
      restoreQuiz.mockResolvedValue(undefined)
      const store = useAdminQuizStore()
      await store.fetchQuizzes()

      await store.restoreQuiz('quiz-1')

      expect(restoreQuiz).toHaveBeenCalledWith('quiz-1')
      expect(store.quizzes?.items[0]?.status).toBe('draft')
    })

    it('失敗時設定 error 並往外拋出', async () => {
      restoreQuiz.mockRejectedValue(new Error('boom'))
      const store = useAdminQuizStore()

      await expect(store.restoreQuiz('quiz-1')).rejects.toThrow('boom')

      expect(store.error).toBe('還原題庫失敗，請稍後再試')
    })
  })
})
