import { describe, it, expect, beforeEach, vi } from 'vitest'
import { useAuthStoreMock, useRouteMock, navigateToMock } from './mocks/authNavigation.mock'

const { useAuthRedirect } = await import('~/composables/useAuthRedirect')

describe('useAuthRedirect', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    useRouteMock.mockReturnValue({ query: {} })
  })

  describe('redirect', () => {
    it('沒有 redirect query 時回題庫首頁', () => {
      useAuthStoreMock.mockReturnValue({ isLoggedIn: false })

      expect(useAuthRedirect().redirect.value).toBe('/library')
    })

    it('站內路徑原樣使用', () => {
      useAuthStoreMock.mockReturnValue({ isLoggedIn: false })
      useRouteMock.mockReturnValue({ query: { redirect: '/library/quiz-1' } })

      expect(useAuthRedirect().redirect.value).toBe('/library/quiz-1')
    })

    it('外部網址經 sanitizeRedirect 擋下，退回題庫首頁', () => {
      useAuthStoreMock.mockReturnValue({ isLoggedIn: false })
      useRouteMock.mockReturnValue({ query: { redirect: 'https://evil.example' } })

      expect(useAuthRedirect().redirect.value).toBe('/library')
    })
  })

  describe('goToRedirect', () => {
    it('導向 redirect 目標', async () => {
      useAuthStoreMock.mockReturnValue({ isLoggedIn: false })
      useRouteMock.mockReturnValue({ query: { redirect: '/history' } })

      await useAuthRedirect().goToRedirect()

      expect(navigateToMock).toHaveBeenCalledWith('/history')
    })
  })

  // persisted 的 isLoggedIn 只代表「曾經登入過」，須用 fetchMe() 驗證 session，避免 session 過期時形成 redirect loop。
  describe('redirectIfAuthenticated', () => {
    it('未登入時不呼叫 fetchMe 也不導向', async () => {
      const fetchMe = vi.fn()
      useAuthStoreMock.mockReturnValue({ isLoggedIn: false, fetchMe })

      await useAuthRedirect().redirectIfAuthenticated()

      expect(fetchMe).not.toHaveBeenCalled()
      expect(navigateToMock).not.toHaveBeenCalled()
    })

    it('session 仍有效時導向 redirect 目標', async () => {
      const fetchMe = vi.fn().mockResolvedValue(true)
      useAuthStoreMock.mockReturnValue({ isLoggedIn: true, fetchMe })
      useRouteMock.mockReturnValue({ query: { redirect: '/settings' } })

      await useAuthRedirect().redirectIfAuthenticated()

      expect(fetchMe).toHaveBeenCalledOnce()
      expect(navigateToMock).toHaveBeenCalledWith('/settings')
    })

    it('session 已失效時留在本頁', async () => {
      const fetchMe = vi.fn().mockResolvedValue(false)
      useAuthStoreMock.mockReturnValue({ isLoggedIn: true, fetchMe })

      await useAuthRedirect().redirectIfAuthenticated()

      expect(navigateToMock).not.toHaveBeenCalled()
    })
  })
})
