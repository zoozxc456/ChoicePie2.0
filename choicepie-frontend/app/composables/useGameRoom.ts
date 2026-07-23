import * as signalR from '@microsoft/signalr'
import { useGameStore } from '~/stores/game'
import type { Player } from '~/types/gameRoom'
import type { AnswerProgressPayload, QuestionPayload, AnswerResultPayload, QuestionEndPayload, RankEntry, RoomStateSyncPayload } from '~/types/other'

let _connection: signalR.HubConnection | null = null
let _reconnectTimer: ReturnType<typeof setTimeout> | null = null
let _tokenRefreshTimer: ReturnType<typeof setInterval> | null = null
let _manuallyDisconnected = false

const RECONNECT_DELAYS_MS = [2000, 5000, 10000, 30000]
// Access token 效期很短（後端預設 60 秒），SignalR 是長連線，token 過期不會中斷 WebSocket，
// 也不會觸發任何錯誤，所以連線期間必須主動定期 refresh，否則 Host 操作最終會被 401 拒絕。
const TOKEN_REFRESH_INTERVAL_MS = 45000

const stopTokenRefreshLoop = () => {
  if (_tokenRefreshTimer) {
    clearInterval(_tokenRefreshTimer)
    _tokenRefreshTimer = null
  }
}

const startTokenRefreshLoop = () => {
  if (_tokenRefreshTimer) return
  _tokenRefreshTimer = setInterval(async () => {
    if (!useAuthStore().isLoggedIn) return
    const refreshed = await useAuthStore().fetchMe()
    if (!refreshed) stopTokenRefreshLoop()
  }, TOKEN_REFRESH_INTERVAL_MS)
}

const scheduleReconnect = (attempt = 0) => {
  if (_manuallyDisconnected) return
  if (_reconnectTimer) return

  const delay = RECONNECT_DELAYS_MS[Math.min(attempt, RECONNECT_DELAYS_MS.length - 1)]
  _reconnectTimer = setTimeout(async () => {
    _reconnectTimer = null
    if (_manuallyDisconnected) return

    try {
      await useGameRoom().connect()
    } catch {
      scheduleReconnect(attempt + 1)
    }
  }, delay)
}

export const useGameRoom = () => {
  const gameStore = useGameStore()

  // ── Connection ──

  const _build = () => {
    const config = useRuntimeConfig()

    _connection = new signalR.HubConnectionBuilder()
      .withUrl(`${config.public.apiBaseUrl}/api/gamehub`, {
        withCredentials: true
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000])
      .configureLogging(signalR.LogLevel.Warning)
      .build()

    _connection.onreconnecting(() => {
      console.warn('[SignalR] 重新連線中...')
    })

    _connection.onreconnected(() => {
      console.warn('[SignalR] 已重新連線')
    })

    _connection.onclose((err) => {
      if (err) console.error('[SignalR] 連線關閉', err)
      stopTokenRefreshLoop()
      // withAutomaticReconnect 重試 4 次後仍失敗才會走到這裡，全新重建連線再試一次
      scheduleReconnect()
    })

    // ── Host Events ──
    _connection.on('RoomCreated', (roomCode: string) => {
      // Host 建立房間後，導向等待室
      navigateTo(`/host/room/${roomCode}`)
    })

    _connection.on('PlayerJoined', (player: Player) => {
      gameStore.addPlayer(player)
    })

    _connection.on('PlayerLeft', (playerId: string) => {
      gameStore.removePlayer(playerId)
    })

    _connection.on('AnswerProgress', (payload: AnswerProgressPayload) => {
      gameStore.setAnswerProgress(payload)
    })

    // ── Player Events ──
    _connection.on('GameStarted', () => {
      // 遊戲開始，等待第一題
    })

    _connection.on('QuestionStart', (payload: QuestionPayload) => {
      gameStore.setQuestion(payload)
    })

    _connection.on('AnswerResult', (payload: AnswerResultPayload) => {
      gameStore.setAnswerResult(payload)
    })

    _connection.on('QuestionEnd', (payload: QuestionEndPayload) => {
      gameStore.setQuestionEnd(payload)
    })

    _connection.on('GameEnd', (rankings: RankEntry[]) => {
      gameStore.endGame(rankings)
    })

    // ── Reconnect Sync ──
    _connection.on('RoomStateSync', (payload: RoomStateSyncPayload) => {
      gameStore.setRoomState(payload)
    })
  }

  const connect = async (isRetry = false): Promise<void> => {
    if (_connection?.state === signalR.HubConnectionState.Connected) return
    _manuallyDisconnected = false
    _build()

    try {
      await _connection!.start()
      startTokenRefreshLoop()
    } catch (e) {
      const isUnauthorized = e instanceof signalR.HttpError && e.statusCode === 401
      if (isUnauthorized && !isRetry) {
        const refreshed = await useAuthStore().fetchMe()
        if (refreshed) return connect(true)
        await useAuthStore().logout('/')
        return
      }
      // negotiate/連線本身失敗（例如後端還沒起來），withAutomaticReconnect 不會處理這種情況，需自行排程重試
      scheduleReconnect()
      throw e
    }
  }

  const disconnect = async () => {
    _manuallyDisconnected = true
    stopTokenRefreshLoop()
    if (_reconnectTimer) {
      clearTimeout(_reconnectTimer)
      _reconnectTimer = null
    }
    if (_connection) {
      await _connection.stop()
      _connection = null
    }
    gameStore.reset()
  }

  // ── Host Actions ──

  const createRoom = async (request: { quizId: string, questionIds?: string[], timeLimit?: number }) => {
    await connect()
    await _connection!.invoke('CreateRoom', { timeLimit: 20, ...request })
  }

  const startGame = async (roomCode: string) => {
    await _connection!.invoke('StartGame', roomCode)
  }

  const skipQuestion = async (roomCode: string) => {
    await _connection!.invoke('SkipQuestion', roomCode)
  }

  const pauseGame = async (roomCode: string) => {
    await _connection!.invoke('PauseGame', roomCode)
  }

  /** 重新整理或直接開啟房間頁時，重新連線並取得目前房間狀態 */
  const rejoinAsHost = async (roomCode: string) => {
    await connect()
    await _connection!.invoke('RejoinRoom', roomCode)
  }

  // ── Player Actions ──

  const joinRoom = async (roomCode: string, nickname: string) => {
    await connect()
    gameStore.setMyNickname(nickname)
    await _connection!.invoke('JoinRoom', roomCode, nickname)
  }

  const submitAnswer = async (roomCode: string, answerIndex: number) => {
    await _connection!.invoke('SubmitAnswer', roomCode, answerIndex)
  }

  // ── State ──

  const isConnected = computed(
    () => _connection?.state === signalR.HubConnectionState.Connected
  )

  return {
    isConnected,
    connect,
    disconnect,
    createRoom,
    startGame,
    skipQuestion,
    pauseGame,
    rejoinAsHost,
    joinRoom,
    submitAnswer
  }
}
