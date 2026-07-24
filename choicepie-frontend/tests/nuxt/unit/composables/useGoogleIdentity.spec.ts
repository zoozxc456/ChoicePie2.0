import { describe, it, expect, beforeEach, vi } from 'vitest'
import './mocks/useGoogleIdentity.mock'

const importUseGoogleIdentity = async () => {
  vi.resetModules()
  const mod = await import('~/composables/useGoogleIdentity')
  return mod
}

describe('useGoogleIdentity', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    document.head.innerHTML = ''
    // @ts-expect-error 測試環境重置全域 google 物件
    delete window.google
  })

  it('取得 ID Token 成功時回傳 credential', async () => {
    const initialize = vi.fn((config: { callback: (r: { credential: string }) => void }) => {
      config.callback({ credential: 'signed-id-token' })
    })
    const prompt = vi.fn()

    const appendChildSpy = vi.spyOn(document.head, 'appendChild').mockImplementation((node) => {
      const script = node as unknown as HTMLScriptElement
      window.google = { accounts: { id: { initialize, prompt } } }
      script.onload?.(new Event('load'))
      return node
    })

    const { useGoogleIdentity } = await importUseGoogleIdentity()
    const { requestIdToken } = useGoogleIdentity()

    const token = await requestIdToken()

    expect(token).toBe('signed-id-token')
    expect(initialize).toHaveBeenCalledWith(expect.objectContaining({
      client_id: 'test-client-id.apps.googleusercontent.com'
    }))
    appendChildSpy.mockRestore()
  })

  it('使用者略過或取消時拋出例外', async () => {
    const initialize = vi.fn()
    const prompt = vi.fn((listener: (n: unknown) => void) => {
      listener({ isNotDisplayed: () => true, isSkippedMoment: () => false })
    })

    const appendChildSpy = vi.spyOn(document.head, 'appendChild').mockImplementation((node) => {
      const script = node as unknown as HTMLScriptElement
      window.google = { accounts: { id: { initialize, prompt } } }
      script.onload?.(new Event('load'))
      return node
    })

    const { useGoogleIdentity } = await importUseGoogleIdentity()
    const { requestIdToken } = useGoogleIdentity()

    await expect(requestIdToken()).rejects.toThrow('Google login was cancelled or failed')
    appendChildSpy.mockRestore()
  })
})
