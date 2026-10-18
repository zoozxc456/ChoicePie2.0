import { describe, it, expect } from 'vitest'
import { applySetCookiesToCookieHeader } from '~/utils/cookieHeader'

describe('applySetCookiesToCookieHeader', () => {
  it('覆寫同名 cookie，保留其他 cookie 與原本順序', () => {
    const result = applySetCookiesToCookieHeader(
      'cp_access_token=old; theme=dark; cp_refresh_token=old-refresh',
      ['cp_access_token=new; Path=/; HttpOnly; Secure; SameSite=Lax; Max-Age=2592000']
    )

    expect(result).toBe('cp_access_token=new; theme=dark; cp_refresh_token=old-refresh')
  })

  it('新增原本沒有的 cookie', () => {
    expect(applySetCookiesToCookieHeader('theme=dark', ['cp_access_token=new; Path=/']))
      .toBe('theme=dark; cp_access_token=new')
  })

  it('原本沒有 cookie header 時直接組出新的', () => {
    expect(applySetCookiesToCookieHeader(undefined, ['a=1; Path=/', 'b=2; Path=/'])).toBe('a=1; b=2')
  })

  it('Max-Age=0 視為刪除', () => {
    expect(applySetCookiesToCookieHeader('cp_access_token=old; theme=dark', ['cp_access_token=; Max-Age=0; Path=/']))
      .toBe('theme=dark')
  })

  it('Expires 已過期視為刪除', () => {
    expect(applySetCookiesToCookieHeader(
      'cp_access_token=old; theme=dark',
      ['cp_access_token=; Path=/; Expires=Thu, 01 Jan 1970 00:00:00 GMT']
    )).toBe('theme=dark')
  })

  it('值內含 = 時完整保留', () => {
    expect(applySetCookiesToCookieHeader('', ['token=abc==; Path=/'])).toBe('token=abc==')
  })
})
