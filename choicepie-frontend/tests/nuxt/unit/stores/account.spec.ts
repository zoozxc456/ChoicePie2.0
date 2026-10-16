import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import type { AccountDto } from '~/types/account'
import { accountClientMock } from './mocks/accountClient.mock'
import { requestIdTokenMock } from './mocks/authClient.mock'

const { useAccountStore } = await import('~/stores/account')
const { useAuthStore } = await import('~/stores/auth')

const makeAccount = (overrides: Partial<AccountDto> = {}): AccountDto => ({
  id: 'm1',
  name: 'Alice',
  email: 'alice@example.com',
  avatar: null,
  isVerified: true,
  hasPassword: true,
  googleLinked: false,
  ...overrides
})

const signIn = () => {
  const auth = useAuthStore()
  auth.setUser({ id: 'm1', email: 'alice@example.com', name: 'Alice', isVerified: true, createdAt: '2026-01-01T00:00:00Z' })
  return auth
}

describe('useAccountStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('fetchAccount 儲存帳號資料', async () => {
    accountClientMock.fetchAccount.mockResolvedValue(makeAccount())
    const store = useAccountStore()

    await store.fetchAccount()

    expect(store.account?.email).toBe('alice@example.com')
    expect(store.isLoading).toBe(false)
  })

  it('updateName 同步更新 account 與 auth.user（header 會跟著更新）', async () => {
    accountClientMock.fetchAccount.mockResolvedValue(makeAccount())
    accountClientMock.updateProfile.mockResolvedValue({ name: 'Bob', avatar: null })
    const auth = signIn()
    const store = useAccountStore()
    await store.fetchAccount()

    await store.updateName('Bob')

    expect(accountClientMock.updateProfile).toHaveBeenCalledWith('Bob')
    expect(store.account?.name).toBe('Bob')
    expect(auth.user?.name).toBe('Bob')
  })

  it('uploadAvatar 同步更新 auth.user.avatar', async () => {
    accountClientMock.uploadAvatar.mockResolvedValue({ name: 'Alice', avatar: 'https://cdn/a.png' })
    const auth = signIn()
    const store = useAccountStore()
    const file = new File(['x'], 'a.png', { type: 'image/png' })

    await store.uploadAvatar(file)

    expect(accountClientMock.uploadAvatar).toHaveBeenCalledWith(file)
    expect(auth.user?.avatar).toBe('https://cdn/a.png')
  })

  it('changePassword 成功後 hasPassword 為 true（純 Google 帳號設定密碼）', async () => {
    accountClientMock.fetchAccount.mockResolvedValue(makeAccount({ hasPassword: false, googleLinked: true }))
    accountClientMock.changePassword.mockResolvedValue(undefined)
    const store = useAccountStore()
    await store.fetchAccount()

    await store.changePassword({ newPassword: 'new-password' })

    expect(store.account?.hasPassword).toBe(true)
  })

  it('changePassword 失敗時拋出錯誤且不改變 hasPassword', async () => {
    accountClientMock.fetchAccount.mockResolvedValue(makeAccount({ hasPassword: false }))
    accountClientMock.changePassword.mockRejectedValue(new Error('bad'))
    const store = useAccountStore()
    await store.fetchAccount()

    await expect(store.changePassword({ newPassword: 'x' })).rejects.toThrow('bad')
    expect(store.account?.hasPassword).toBe(false)
  })

  it('linkGoogle 先取得 idToken 再呼叫 API', async () => {
    accountClientMock.fetchAccount.mockResolvedValue(makeAccount())
    requestIdTokenMock.mockResolvedValue('google-token')
    accountClientMock.linkGoogle.mockResolvedValue(undefined)
    const store = useAccountStore()
    await store.fetchAccount()

    await store.linkGoogle()

    expect(accountClientMock.linkGoogle).toHaveBeenCalledWith('google-token')
    expect(store.account?.googleLinked).toBe(true)
  })

  it('unlinkGoogle 成功後 googleLinked 為 false', async () => {
    accountClientMock.fetchAccount.mockResolvedValue(makeAccount({ googleLinked: true }))
    accountClientMock.unlinkGoogle.mockResolvedValue(undefined)
    const store = useAccountStore()
    await store.fetchAccount()

    await store.unlinkGoogle()

    expect(store.account?.googleLinked).toBe(false)
  })

  it('deleteAccount 成功後清除 account 與登入狀態', async () => {
    accountClientMock.fetchAccount.mockResolvedValue(makeAccount())
    accountClientMock.deleteAccount.mockResolvedValue(undefined)
    const auth = signIn()
    const store = useAccountStore()
    await store.fetchAccount()

    await store.deleteAccount({ password: 'pw' })

    expect(accountClientMock.deleteAccount).toHaveBeenCalledWith({ password: 'pw' })
    expect(store.account).toBeNull()
    expect(auth.user).toBeNull()
  })

  it('deleteAccount 失敗時保留登入狀態', async () => {
    accountClientMock.deleteAccount.mockRejectedValue(new Error('wrong password'))
    const auth = signIn()
    const store = useAccountStore()

    await expect(store.deleteAccount({ password: 'bad' })).rejects.toThrow('wrong password')
    expect(auth.user).not.toBeNull()
  })
})
