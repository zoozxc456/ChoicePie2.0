import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import type { QuizDto, QuizSummaryDto, PagedResult, CommentDto, GenerateQuestionsResultDto } from '~/types/api'
import { quizClientMock } from './mocks/quizClient.mock'

const {
  fetchQuizzes, fetchQuizById,
  updateQuiz, deleteQuiz,
  fetchFavoriteStatus, addFavorite, removeFavorite,
  fetchComments, addComment,
  generateQuestions, saveQuiz
} = quizClientMock

const { useQuizStore } = await import('~/stores/quiz')

const makeQuizDto = (overrides: Partial<QuizDto> = {}): QuizDto => ({
  id: 'quiz-1',
  title: 'Sample Quiz',
  description: null,
  coverEmoji: '📝',
  coverGradient: 'linear-gradient(135deg,#0f3460,#533483)',
  difficulty: 'beginner',
  status: 'Draft',
  challengeCount: 0,
  passRate: 0,
  creatorId: 'creator-1',
  creatorName: 'Alice',
  creatorAvatar: null,
  questions: [],
  tags: [],
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: '2026-01-01T00:00:00Z',
  ...overrides
})

const makeQuizSummaryDto = (overrides: Partial<QuizSummaryDto> = {}): QuizSummaryDto => ({
  id: 'quiz-1',
  title: 'Sample Quiz',
  description: null,
  coverEmoji: '📝',
  coverGradient: 'linear-gradient(135deg,#0f3460,#533483)',
  difficulty: 'beginner',
  status: 'Published',
  questionCount: 3,
  challengeCount: 0,
  passRate: 0,
  creatorId: 'creator-1',
  creatorName: 'Alice',
  creatorAvatar: null,
  tags: [],
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: '2026-01-01T00:00:00Z',
  ...overrides
})

const makeComment = (overrides: Partial<CommentDto> = {}): CommentDto => ({
  id: 'comment-1',
  quizId: 'quiz-1',
  userId: 'user-1',
  userName: 'Bob',
  userAvatar: null,
  text: 'Great quiz!',
  createdAt: '2026-01-01T00:00:00Z',
  ...overrides
})

describe('useQuizStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    vi.spyOn(console, 'error').mockImplementation(() => {})
  })

  describe('fetchQuizzes', () => {
    it('成功時將 summary 轉換為 Quiz 並存入 quizzes', async () => {
      const paged: PagedResult<QuizSummaryDto> = {
        pageNumber: 1, pageSize: 20, totalCount: 1, items: [makeQuizSummaryDto()]
      }
      fetchQuizzes.mockResolvedValue(paged)
      const store = useQuizStore()

      await store.fetchQuizzes()

      expect(store.quizzes).toHaveLength(1)
      expect(store.quizzes[0]?.id).toBe('quiz-1')
      expect(store.quizzes[0]?.isPublic).toBe(true)
      expect(store.quizzes[0]?.questions).toEqual([])
    })

    it('失敗時設定 error 訊息', async () => {
      fetchQuizzes.mockRejectedValue(new Error('boom'))
      const store = useQuizStore()

      await store.fetchQuizzes()

      expect(store.error).toBe('載入題庫失敗，請稍後再試')
      expect(store.quizzes).toEqual([])
    })
  })

  describe('fetchQuizById', () => {
    it('成功時將 QuizDto 轉為 Quiz 並存入 currentQuiz', async () => {
      fetchQuizById.mockResolvedValue(makeQuizDto({
        questions: [{ id: 'q1', text: 'Q1', options: ['a', 'b'], answerIndex: 0, explanation: 'exp' }]
      }))
      const store = useQuizStore()

      await store.fetchQuizById('quiz-1')

      expect(store.currentQuiz?.id).toBe('quiz-1')
      expect(store.currentQuiz?.questions).toHaveLength(1)
      expect(store.currentQuiz?.isPublic).toBe(false)
    })

    it('失敗時設定 error 訊息', async () => {
      fetchQuizById.mockRejectedValue(new Error('not found'))
      const store = useQuizStore()

      await store.fetchQuizById('quiz-1')

      expect(store.error).toBe('無法載入此題庫')
      expect(store.currentQuiz).toBeNull()
    })
  })

  describe('updateQuiz', () => {
    it('更新 currentQuiz 與 quizzes 清單中對應的項目', async () => {
      const store = useQuizStore()
      store.setCurrentQuiz({
        id: 'quiz-1', title: 'Old', coverEmoji: '📝', coverGradient: 'g',
        difficulty: 'beginner', questionCount: 0, challengeCount: 0, passRate: 0,
        creatorId: 'c1', creatorName: 'Alice', questions: [], tags: [],
        isPublic: false, status: 'Draft', createdAt: 't', updatedAt: 't'
      })
      updateQuiz.mockResolvedValue(makeQuizDto({ title: 'New Title' }))

      const result = await store.updateQuiz('quiz-1', { title: 'New Title', description: null, tags: [] })

      expect(result.title).toBe('New Title')
      expect(store.currentQuiz?.title).toBe('New Title')
    })

    it('不影響其他 id 的 currentQuiz', async () => {
      const store = useQuizStore()
      store.setCurrentQuiz({
        id: 'quiz-2', title: 'Other', coverEmoji: '📝', coverGradient: 'g',
        difficulty: 'beginner', questionCount: 0, challengeCount: 0, passRate: 0,
        creatorId: 'c1', creatorName: 'Alice', questions: [], tags: [],
        isPublic: false, status: 'Draft', createdAt: 't', updatedAt: 't'
      })
      updateQuiz.mockResolvedValue(makeQuizDto({ id: 'quiz-1', title: 'New Title' }))

      await store.updateQuiz('quiz-1', { title: 'New Title', description: null, tags: [] })

      expect(store.currentQuiz?.title).toBe('Other')
    })
  })

  describe('deleteQuiz', () => {
    it('從 quizzes 移除並清空符合 id 的 currentQuiz', async () => {
      deleteQuiz.mockResolvedValue(undefined)
      const store = useQuizStore()
      store.quizzes.push({
        id: 'quiz-1', title: 'Sample', coverEmoji: '📝', coverGradient: 'g',
        difficulty: 'beginner', questionCount: 0, challengeCount: 0, passRate: 0,
        creatorId: 'c1', creatorName: 'Alice', questions: [], tags: [],
        isPublic: false, status: 'Draft', createdAt: 't', updatedAt: 't'
      })
      store.setCurrentQuiz(store.quizzes[0]!)

      await store.deleteQuiz('quiz-1')

      expect(store.quizzes).toHaveLength(0)
      expect(store.currentQuiz).toBeNull()
    })
  })

  describe('fetchFavoriteStatus / toggleFavorite', () => {
    it('fetchFavoriteStatus 成功時更新 isFavorited', async () => {
      fetchFavoriteStatus.mockResolvedValue(true)
      const store = useQuizStore()

      await store.fetchFavoriteStatus('quiz-1')

      expect(store.isFavorited).toBe(true)
    })

    it('toggleFavorite 從未收藏切換到收藏會呼叫 addFavorite', async () => {
      addFavorite.mockResolvedValue(undefined)
      const store = useQuizStore()

      await store.toggleFavorite('quiz-1')

      expect(addFavorite).toHaveBeenCalledWith('quiz-1')
      expect(store.isFavorited).toBe(true)
    })

    it('toggleFavorite 已收藏時切換會呼叫 removeFavorite', async () => {
      removeFavorite.mockResolvedValue(undefined)
      const store = useQuizStore()
      store.isFavorited = true

      await store.toggleFavorite('quiz-1')

      expect(removeFavorite).toHaveBeenCalledWith('quiz-1')
      expect(store.isFavorited).toBe(false)
    })

    it('toggleFavorite 失敗時還原狀態並設定 error、往外拋出', async () => {
      addFavorite.mockRejectedValue(new Error('boom'))
      const store = useQuizStore()

      await expect(store.toggleFavorite('quiz-1')).rejects.toThrow('boom')

      expect(store.isFavorited).toBe(false)
      expect(store.error).toBe('操作失敗，請稍後再試')
      expect(store.isTogglingFavorite).toBe(false)
    })
  })

  describe('comments', () => {
    it('fetchComments 成功時儲存留言列表', async () => {
      fetchComments.mockResolvedValue([makeComment()])
      const store = useQuizStore()

      await store.fetchComments('quiz-1')

      expect(store.comments).toHaveLength(1)
      expect(store.isLoadingComments).toBe(false)
    })

    it('addComment 成功時將新留言插入最前面', async () => {
      const existing = makeComment({ id: 'comment-0', text: 'first' })
      addComment.mockResolvedValue(makeComment({ id: 'comment-1', text: 'second' }))
      const store = useQuizStore()
      store.comments.push(existing)

      const result = await store.addComment('quiz-1', 'second')

      expect(result.text).toBe('second')
      expect(store.comments[0]?.text).toBe('second')
      expect(store.comments[1]?.text).toBe('first')
    })

    it('addComment 失敗時設定 error 並往外拋出', async () => {
      addComment.mockRejectedValue(new Error('boom'))
      const store = useQuizStore()

      await expect(store.addComment('quiz-1', 'hi')).rejects.toThrow('boom')

      expect(store.error).toBe('留言送出失敗，請稍後再試')
      expect(store.isPostingComment).toBe(false)
    })
  })

  describe('AI 出題每日額度', () => {
    beforeEach(() => {
      vi.useFakeTimers()
      vi.setSystemTime(new Date('2026-01-01T00:00:00Z'))
    })

    afterEach(() => {
      vi.useRealTimers()
    })

    const generatedResult: GenerateQuestionsResultDto = {
      tokensUsed: 100,
      questions: [{ text: 'Q1', options: ['a', 'b'], answerIndex: 0, explanation: 'exp' }]
    }

    it('canUseAiToday 初始為 true', () => {
      const store = useQuizStore()
      expect(store.canUseAiToday).toBe(true)
      expect(store.aiUsesToday).toBe(0)
    })

    it('generateQuestions 成功後 canUseAiToday 變 false（每日限用 1 次）', async () => {
      generateQuestions.mockResolvedValue(generatedResult)
      const store = useQuizStore()

      await store.generateQuestions('content', 3, 'beginner')

      expect(store.generatedQuestions).toHaveLength(1)
      expect(store.aiUsesToday).toBe(1)
      expect(store.canUseAiToday).toBe(false)
    })

    it('額度用完時再次呼叫會直接拋出例外、不呼叫 API', async () => {
      generateQuestions.mockResolvedValue(generatedResult)
      const store = useQuizStore()
      await store.generateQuestions('content', 3, 'beginner')
      generateQuestions.mockClear()

      await expect(store.generateQuestions('content', 3, 'beginner')).rejects.toThrow()

      expect(generateQuestions).not.toHaveBeenCalled()
      expect(store.error).toBe('AI 協助出題每天限用 1 次，今日額度已使用完畢')
    })

    it('隔天額度會重置', async () => {
      generateQuestions.mockResolvedValue(generatedResult)
      const store = useQuizStore()
      await store.generateQuestions('content', 3, 'beginner')

      vi.setSystemTime(new Date('2026-01-02T00:00:00Z'))

      expect(store.canUseAiToday).toBe(true)
      expect(store.aiUsesToday).toBe(0)
    })

    it('generateQuestions API 失敗時設定 error 並往外拋出', async () => {
      generateQuestions.mockRejectedValue(new Error('boom'))
      const store = useQuizStore()

      await expect(store.generateQuestions('content', 3, 'beginner')).rejects.toThrow('boom')

      expect(store.error).toBe('AI 出題失敗，請稍後再試')
      expect(store.isGenerating).toBe(false)
    })
  })

  describe('saveQuiz', () => {
    it('成功時將新題庫加到 quizzes 最前面', async () => {
      saveQuiz.mockResolvedValue(makeQuizDto({ id: 'quiz-new', title: 'New Quiz' }))
      const store = useQuizStore()
      store.quizzes.push({
        id: 'quiz-old', title: 'Old', coverEmoji: '📝', coverGradient: 'g',
        difficulty: 'beginner', questionCount: 0, challengeCount: 0, passRate: 0,
        creatorId: 'c1', creatorName: 'Alice', questions: [], tags: [],
        isPublic: false, status: 'Draft', createdAt: 't', updatedAt: 't'
      })

      const result = await store.saveQuiz(
        [{ id: 'q1', text: 'Q1', options: ['a', 'b'], answerIndex: 0, explanation: 'exp' }],
        'New Quiz',
        'beginner'
      )

      expect(result.id).toBe('quiz-new')
      expect(store.quizzes[0]?.id).toBe('quiz-new')
      expect(store.quizzes).toHaveLength(2)
    })
  })

  describe('updateGeneratedQuestion / clearGenerated', () => {
    it('updateGeneratedQuestion 只更新指定索引的欄位', () => {
      const store = useQuizStore()
      store.generatedQuestions.push(
        { id: 'ai-1', text: 'Q1', options: ['a', 'b'], answerIndex: 0, explanation: 'exp1' },
        { id: 'ai-2', text: 'Q2', options: ['c', 'd'], answerIndex: 1, explanation: 'exp2' }
      )

      store.updateGeneratedQuestion(0, { text: 'Updated Q1' })

      expect(store.generatedQuestions[0]?.text).toBe('Updated Q1')
      expect(store.generatedQuestions[1]?.text).toBe('Q2')
    })

    it('索引不存在時不做任何事', () => {
      const store = useQuizStore()
      store.updateGeneratedQuestion(5, { text: 'noop' })
      expect(store.generatedQuestions).toEqual([])
    })

    it('clearGenerated 清空已產生的題目', () => {
      const store = useQuizStore()
      store.generatedQuestions.push({ id: 'ai-1', text: 'Q1', options: ['a', 'b'], answerIndex: 0, explanation: 'exp' })

      store.clearGenerated()

      expect(store.generatedQuestions).toEqual([])
    })
  })
})
