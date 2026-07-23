import { describe, it, expect, beforeEach, vi } from 'vitest'
import { apiMock } from './mocks/useApi.mock'
import { useGameSessionClientApi } from '~/services/gameSession/client'

describe('useGameSessionClientApi', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('fetchHostedSessions 呼叫正確路徑', () => {
    const client = useGameSessionClientApi()

    client.fetchHostedSessions()

    expect(apiMock.get).toHaveBeenCalledWith('/api/v1/game-sessions/hosted')
  })

  it('fetchPlayedSessions 呼叫正確路徑', () => {
    const client = useGameSessionClientApi()

    client.fetchPlayedSessions()

    expect(apiMock.get).toHaveBeenCalledWith('/api/v1/game-sessions/played')
  })

  it('fetchSessionById 呼叫正確路徑', () => {
    const client = useGameSessionClientApi()

    client.fetchSessionById('s1')

    expect(apiMock.get).toHaveBeenCalledWith('/api/v1/game-sessions/s1')
  })
})
