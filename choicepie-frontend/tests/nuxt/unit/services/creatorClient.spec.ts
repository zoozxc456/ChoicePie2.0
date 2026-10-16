import { describe, it, expect, beforeEach, vi } from 'vitest'
import { apiMock } from './mocks/useApi.mock'
import { useCreatorClientApi } from '~/services/creator/client'

describe('useCreatorClientApi', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('fetchCreatorProfile 呼叫正確路徑', () => {
    apiMock.get.mockResolvedValue({ id: 'c1' })
    const client = useCreatorClientApi()

    client.fetchCreatorProfile('c1')

    expect(apiMock.get).toHaveBeenCalledWith('/api/v1/creators/c1')
  })

  it('fetchCreatorQuizzes 帶分頁參數', () => {
    const client = useCreatorClientApi()

    client.fetchCreatorQuizzes('c1', 2, 6)

    expect(apiMock.get).toHaveBeenCalledWith('/api/v1/creators/c1/quizzes', { page: 2, pageSize: 6 })
  })

  it('fetchFollowing 呼叫 me/following', () => {
    const client = useCreatorClientApi()

    client.fetchFollowing(1, 20)

    expect(apiMock.get).toHaveBeenCalledWith('/api/v1/me/following', { page: 1, pageSize: 20 })
  })

  it('fetchFollowingQuizzes 呼叫 me/following/quizzes', () => {
    const client = useCreatorClientApi()

    client.fetchFollowingQuizzes(3, 12)

    expect(apiMock.get).toHaveBeenCalledWith('/api/v1/me/following/quizzes', { page: 3, pageSize: 12 })
  })

  it('followCreator 呼叫 PUT', () => {
    const client = useCreatorClientApi()

    client.followCreator('c1')

    expect(apiMock.put).toHaveBeenCalledWith('/api/v1/creators/c1/follow')
  })

  it('unfollowCreator 呼叫 DELETE', () => {
    const client = useCreatorClientApi()

    client.unfollowCreator('c1')

    expect(apiMock.del).toHaveBeenCalledWith('/api/v1/creators/c1/follow')
  })
})
