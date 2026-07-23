import { describe, it, expect, beforeEach, vi } from 'vitest'
import { apiMock } from './mocks/useApi.mock'
import { useQuizClientApi } from '~/services/quiz/client'

describe('useQuizClientApi', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('fetchQuizzes 帶入查詢參數', () => {
    const client = useQuizClientApi()

    client.fetchQuizzes({ tag: 'science', search: 'foo', mine: true })

    expect(apiMock.get).toHaveBeenCalledWith('/api/v1/quizzes', { tag: 'science', search: 'foo', mine: true })
  })

  it('fetchQuizById 呼叫正確路徑', () => {
    const client = useQuizClientApi()

    client.fetchQuizById('q1')

    expect(apiMock.get).toHaveBeenCalledWith('/api/v1/quizzes/q1')
  })

  it('fetchQuizPreview 呼叫正確路徑', () => {
    const client = useQuizClientApi()

    client.fetchQuizPreview('q1')

    expect(apiMock.get).toHaveBeenCalledWith('/api/v1/quizzes/q1/preview')
  })

  it('fetchTags 呼叫正確路徑', () => {
    const client = useQuizClientApi()

    client.fetchTags()

    expect(apiMock.get).toHaveBeenCalledWith('/api/v1/quizzes/tags')
  })

  it('updateQuiz 呼叫 PUT 並帶入 payload', () => {
    const client = useQuizClientApi()
    const payload = { title: 'T', description: null, tags: ['a'] }

    client.updateQuiz('q1', payload)

    expect(apiMock.put).toHaveBeenCalledWith('/api/v1/quizzes/q1', payload)
  })

  it('deleteQuiz 呼叫 DELETE', () => {
    const client = useQuizClientApi()

    client.deleteQuiz('q1')

    expect(apiMock.del).toHaveBeenCalledWith('/api/v1/quizzes/q1')
  })

  it('addQuestion 呼叫正確路徑與 body', () => {
    const client = useQuizClientApi()
    const question = { text: 'Q', options: ['a', 'b'], answerIndex: 0, explanation: '' }

    client.addQuestion('q1', question)

    expect(apiMock.post).toHaveBeenCalledWith('/api/v1/quizzes/q1/questions', question)
  })

  it('updateQuestion 呼叫正確路徑與 body', () => {
    const client = useQuizClientApi()
    const question = { text: 'Q', options: ['a', 'b'], answerIndex: 0, explanation: '' }

    client.updateQuestion('q1', 'question1', question)

    expect(apiMock.put).toHaveBeenCalledWith('/api/v1/quizzes/q1/questions/question1', question)
  })

  it('removeQuestion 呼叫正確路徑', () => {
    const client = useQuizClientApi()

    client.removeQuestion('q1', 'question1')

    expect(apiMock.del).toHaveBeenCalledWith('/api/v1/quizzes/q1/questions/question1')
  })

  it('publishQuiz / unpublishQuiz / archiveQuiz 呼叫正確路徑', () => {
    const client = useQuizClientApi()

    client.publishQuiz('q1')
    client.unpublishQuiz('q1')
    client.archiveQuiz('q1')

    expect(apiMock.post).toHaveBeenCalledWith('/api/v1/quizzes/q1/publish')
    expect(apiMock.post).toHaveBeenCalledWith('/api/v1/quizzes/q1/unpublish')
    expect(apiMock.post).toHaveBeenCalledWith('/api/v1/quizzes/q1/archive')
  })

  it('generateQuestions 呼叫正確路徑與 body', () => {
    const client = useQuizClientApi()

    client.generateQuestions('some content', 5, 'beginner')

    expect(apiMock.post).toHaveBeenCalledWith('/api/v1/quizzes/generate-questions', {
      content: 'some content',
      questionCount: 5,
      difficulty: 'beginner'
    })
  })

  it('saveQuiz 呼叫正確路徑與 payload', () => {
    const client = useQuizClientApi()
    const payload = {
      title: 'T',
      description: null,
      coverEmoji: '🎯',
      coverGradient: 'from-red-500 to-orange-500',
      difficulty: 'beginner' as const,
      tags: ['a'],
      questions: []
    }

    client.saveQuiz(payload)

    expect(apiMock.post).toHaveBeenCalledWith('/api/v1/quizzes', payload)
  })

  it('fetchFavoriteStatus 呼叫正確路徑', () => {
    const client = useQuizClientApi()

    client.fetchFavoriteStatus('q1')

    expect(apiMock.get).toHaveBeenCalledWith('/api/v1/quizzes/q1/favorite')
  })

  it('addFavorite / removeFavorite 呼叫正確路徑', () => {
    const client = useQuizClientApi()

    client.addFavorite('q1')
    client.removeFavorite('q1')

    expect(apiMock.put).toHaveBeenCalledWith('/api/v1/quizzes/q1/favorite')
    expect(apiMock.del).toHaveBeenCalledWith('/api/v1/quizzes/q1/favorite')
  })

  it('fetchComments 呼叫正確路徑', () => {
    const client = useQuizClientApi()

    client.fetchComments('q1')

    expect(apiMock.get).toHaveBeenCalledWith('/api/v1/quizzes/q1/comments')
  })

  it('addComment 呼叫正確路徑與 body', () => {
    const client = useQuizClientApi()

    client.addComment('q1', 'nice quiz')

    expect(apiMock.post).toHaveBeenCalledWith('/api/v1/quizzes/q1/comments', { text: 'nice quiz' })
  })

  it('fetchRelatedQuizzes 呼叫正確路徑', () => {
    const client = useQuizClientApi()

    client.fetchRelatedQuizzes('q1')

    expect(apiMock.get).toHaveBeenCalledWith('/api/v1/quizzes/q1/related')
  })

  it('recordShare 呼叫正確路徑', () => {
    const client = useQuizClientApi()

    client.recordShare('q1')

    expect(apiMock.post).toHaveBeenCalledWith('/api/v1/quizzes/q1/share')
  })
})
