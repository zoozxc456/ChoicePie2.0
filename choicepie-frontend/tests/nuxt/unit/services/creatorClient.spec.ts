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
