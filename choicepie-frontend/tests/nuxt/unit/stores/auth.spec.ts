import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import type { MemberDto } from '~/types/api'
import type { LoginSchema, RegisterSchema } from '~/types/auth'
import { authClientMock, navigateToMock } from './mocks/authClient.mock'

const { register, loginWithEmail, logout, refresh, forgotPassword, resetPassword, verifyEmail, resendVerification } = authClientMock
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

  describe('forgotPassword', () => {
    it('成功時呼叫 API', async () => {
      forgotPassword.mockResolvedValue(undefined)
      const store = useAuthStore()

      await store.forgotPassword({ email: 'alice@example.com' })

      expect(forgotPassword).toHaveBeenCalledWith({ email: 'alice@example.com' })
      expect(store.isForgotPasswordLoading).toBe(false)
    })

    it('失敗時設定 error 並往外拋出', async () => {
      forgotPassword.mockRejectedValue(new Error('boom'))
      const store = useAuthStore()

      await expect(store.forgotPassword({ email: 'alice@example.com' })).rejects.toThrow('boom')

      expect(store.error).toBe('發送失敗，請稍後再試')
    })
  })

  describe('resetPassword', () => {
    it('成功時呼叫 API', async () => {
      resetPassword.mockResolvedValue(undefined)
      const store = useAuthStore()
      const payload = { token: 'raw-token', password: 'newpass123', confirmPassword: 'newpass123' }

      await store.resetPassword(payload)

      expect(resetPassword).toHaveBeenCalledWith(payload)
      expect(store.isResetPasswordLoading).toBe(false)
    })

    it('失敗時設定 error 並往外拋出', async () => {
      resetPassword.mockRejectedValue(new Error('boom'))
      const store = useAuthStore()

      await expect(store.resetPassword({ token: 't', password: 'p', confirmPassword: 'p' })).rejects.toThrow('boom')

      expect(store.error).toBe('重設密碼失敗，連結可能已失效')
    })
  })

  describe('verifyEmail', () => {
    it('成功時呼叫 API 並更新 user.isVerified', async () => {
      verifyEmail.mockResolvedValue(undefined)
      const store = useAuthStore()
      store.setUser({
        id: 'member-1', email: 'alice@example.com', name: 'Alice', isVerified: false, createdAt: '2026-01-01T00:00:00Z'
      })

      await store.verifyEmail('raw-token')

      expect(verifyEmail).toHaveBeenCalledWith('raw-token')
      expect(store.user?.isVerified).toBe(true)
    })

    it('失敗時設定 error 並往外拋出', async () => {
      verifyEmail.mockRejectedValue(new Error('boom'))
      const store = useAuthStore()

      await expect(store.verifyEmail('bad-token')).rejects.toThrow('boom')

      expect(store.error).toBe('驗證失敗，連結可能已失效')
    })
  })

  describe('resendVerification', () => {
    it('成功時呼叫 API', async () => {
      resendVerification.mockResolvedValue(undefined)
      const store = useAuthStore()

      await store.resendVerification()

      expect(resendVerification).toHaveBeenCalled()
      expect(store.isResendVerificationLoading).toBe(false)
    })

    it('失敗時設定 error 並往外拋出', async () => {
      resendVerification.mockRejectedValue(new Error('boom'))
      const store = useAuthStore()

      await expect(store.resendVerification()).rejects.toThrow('boom')

      expect(store.error).toBe('發送失敗，請稍後再試')
    })
  })
})
