import { vi } from 'vitest'

export const creatorClientMock = {
  fetchCreatorProfile: vi.fn(),
  followCreator: vi.fn(),
  unfollowCreator: vi.fn()
}

vi.mock('~/services/creator/client', () => ({
  useCreatorClientApi: () => creatorClientMock
}))
