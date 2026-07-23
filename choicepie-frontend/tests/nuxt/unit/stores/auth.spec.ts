import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import type { MemberDto } from '~/types/api'
import type { LoginSchema, RegisterSchema } from '~/types/auth'
import { authClientMock, navigateToMock } from './mocks/authClient.mock'

const { register, loginWithEmail, logout, refresh } = authClientMock
const navigateTo = navigateToMock

const { useAuthStore } = await import('~/stores/auth')

const member: MemberDto = {
  id: 'member-1',
  email: 'alice@example.com',
  name: 'Alice',
  avatar: null,
  isVerified: true,
  createdAt: '2026-01-01T00:00:00Z'
}

describe('useAuthStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  describe('isLoggedIn', () => {
    it('user 為 null 時為 false', () => {
      const store = useAuthStore()
      expect(store.isLoggedIn).toBe(false)
    })

    it('user 存在時為 true', () => {
      const store = useAuthStore()
      store.setUser({
        id: 'member-1',
        email: 'alice@example.com',
        name: 'Alice',
        isVerified: true,
        createdAt: '2026-01-01T00:00:00Z'
      })
      expect(store.isLoggedIn).toBe(true)
    })
  })

  describe('register', () => {
    it('成功時將 MemberDto 轉為 User 並存入 user', async () => {
      register.mockResolvedValue(member)
      const store = useAuthStore()
      const payload = { email: 'alice@example.com', password: 'secret', name: 'Alice' } as unknown as RegisterSchema

      await store.register(payload)

      expect(register).toHaveBeenCalledWith(payload)
      expect(store.user).toEqual({
        id: 'member-1',
        email: 'alice@example.com',
        name: 'Alice',
        avatar: undefined,
        isVerified: true,
        createdAt: '2026-01-01T00:00:00Z'
      })
      expect(store.isLoading).toBe(false)
    })

    it('失敗時不設定 user 並往外拋出例外', async () => {
      register.mockRejectedValue(new Error('email taken'))
      const store = useAuthStore()

      await expect(store.register({} as RegisterSchema)).rejects.toThrow('email taken')

      expect(store.user).toBeNull()
      expect(store.isLoading).toBe(false)
    })
  })

  describe('loginWithEmail', () => {
    it('成功時儲存使用者資料', async () => {
      loginWithEmail.mockResolvedValue(member)
      const store = useAuthStore()

      await store.loginWithEmail({ email: 'alice@example.com', password: 'secret' } as LoginSchema)

      expect(store.user?.email).toBe('alice@example.com')
    })

    it('avatar 為 null 時轉換成 undefined', async () => {
      loginWithEmail.mockResolvedValue({ ...member, avatar: null })
      const store = useAuthStore()

      await store.loginWithEmail({ email: 'alice@example.com', password: 'secret' } as LoginSchema)

      expect(store.user?.avatar).toBeUndefined()
    })
  })

  describe('logout', () => {
    it('清除 user 並導向預設路徑', async () => {
      logout.mockResolvedValue(undefined)
      const store = useAuthStore()
      store.setUser({
        id: 'member-1',
        email: 'alice@example.com',
        name: 'Alice',
        isVerified: true,
        createdAt: '2026-01-01T00:00:00Z'
      })

      await store.logout()

      expect(store.user).toBeNull()
      expect(navigateTo).toHaveBeenCalledWith('/login')
    })

    it('API 失敗時仍清除 user 並導向指定路徑', async () => {
      logout.mockRejectedValue(new Error('network error'))
      const store = useAuthStore()
      store.setUser({
        id: 'member-1',
        email: 'alice@example.com',
        name: 'Alice',
        isVerified: true,
        createdAt: '2026-01-01T00:00:00Z'
      })

      await store.logout('/')

      expect(store.user).toBeNull()
      expect(navigateTo).toHaveBeenCalledWith('/')
    })
  })

  describe('fetchMe', () => {
    it('成功時儲存使用者並回傳 true', async () => {
      refresh.mockResolvedValue(member)
      const store = useAuthStore()

      const result = await store.fetchMe()

      expect(result).toBe(true)
      expect(store.user?.id).toBe('member-1')
    })

    it('失敗時清除 user 並回傳 false', async () => {
      refresh.mockRejectedValue(new Error('unauthorized'))
      const store = useAuthStore()
      store.setUser({
        id: 'member-1',
        email: 'alice@example.com',
        name: 'Alice',
        isVerified: true,
        createdAt: '2026-01-01T00:00:00Z'
      })

      const result = await store.fetchMe()

      expect(result).toBe(false)
      expect(store.user).toBeNull()
    })
  })
})
