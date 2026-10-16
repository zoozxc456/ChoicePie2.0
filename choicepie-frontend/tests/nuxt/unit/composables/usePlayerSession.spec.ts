import { describe, it, expect, beforeEach, vi } from 'vitest'
import { usePlayerSession } from '~/composables/usePlayerSession'

describe('usePlayerSession', () => {
  beforeEach(() => {
    localStorage.clear()
    vi.unstubAllGlobals()
  })

  it('save 之後可用房間代碼 load（不分大小寫）', () => {
    const session = usePlayerSession()

    session.save({ roomCode: 'ABCD12', nickname: 'Alice', reconnectToken: 'tok' })

    expect(session.load('abcd12')).toEqual({ roomCode: 'ABCD12', nickname: 'Alice', reconnectToken: 'tok' })
  })

  it('依房間代碼分開存放', () => {
    const session = usePlayerSession()
    session.save({ roomCode: 'AAAAAA', nickname: 'A', reconnectToken: 't1' })
    session.save({ roomCode: 'BBBBBB', nickname: 'B', reconnectToken: 't2' })

    session.clear('AAAAAA')

    expect(session.load('AAAAAA')).toBeNull()
    expect(session.load('BBBBBB')?.reconnectToken).toBe('t2')
  })

  it('內容損毀或缺欄位時回傳 null', () => {
    const session = usePlayerSession()
    localStorage.setItem('choicepie:player-session:AAAAAA', '{bad json')
    localStorage.setItem('choicepie:player-session:BBBBBB', JSON.stringify({ roomCode: 'BBBBBB', nickname: 'x' }))

    expect(session.load('AAAAAA')).toBeNull()
    expect(session.load('BBBBBB')).toBeNull()
  })

  it('localStorage 丟例外時不會拋出', () => {
    const denied = () => {
      throw new Error('denied')
    }
    vi.stubGlobal('localStorage', { getItem: denied, setItem: denied, removeItem: denied })
    const session = usePlayerSession()

    expect(() => session.save({ roomCode: 'AAAAAA', nickname: 'A', reconnectToken: 't' })).not.toThrow()
    expect(session.load('AAAAAA')).toBeNull()
    expect(() => session.clear('AAAAAA')).not.toThrow()
  })
})
