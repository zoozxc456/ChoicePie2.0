import { describe, it, expect, beforeEach, vi } from 'vitest'
import { apiMock } from './mocks/useApi.mock'
import { useAuthClientApi } from '~/services/auth'

describe('useAuthClientApi', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('register 呼叫正確路徑與 payload', () => {
    const client = useAuthClientApi()
    const payload = { email: 'a@b.com', password: 'pw', confirmPassword: 'pw', name: 'Alice' }

    client.register(payload)

    expect(apiMock.post).toHaveBeenCalledWith('/api/v1/auth/register', payload)
  })

  it('loginWithEmail 呼叫正確路徑與 payload', () => {
    const client = useAuthClientApi()
    const payload = { email: 'a@b.com', password: 'pw' }

    client.loginWithEmail(payload)

    expect(apiMock.post).toHaveBeenCalledWith('/api/v1/auth/login', payload)
  })

  it('logout 呼叫正確路徑', () => {
    const client = useAuthClientApi()

    client.logout()

    expect(apiMock.post).toHaveBeenCalledWith('/api/v1/auth/logout')
  })

  it('refresh 呼叫正確路徑', () => {
    const client = useAuthClientApi()

    client.refresh()

    expect(apiMock.post).toHaveBeenCalledWith('/api/v1/auth/refresh')
  })

  it('forgotPassword 呼叫正確路徑與 payload', () => {
    const client = useAuthClientApi()
    const payload = { email: 'a@b.com' }

    client.forgotPassword(payload)

    expect(apiMock.post).toHaveBeenCalledWith('/api/v1/auth/forgot-password', payload)
  })

  it('resetPassword 呼叫正確路徑與 payload', () => {
    const client = useAuthClientApi()
    const payload = { token: 'raw-token', password: 'newpass123', confirmPassword: 'newpass123' }

    client.resetPassword(payload)

    expect(apiMock.post).toHaveBeenCalledWith('/api/v1/auth/reset-password', payload)
  })

  it('verifyEmail 呼叫正確路徑與 body', () => {
    const client = useAuthClientApi()

    client.verifyEmail('raw-token')

    expect(apiMock.post).toHaveBeenCalledWith('/api/v1/auth/verify-email', { token: 'raw-token' })
  })

  it('resendVerification 呼叫正確路徑', () => {
    const client = useAuthClientApi()

    client.resendVerification()

    expect(apiMock.post).toHaveBeenCalledWith('/api/v1/auth/resend-verification')
  })
})
