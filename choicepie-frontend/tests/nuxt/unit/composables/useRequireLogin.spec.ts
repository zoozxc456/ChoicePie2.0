import { describe, it, expect, beforeEach, vi } from 'vitest'
import { useAuthStoreMock, useRouteMock, navigateToMock } from './mocks/authNavigation.mock'

const { useRequireLogin } = await import('~/composables/useRequireLogin')

describe('useRequireLogin', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    useRouteMock.mockReturnValue({ fullPath: '/library/quiz-1?ref=line' })
  })

  it('已登入時回傳 true 且不導向', async () => {
    useAuthStoreMock.mockReturnValue({ isLoggedIn: true })
    const { ensureLoggedIn } = useRequireLogin()

    expect(await ensureLoggedIn()).toBe(true)
    expect(navigateToMock).not.toHaveBeenCalled()
  })

  it('未登入時導向登入頁並帶上 encode 過的目前路徑，回傳 false', async () => {
    useAuthStoreMock.mockReturnValue({ isLoggedIn: false })
    const { ensureLoggedIn } = useRequireLogin()

    expect(await ensureLoggedIn()).toBe(false)
    expect(navigateToMock).toHaveBeenCalledWith(`/login?redirect=${encodeURIComponent('/library/quiz-1?ref=line')}`)
  })

  it('每次呼叫都讀取最新的登入狀態', async () => {
    const auth = { isLoggedIn: false }
    useAuthStoreMock.mockReturnValue(auth)
    const { ensureLoggedIn } = useRequireLogin()

    auth.isLoggedIn = true

    expect(await ensureLoggedIn()).toBe(true)
  })
})
