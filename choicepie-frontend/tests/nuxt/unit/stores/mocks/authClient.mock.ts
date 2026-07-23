import { vi } from 'vitest'
import { mockNuxtImport } from '@nuxt/test-utils/runtime'

const authClientMock = vi.hoisted(() => ({
  register: vi.fn(),
  loginWithEmail: vi.fn(),
  logout: vi.fn(),
  refresh: vi.fn()
}))

const navigateToMock = vi.hoisted(() => vi.fn())

vi.mock('~/services/auth', () => ({
  useAuthClientApi: () => authClientMock
}))

mockNuxtImport('navigateTo', () => navigateToMock)

export { authClientMock, navigateToMock }
