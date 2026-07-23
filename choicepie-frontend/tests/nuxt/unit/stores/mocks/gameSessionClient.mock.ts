import { vi } from 'vitest'

export const gameSessionClientMock = {
  fetchHostedSessions: vi.fn(),
  fetchPlayedSessions: vi.fn(),
  fetchSessionById: vi.fn()
}

vi.mock('~/services/gameSession/client', () => ({
  useGameSessionClientApi: () => gameSessionClientMock
}))
