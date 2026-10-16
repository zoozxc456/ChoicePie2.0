import { describe, it, expect, beforeEach, vi } from 'vitest'
import { apiMock } from './mocks/useApi.mock'
import { useAccountClientApi } from '~/services/account/client'

describe('useAccountClientApi', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('fetchAccount 呼叫 GET /account', () => {
    useAccountClientApi().fetchAccount()
    expect(apiMock.get).toHaveBeenCalledWith('/api/v1/account')
  })

  it('updateProfile 以 PUT 送出 name', () => {
    useAccountClientApi().updateProfile('Alice')
    expect(apiMock.put).toHaveBeenCalledWith('/api/v1/account/profile', { name: 'Alice' })
  })

  it('changePassword 以 PUT 送出密碼 payload', () => {
    useAccountClientApi().changePassword({ currentPassword: 'old', newPassword: 'new-password' })
    expect(apiMock.put).toHaveBeenCalledWith('/api/v1/account/password', { currentPassword: 'old', newPassword: 'new-password' })
  })

  it('linkGoogle 送出 idToken', () => {
    useAccountClientApi().linkGoogle('token')
    expect(apiMock.post).toHaveBeenCalledWith('/api/v1/account/google', { idToken: 'token' })
  })

  it('unlinkGoogle 呼叫 DELETE /account/google', () => {
    useAccountClientApi().unlinkGoogle()
    expect(apiMock.del).toHaveBeenCalledWith('/api/v1/account/google')
  })

  it('deleteAccount 走 BFF 的 POST /account/delete', () => {
    useAccountClientApi().deleteAccount({ password: 'pw' })
    expect(apiMock.post).toHaveBeenCalledWith('/api/v1/account/delete', { password: 'pw' })
  })
})
