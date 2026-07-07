import * as signalR from '@microsoft/signalr'
import { useGameStore } from '~/stores/game'
import type { Player } from '~/types/gameRoom'
import type { AnswerProgressPayload, QuestionPayload, AnswerResultPayload, QuestionEndPayload, RankEntry, RoomStateSyncPayload } from '~/types/other'

let _connection: signalR.HubConnection | null = null

export const useGameRoom = () => {
  const gameStore = useGameStore()

  // ── Connection ──

  const _build = () => {
    _connection = new signalR.HubConnectionBuilder()
      .withUrl('/api/gamehub')
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
    })

    // ── Host Events ──
    _connection.on('RoomCreated', (roomCode: string) => {
      // Host 建立房間後，導向等待室
      navigateTo(`/host/room/${roomCode}`)
    })

    _connection.on('PlayerJoined', (player: Player) => {
      gameStore.addPlayer(player)
    })

    _connection.on('PlayerLeft', (connectionId: string) => {
      gameStore.removePlayer(connectionId)
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
      navigateTo(`/room/${gameStore.roomCode}/result`)
    })

    // ── Reconnect Sync ──
    _connection.on('RoomStateSync', (payload: RoomStateSyncPayload) => {
      gameStore.setRoomState(payload)
    })
  }

  const connect = async () => {
    if (_connection?.state === signalR.HubConnectionState.Connected) return
    _build()
    await _connection!.start()
  }

  const disconnect = async () => {
    if (_connection) {
      await _connection.stop()
      _connection = null
    }
    gameStore.reset()
  }

  // ── Host Actions ──

  const createRoom = async (request: { quizId?: string, questionIds: string[], timeLimit?: number }) => {
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
