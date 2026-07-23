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

  it('已登入時不呼叫 fetchMe，也不導向', async () => {
    const fetchMe = vi.fn()
    useAuthStoreMock.mockReturnValue({ isLoggedIn: true, fetchMe })

    const result = await authMiddleware(makeRoute('/host/room/ABCD'), makeRoute('/'))

    expect(fetchMe).not.toHaveBeenCalled()
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
