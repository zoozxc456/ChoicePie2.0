import { describe, it, expect, beforeEach, vi } from 'vitest'
import { apiMock } from './mocks/useApi.mock'
import { useQuizAttemptClientApi } from '~/services/quizAttempt/client'

describe('useQuizAttemptClientApi', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('startAttempt 呼叫正確路徑與 body', () => {
    const client = useQuizAttemptClientApi()

    client.startAttempt('quiz1')

    expect(apiMock.post).toHaveBeenCalledWith('/api/v1/quiz-attempts', { quizId: 'quiz1' })
  })

  it('submitAnswer 呼叫正確路徑與 body', () => {
    const client = useQuizAttemptClientApi()

    client.submitAnswer('attempt1', 'q1', 2)

    expect(apiMock.post).toHaveBeenCalledWith('/api/v1/quiz-attempts/attempt1/answers', {
      questionId: 'q1',
      selectedOptionIndex: 2
    })
  })

  it('completeAttempt 呼叫正確路徑', () => {
    const client = useQuizAttemptClientApi()

    client.completeAttempt('attempt1')

    expect(apiMock.post).toHaveBeenCalledWith('/api/v1/quiz-attempts/attempt1/complete')
  })

  it('fetchAttemptById 呼叫正確路徑', () => {
    const client = useQuizAttemptClientApi()

    client.fetchAttemptById('attempt1')

    expect(apiMock.get).toHaveBeenCalledWith('/api/v1/quiz-attempts/attempt1')
  })

  it('fetchAttemptHistory 呼叫正確路徑與查詢參數', () => {
    const client = useQuizAttemptClientApi()

    client.fetchAttemptHistory('quiz1')

    expect(apiMock.get).toHaveBeenCalledWith('/api/v1/quiz-attempts/history', { quizId: 'quiz1' })
  })
})
