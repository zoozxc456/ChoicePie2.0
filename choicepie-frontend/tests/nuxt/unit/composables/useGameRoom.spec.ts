import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest'
import { usePlayerSession } from '~/composables/usePlayerSession'
import {
  hubConnectionMock,
  resetHubConnectionMock,
  builderMock,
  resetBuilderMock,
  useGameStoreMock,
  useAuthStoreMock,
  navigateToMock,
  HttpError
} from './mocks/useGameRoom.mock'

const importUseGameRoom = async () => {
  vi.resetModules()
  const mod = await import('~/composables/useGameRoom')
  return mod
}

const makeGameStore = () => ({
  addPlayer: vi.fn(),
  removePlayer: vi.fn(),
  setAnswerProgress: vi.fn(),
  setQuestion: vi.fn(),
  setAnswerResult: vi.fn(),
  setQuestionEnd: vi.fn(),
  endGame: vi.fn(),
  setRoomState: vi.fn(),
  setMyNickname: vi.fn(),
  markPlayerDisconnected: vi.fn(),
  markPlayerReconnected: vi.fn(),
  setPauseState: vi.fn(),
  setHostConnection: vi.fn(),
  roomCode: 'ABCD',
  reset: vi.fn()
})

describe('useGameRoom', () => {
  beforeEach(() => {
    resetHubConnectionMock()
    resetBuilderMock()
    vi.clearAllMocks()
    resetHubConnectionMock()
    resetBuilderMock()
    useGameStoreMock.mockReturnValue(makeGameStore())
    useAuthStoreMock.mockReturnValue({ isLoggedIn: true, fetchMe: vi.fn().mockResolvedValue(true), logout: vi.fn() })
    localStorage.clear()
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  describe('connect', () => {
    it('建立連線並呼叫 start', async () => {
      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()

      await gameRoom.connect()

      expect(builderMock.withUrl).toHaveBeenCalledWith(
        'https://api.example.test/api/gamehub',
        expect.objectContaining({ withCredentials: true })
      )
      expect(hubConnectionMock.start).toHaveBeenCalledTimes(1)
    })

    it('跳過 negotiate 直接用 WebSocket 連線，後端多台時不需要 sticky session', async () => {
      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()

      await gameRoom.connect()

      expect(builderMock.withUrl).toHaveBeenCalledWith(
        'https://api.example.test/api/gamehub',
        expect.objectContaining({ skipNegotiation: true, transport: 1 })
      )
    })

    it('已連線時不重複建立', async () => {
      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()
      await gameRoom.connect()
      hubConnectionMock.state = 'Connected'

      await gameRoom.connect()

      expect(hubConnectionMock.start).toHaveBeenCalledTimes(1)
    })

    it('401 時嘗試 fetchMe 成功後重新連線一次', async () => {
      const fetchMe = vi.fn().mockResolvedValue(true)
      useAuthStoreMock.mockReturnValue({ isLoggedIn: true, fetchMe, logout: vi.fn() })
      hubConnectionMock.start
        .mockRejectedValueOnce(new HttpError('unauthorized', 401))
        .mockResolvedValueOnce(undefined)

      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()
      await gameRoom.connect()

      expect(fetchMe).toHaveBeenCalledTimes(1)
      expect(hubConnectionMock.start).toHaveBeenCalledTimes(2)
    })

    it('401 且 fetchMe 失敗時呼叫 logout', async () => {
      const fetchMe = vi.fn().mockResolvedValue(false)
      const logout = vi.fn().mockResolvedValue(undefined)
      useAuthStoreMock.mockReturnValue({ isLoggedIn: true, fetchMe, logout })
      hubConnectionMock.start.mockRejectedValue(new HttpError('unauthorized', 401))

      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()
      await gameRoom.connect()

      expect(logout).toHaveBeenCalledWith('/')
    })

    it('非 401 連線失敗會排程重連並往外拋出', async () => {
      vi.useFakeTimers()
      hubConnectionMock.start.mockRejectedValue(new Error('negotiate failed'))

      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()

      await expect(gameRoom.connect()).rejects.toThrow('negotiate failed')

      hubConnectionMock.start.mockResolvedValue(undefined)
      await vi.advanceTimersByTimeAsync(2000)

      expect(hubConnectionMock.start).toHaveBeenCalledTimes(2)
    })
  })

  describe('SignalR 事件路由到 gameStore', () => {
    it('QuestionStart 呼叫 setQuestion', async () => {
      const gameStore = makeGameStore()
      useGameStoreMock.mockReturnValue(gameStore)
      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()
      await gameRoom.connect()

      const payload = { index: 0, total: 3, text: 'Q1', options: ['a', 'b'], timeLimit: 20 }
      hubConnectionMock.handlers.get('QuestionStart')?.(payload)

      expect(gameStore.setQuestion).toHaveBeenCalledWith(payload)
    })

    it('PlayerJoined 呼叫 addPlayer', async () => {
      const gameStore = makeGameStore()
      useGameStoreMock.mockReturnValue(gameStore)
      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()
      await gameRoom.connect()

      const player = { id: 'p1', nickname: 'Alice', score: 0, rank: 0, hasAnswered: false }
      hubConnectionMock.handlers.get('PlayerJoined')?.(player)

      expect(gameStore.addPlayer).toHaveBeenCalledWith(player)
    })

    it('GameEnd 呼叫 endGame', async () => {
      const gameStore = makeGameStore()
      useGameStoreMock.mockReturnValue(gameStore)
      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()
      await gameRoom.connect()

      const rankings = [{ rank: 1, nickname: 'Alice', score: 300 }]
      hubConnectionMock.handlers.get('GameEnd')?.(rankings)

      expect(gameStore.endGame).toHaveBeenCalledWith(rankings)
    })

    it('RoomStateSync 呼叫 setRoomState', async () => {
      const gameStore = makeGameStore()
      useGameStoreMock.mockReturnValue(gameStore)
      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()
      await gameRoom.connect()

      const payload = { phase: 'lobby' as const, room: { roomCode: 'ABCD' } }
      hubConnectionMock.handlers.get('RoomStateSync')?.(payload)

      expect(gameStore.setRoomState).toHaveBeenCalledWith(payload)
    })

    it('PauseStateChanged 呼叫 setPauseState', async () => {
      const gameStore = makeGameStore()
      useGameStoreMock.mockReturnValue(gameStore)
      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()
      await gameRoom.connect()

      hubConnectionMock.handlers.get('PauseStateChanged')?.({ isPaused: true, remainingMs: 12000 })

      expect(gameStore.setPauseState).toHaveBeenCalledWith({ isPaused: true, remainingMs: 12000 })
    })

    it('PlayerDisconnected / PlayerReconnected 更新玩家連線狀態', async () => {
      const gameStore = makeGameStore()
      useGameStoreMock.mockReturnValue(gameStore)
      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()
      await gameRoom.connect()

      hubConnectionMock.handlers.get('PlayerDisconnected')?.('p1')
      const player = { id: 'p1', nickname: 'Alice' }
      hubConnectionMock.handlers.get('PlayerReconnected')?.(player)

      expect(gameStore.markPlayerDisconnected).toHaveBeenCalledWith('p1')
      expect(gameStore.markPlayerReconnected).toHaveBeenCalledWith(player)
    })

    it('GameEnd 後清除本機玩家 session', async () => {
      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()
      hubConnectionMock.invoke.mockResolvedValueOnce({ playerId: 'p1', reconnectToken: 'tok-1' })
      await gameRoom.joinRoom('ABCD', 'Alice')

      hubConnectionMock.handlers.get('GameEnd')?.([])

      expect(usePlayerSession().load('ABCD')).toBeNull()
    })

    it('RoomCreated 導向 host room 頁面', async () => {
      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()
      await gameRoom.connect()

      hubConnectionMock.handlers.get('RoomCreated')?.('ABCD')

      expect(navigateToMock).toHaveBeenCalledWith('/host/room/ABCD')
    })
  })

  describe('Host / Player actions', () => {
    it('createRoom 先連線再 invoke CreateRoom', async () => {
      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()

      await gameRoom.createRoom({ quizId: 'quiz-1' })

      expect(hubConnectionMock.start).toHaveBeenCalledTimes(1)
      expect(hubConnectionMock.invoke).toHaveBeenCalledWith('CreateRoom', { timeLimit: 20, quizId: 'quiz-1' })
    })

    it('joinRoom 設定暱稱並 invoke JoinRoom', async () => {
      const gameStore = makeGameStore()
      useGameStoreMock.mockReturnValue(gameStore)
      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()
      hubConnectionMock.invoke.mockResolvedValueOnce({ playerId: 'p1', reconnectToken: 'tok-1' })

      await gameRoom.joinRoom('ABCD', 'Alice')

      expect(gameStore.setMyNickname).toHaveBeenCalledWith('Alice')
      expect(hubConnectionMock.invoke).toHaveBeenCalledWith('JoinRoom', 'ABCD', 'Alice')
    })

    it('joinRoom 成功後把重連憑證存進 localStorage', async () => {
      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()
      hubConnectionMock.invoke.mockResolvedValueOnce({ playerId: 'p1', reconnectToken: 'tok-1' })

      await gameRoom.joinRoom('ABCD', 'Alice')

      expect(usePlayerSession().load('ABCD')).toEqual({ roomCode: 'ABCD', nickname: 'Alice', reconnectToken: 'tok-1' })
    })

    it('rejoinAsPlayer invoke RejoinAsPlayer 並帶上憑證', async () => {
      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()

      await gameRoom.rejoinAsPlayer('ABCD', 'tok-1')

      expect(hubConnectionMock.invoke).toHaveBeenCalledWith('RejoinAsPlayer', 'ABCD', 'tok-1')
    })

    it('SignalR 自動重連後（connectionId 改變）重新呼叫 RejoinAsPlayer', async () => {
      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()
      hubConnectionMock.invoke.mockResolvedValueOnce({ playerId: 'p1', reconnectToken: 'tok-1' })
      await gameRoom.joinRoom('ABCD', 'Alice')
      hubConnectionMock.invoke.mockClear()

      const onReconnected = hubConnectionMock.onreconnected.mock.calls[0]![0] as () => Promise<void>
      await onReconnected()

      expect(hubConnectionMock.invoke).toHaveBeenCalledWith('RejoinAsPlayer', 'ABCD', 'tok-1')
    })

    it('自動重連後重連失敗時清除本機憑證', async () => {
      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()
      hubConnectionMock.invoke.mockResolvedValueOnce({ playerId: 'p1', reconnectToken: 'tok-1' })
      await gameRoom.joinRoom('ABCD', 'Alice')
      hubConnectionMock.invoke.mockRejectedValueOnce(new Error('[GAME_ROOM_INVALID_RECONNECT_TOKEN] x'))

      const onReconnected = hubConnectionMock.onreconnected.mock.calls[0]![0] as () => Promise<void>
      await onReconnected()

      expect(usePlayerSession().load('ABCD')).toBeNull()
    })

    it('非玩家連線（Host）自動重連後不呼叫 RejoinAsPlayer', async () => {
      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()
      await gameRoom.connect()
      hubConnectionMock.invoke.mockClear()

      const onReconnected = hubConnectionMock.onreconnected.mock.calls[0]![0] as () => Promise<void>
      await onReconnected()

      expect(hubConnectionMock.invoke).not.toHaveBeenCalled()
    })

    it('extractHubErrorCode 取出後端錯誤代碼', async () => {
      const { extractHubErrorCode } = await importUseGameRoom()

      expect(extractHubErrorCode(new Error('An error: [GAME_ROOM_DUPLICATE_NICKNAME] 此暱稱已有人使用'))).toBe('GAME_ROOM_DUPLICATE_NICKNAME')
      expect(extractHubErrorCode(new Error('boom'))).toBeNull()
    })

    it('submitAnswer invoke SubmitAnswer', async () => {
      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()
      await gameRoom.connect()

      await gameRoom.submitAnswer('ABCD', 2)

      expect(hubConnectionMock.invoke).toHaveBeenCalledWith('SubmitAnswer', 'ABCD', 2)
    })

    it('skipQuestion invoke SkipQuestion', async () => {
      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()
      await gameRoom.connect()

      await gameRoom.skipQuestion('ABCD')

      expect(hubConnectionMock.invoke).toHaveBeenCalledWith('SkipQuestion', 'ABCD')
    })
  })

  describe('disconnect', () => {
    it('停止連線並重置 gameStore', async () => {
      const gameStore = makeGameStore()
      useGameStoreMock.mockReturnValue(gameStore)
      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()
      await gameRoom.connect()

      await gameRoom.disconnect()

      expect(hubConnectionMock.stop).toHaveBeenCalledTimes(1)
      expect(gameStore.reset).toHaveBeenCalledTimes(1)
    })

    it('disconnect 後不會自動重連', async () => {
      vi.useFakeTimers()
      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()
      await gameRoom.connect()

      await gameRoom.disconnect()
      hubConnectionMock.onclose.mock.calls[0]?.[0]?.()
      await vi.advanceTimersByTimeAsync(35000)

      expect(hubConnectionMock.start).toHaveBeenCalledTimes(1)
    })
  })

  describe('isConnected', () => {
    it('連線狀態為 Connected 時為 true', async () => {
      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()
      await gameRoom.connect()
      hubConnectionMock.state = 'Connected'

      expect(gameRoom.isConnected.value).toBe(true)
    })

    it('尚未連線時為 false', async () => {
      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()

      expect(gameRoom.isConnected.value).toBe(false)
    })
  })

  describe('Host 重連與連線狀態', () => {
    it('HostConnectionChanged 呼叫 setHostConnection', async () => {
      const gameStore = makeGameStore()
      useGameStoreMock.mockReturnValue(gameStore)
      const { useGameRoom } = await importUseGameRoom()
      await useGameRoom().connect()

      hubConnectionMock.handlers.get('HostConnectionChanged')?.({ connected: false })

      expect(gameStore.setHostConnection).toHaveBeenCalledWith({ connected: false })
    })

    it('Host 自動重連後重新呼叫 RejoinRoom', async () => {
      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()
      await gameRoom.rejoinAsHost('ABCD')
      hubConnectionMock.invoke.mockClear()

      const onReconnected = hubConnectionMock.onreconnected.mock.calls[0]![0] as () => Promise<void>
      await onReconnected()

      expect(hubConnectionMock.invoke).toHaveBeenCalledWith('RejoinRoom', 'ABCD')
    })

    it('建立房間後（RoomCreated）自動重連也會 RejoinRoom', async () => {
      const { useGameRoom } = await importUseGameRoom()
      await useGameRoom().connect()
      hubConnectionMock.handlers.get('RoomCreated')?.('WXYZ')
      hubConnectionMock.invoke.mockClear()

      const onReconnected = hubConnectionMock.onreconnected.mock.calls[0]![0] as () => Promise<void>
      await onReconnected()

      expect(hubConnectionMock.invoke).toHaveBeenCalledWith('RejoinRoom', 'WXYZ')
    })

    it('disconnect 後不再 RejoinRoom', async () => {
      const { useGameRoom } = await importUseGameRoom()
      const gameRoom = useGameRoom()
      await gameRoom.rejoinAsHost('ABCD')
      const onReconnected = hubConnectionMock.onreconnected.mock.calls[0]![0] as () => Promise<void>
      await gameRoom.disconnect()
      hubConnectionMock.invoke.mockClear()

      await onReconnected()

      expect(hubConnectionMock.invoke).not.toHaveBeenCalled()
    })
  })

  describe('大螢幕 WatchRoom', () => {
    it('watchRoom 連線後 invoke WatchRoom', async () => {
      const { useGameRoom } = await importUseGameRoom()

      await useGameRoom().watchRoom('ABCD')

      expect(hubConnectionMock.start).toHaveBeenCalledTimes(1)
      expect(hubConnectionMock.invoke).toHaveBeenCalledWith('WatchRoom', 'ABCD')
    })

    it('自動重連後重新 WatchRoom，且不會呼叫玩家/Host 的重連方法', async () => {
      const { useGameRoom } = await importUseGameRoom()
      await useGameRoom().watchRoom('ABCD')
      hubConnectionMock.invoke.mockClear()

      const onReconnected = hubConnectionMock.onreconnected.mock.calls[0]![0] as () => Promise<void>
      await onReconnected()

      expect(hubConnectionMock.invoke).toHaveBeenCalledTimes(1)
      expect(hubConnectionMock.invoke).toHaveBeenCalledWith('WatchRoom', 'ABCD')
    })

    it('WatchRoom 失敗會往外拋出，且重連時不再重試', async () => {
      const { useGameRoom } = await importUseGameRoom()
      hubConnectionMock.invoke.mockRejectedValueOnce(new Error('[GAME_ROOM_NOT_FOUND] x'))

      await expect(useGameRoom().watchRoom('ZZZZ')).rejects.toThrow()
      hubConnectionMock.invoke.mockClear()
      const onReconnected = hubConnectionMock.onreconnected.mock.calls[0]![0] as () => Promise<void>
      await onReconnected()

      expect(hubConnectionMock.invoke).not.toHaveBeenCalled()
    })

    it('RoomStateSync 會套用到 store（旁觀者快照）', async () => {
      const gameStore = makeGameStore()
      useGameStoreMock.mockReturnValue(gameStore)
      const { useGameRoom } = await importUseGameRoom()
      await useGameRoom().watchRoom('ABCD')

      const payload = { phase: 'lobby', room: { roomCode: 'ABCD', players: [] }, isHostOnline: true }
      hubConnectionMock.handlers.get('RoomStateSync')?.(payload)

      expect(gameStore.setRoomState).toHaveBeenCalledWith(payload)
    })
  })
})
