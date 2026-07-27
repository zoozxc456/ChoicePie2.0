import { describe, it, expect, beforeEach, vi } from 'vitest'
import { apiMock } from './mocks/useApi.mock'
import { useAdminQuizClientApi } from '~/services/admin/quiz'

describe('useAdminQuizClientApi', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('fetchQuizzes 帶入查詢參數', () => {
    const client = useAdminQuizClientApi()

    client.fetchQuizzes({ search: 'foo' })

    expect(apiMock.get).toHaveBeenCalledWith('/api/v1/admin/quizzes', { search: 'foo' })
  })

  it('takeDownQuiz 呼叫正確路徑與 body', () => {
    const client = useAdminQuizClientApi()

    client.takeDownQuiz('quiz1', 'inappropriate content')

    expect(apiMock.post).toHaveBeenCalledWith('/api/v1/admin/quizzes/quiz1/takedown', { reason: 'inappropriate content' })
  })

  it('restoreQuiz 呼叫正確路徑', () => {
    const client = useAdminQuizClientApi()

    client.restoreQuiz('quiz1')

    expect(apiMock.post).toHaveBeenCalledWith('/api/v1/admin/quizzes/quiz1/restore')
  })
})
