import { vi } from 'vitest'

export const apiMock = {
  get: vi.fn(),
  post: vi.fn(),
  put: vi.fn(),
  del: vi.fn()
}

vi.mock('~/composables/useApi', () => ({
  useApi: () => apiMock
}))
