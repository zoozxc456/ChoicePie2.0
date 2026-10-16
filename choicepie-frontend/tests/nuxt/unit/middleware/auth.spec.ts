import { describe, it, expect, beforeEach, vi } from 'vitest'
import type { RouteLocationNormalized } from 'vue-router'
import { useAuthStoreMock, navigateToMock } from './mocks/authMiddleware.mock'

const authMiddleware = (await import('~/middleware/auth')).default

const makeRoute = (fullPath: string): RouteLocationNormalized =>
  ({ fullPath } as RouteLocationNormalized)

describe('middleware/auth', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  // persisted 的 isLoggedIn 只代表曾經登入過，cookie 可能已過期，所以即使已登入也要用 fetchMe 向後端驗證。
  it('已登入時仍呼叫 fetchMe 驗證 session，驗證通過則不導向', async () => {
    const fetchMe = vi.fn().mockResolvedValue(true)
    useAuthStoreMock.mockReturnValue({ isLoggedIn: true, fetchMe })

    const result = await authMiddleware(makeRoute('/host/room/ABCD'), makeRoute('/'))

    expect(fetchMe).toHaveBeenCalledOnce()
    expect(navigateToMock).not.toHaveBeenCalled()
    expect(result).toBeUndefined()
  })

  it('未登入但 fetchMe 後恢復登入狀態時放行', async () => {
    const fetchMe = vi.fn().mockImplementation(async () => {
      authState.isLoggedIn = true
      return true
    })
    const authState = { isLoggedIn: false, fetchMe }
    useAuthStoreMock.mockReturnValue(authState)

    const result = await authMiddleware(makeRoute('/host/room/ABCD'), makeRoute('/'))

    expect(fetchMe).toHaveBeenCalled()
    expect(navigateToMock).not.toHaveBeenCalled()
    expect(result).toBeUndefined()
  })

  it('未登入且 fetchMe 後仍未登入時導向 login 並帶上 redirect 參數', async () => {
    const fetchMe = vi.fn().mockResolvedValue(false)
    useAuthStoreMock.mockReturnValue({ isLoggedIn: false, fetchMe })
    navigateToMock.mockReturnValue('navigated')

    const result = await authMiddleware(makeRoute('/host/room/ABCD?foo=bar'), makeRoute('/'))

    expect(fetchMe).toHaveBeenCalled()
    expect(navigateToMock).toHaveBeenCalledWith('/login?redirect=%2Fhost%2Froom%2FABCD%3Ffoo%3Dbar')
    expect(result).toBe('navigated')
  })
})
