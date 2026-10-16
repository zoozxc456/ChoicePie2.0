/** 玩家重連所需的本機資料，依房間代碼分開存放；reconnectToken 為後端 JoinRoom 回傳的秘密憑證 */
export interface PlayerSession {
  roomCode: string
  nickname: string
  reconnectToken: string
}

const STORAGE_PREFIX = 'choicepie:player-session:'

const storageKey = (roomCode: string) => `${STORAGE_PREFIX}${roomCode.toUpperCase()}`

/**
 * 以 localStorage 保存玩家 session，讓重新整理 / 手機鎖屏斷線後可用憑證重連同一位玩家。
 * 所有存取都包 try/catch：隱私模式或被封鎖時 localStorage 可能丟例外，此時視為沒有 session。
 */
export const usePlayerSession = () => {
  const save = (session: PlayerSession) => {
    try {
      localStorage.setItem(storageKey(session.roomCode), JSON.stringify(session))
    } catch {
      // 儲存失敗只代表重新整理後無法自動重連，不影響目前遊戲
    }
  }

  const load = (roomCode: string): PlayerSession | null => {
    try {
      const raw = localStorage.getItem(storageKey(roomCode))
      if (!raw) return null
      const parsed = JSON.parse(raw) as Partial<PlayerSession>
      if (
        typeof parsed.roomCode === 'string'
        && typeof parsed.nickname === 'string'
        && typeof parsed.reconnectToken === 'string'
        && parsed.reconnectToken
      ) {
        return { roomCode: parsed.roomCode, nickname: parsed.nickname, reconnectToken: parsed.reconnectToken }
      }
      return null
    } catch {
      return null
    }
  }

  const clear = (roomCode: string) => {
    try {
      localStorage.removeItem(storageKey(roomCode))
    } catch {
      // 忽略
    }
  }

  return { save, load, clear }
}
