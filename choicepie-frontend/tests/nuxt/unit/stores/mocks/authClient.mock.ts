import { vi } from 'vitest'
import { mockNuxtImport } from '@nuxt/test-utils/runtime'

const authClientMock = vi.hoisted(() => ({
  register: vi.fn(),
  loginWithEmail: vi.fn(),
  loginWithGoogle: vi.fn(),
  logout: vi.fn(),
  refresh: vi.fn(),
  forgotPassword: vi.fn(),
  resetPassword: vi.fn(),
  verifyEmail: vi.fn(),
  resendVerification: vi.fn()
}))

const navigateToMock = vi.hoisted(() => vi.fn())
const requestIdTokenMock = vi.hoisted(() => vi.fn())

vi.mock('~/services/auth', () => ({
  useAuthClientApi: () => authClientMock
}))

mockNuxtImport('navigateTo', () => navigateToMock)
mockNuxtImport('useGoogleIdentity', () => () => ({ requestIdToken: requestIdTokenMock }))

export { authClientMock, navigateToMock, requestIdTokenMock }
