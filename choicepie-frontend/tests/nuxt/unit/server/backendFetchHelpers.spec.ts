import { describe, it, expect } from 'vitest'
import { redactBody, redactHeaders, resolveClientIp } from '../../../../server/utils/backendFetchHelpers'

describe('resolveClientIp', () => {
  it('預設只用 socket IP，忽略使用者自帶的 X-Forwarded-For', () => {
    expect(resolveClientIp({ socketIp: '203.0.113.5', forwardedFor: '1.2.3.4', trustProxyHeaders: false }))
      .toBe('203.0.113.5')
  })

  it('信任 proxy 時取 X-Forwarded-For 最右邊（由可信任 LB 附加）的位址', () => {
    expect(resolveClientIp({ socketIp: '10.0.0.2', forwardedFor: '6.6.6.6, 198.51.100.7', trustProxyHeaders: true }))
      .toBe('198.51.100.7')
  })

  it('信任 proxy 但沒有 X-Forwarded-For 時退回 socket IP', () => {
    expect(resolveClientIp({ socketIp: '10.0.0.2', trustProxyHeaders: true })).toBe('10.0.0.2')
  })

  it('兩者都沒有時回傳 undefined', () => {
    expect(resolveClientIp({ trustProxyHeaders: false })).toBeUndefined()
  })
})

describe('redactHeaders', () => {
  it('遮蔽 authorization / x-refresh-token / cookie，保留其他 header', () => {
    const result = redactHeaders({
      'authorization': 'Bearer abc',
      'X-Refresh-Token': 'refresh',
      'Cookie': 'a=b',
      'content-type': 'application/json'
    })

    expect(result).toEqual({
      'authorization': '[REDACTED]',
      'X-Refresh-Token': '[REDACTED]',
      'Cookie': '[REDACTED]',
      'content-type': 'application/json'
    })
  })
})

describe('redactBody', () => {
  it('遞迴遮蔽 password 與 token 類欄位', () => {
    const result = redactBody({
      email: 'a@b.c',
      password: 'pw',
      data: { accessToken: 'x', refreshToken: 'y', name: 'n' },
      list: [{ newPassword: 'z' }]
    })

    expect(result).toEqual({
      email: 'a@b.c',
      password: '[REDACTED]',
      data: { accessToken: '[REDACTED]', refreshToken: '[REDACTED]', name: 'n' },
      list: [{ newPassword: '[REDACTED]' }]
    })
  })

  it('FormData 不展開內容', () => {
    expect(redactBody(new FormData())).toBe('[FormData]')
  })
})
