import { defineStore } from 'pinia'
import type { GameRoom, GamePhase, Player } from '~/types/gameRoom'
import type { PauseStatePayload, QuestionPayload, RankEntry, AnswerResultPayload, QuestionEndPayload, RoomStateSyncPayload, AnswerProgressPayload, BackendGamePhase } from '~/types/other'

export const useGameStore = defineStore('game', () => {
  // ── 房間狀態 ──
  const room = ref<GameRoom | null>(null)
  const phase = ref<GamePhase>('idle')

  // ── 當前題目 ──
  const currentQuestion = ref<QuestionPayload | null>(null)
  const selectedAnswerIndex = ref<number | null>(null)
  const isCorrect = ref<boolean | null>(null)
  const correctAnswerIndex = ref<number | null>(null)
  const currentExplanation = ref('')
  const pointsEarned = ref(0)

  // ── 計時器 ──
  const timeLeft = ref(0)
  const timeLimitTotal = ref(20)
  /** Host 暫停中：所有倒數停止，直到收到伺服器的繼續事件（含剩餘毫秒數）才恢復 */
  const isPaused = ref(false)
  let _timerInterval: ReturnType<typeof setInterval> | null = null

  /** 公布答案畫面停留秒數，時間到由 Host 端自動呼叫 SkipQuestion 推進下一題 */
  const REVEAL_DURATION_SECONDS = 5

  // ── 排名 ──
  const rankings = ref<RankEntry[]>([])

  // ── 我自己 ──
  const myNickname = ref('')
  const myScore = ref(0)
  const myRank = ref(0)

  // ── Host 答題進度 ──
  const answeredCount = ref(0)
  const totalCount = ref(0)

  // ── Computed ──
  const players = computed<Player[]>(() => room.value?.players ?? [])
  const roomCode = computed(() => room.value?.roomCode ?? '')
  const playerCount = computed(() => players.value.length)
  const isTimerUrgent = computed(() => timeLeft.value <= 5 && timeLeft.value > 0)
  const timerPercent = computed(() =>
    timeLimitTotal.value > 0 ? (timeLeft.value / timeLimitTotal.value) * 100 : 0
  )
  const myRankEntry = computed(() =>
    rankings.value.find(r => r.nickname === myNickname.value)
  )
  /** 作答階段內，所有人提前送出答案（尚未倒數結束）即可提前結算 */
  const isAllAnsweredEarly = computed(() =>
    phase.value === 'question'
    && !isPaused.value
    && timeLeft.value > 0
    && totalCount.value > 0
    && answeredCount.value >= totalCount.value
  )
  /** 各選項目前已選人數（僅計入已作答者，公布答案前僅 Host 頁面使用） */
  const optionVoteCounts = computed(() => {
    const optionCount = currentQuestion.value?.options.length ?? 0
    const counts = new Array(optionCount).fill(0)
    for (const p of players.value) {
      if (p.selectedOptionIndex !== undefined) counts[p.selectedOptionIndex]++
    }
    return counts
  })

  // ── Timer ──
  const _stopTimer = () => {
    if (_timerInterval !== null) {
      clearInterval(_timerInterval)
      _timerInterval = null
    }
  }

  const _startTimer = (seconds: number, total = seconds) => {
    _stopTimer()
    timeLeft.value = seconds
    timeLimitTotal.value = total
    _timerInterval = setInterval(() => {
      if (timeLeft.value > 0) {
        timeLeft.value--
      } else {
        _stopTimer()
      }
    }, 1000)
  }

  /** 伺服器回報的剩餘毫秒轉成倒數秒數（無條件進位，避免暫停/繼續後提前歸零） */
  const _msToSeconds = (ms: number) => Math.max(0, Math.ceil(ms / 1000))

  /** 依暫停狀態套用倒數：暫停時凍結在伺服器剩餘秒數，繼續時從該秒數重新倒數 */
  const _applyRemaining = (remainingMs: number, total: number, paused: boolean) => {
    const seconds = _msToSeconds(remainingMs)
    if (paused) {
      _stopTimer()
      timeLeft.value = seconds
      timeLimitTotal.value = total
    } else {
      _startTimer(seconds, total)
    }
  }

  // ── Actions ──

  const _resetPlayersAnswered = (hasAnswered: boolean) => {
    if (!room.value) return
    room.value.players = room.value.players.map(p => ({ ...p, hasAnswered, selectedOptionIndex: undefined }))
  }

  const setRoom = (r: GameRoom) => {
    room.value = r
    phase.value = 'waiting'
  }

  /** 後端 GamePhase 字串（lobby/question/reveal/ended）對應前端 GamePhase（waiting/question/result/ended） */
  const BACKEND_PHASE_MAP: Record<BackendGamePhase, GamePhase> = {
    lobby: 'waiting',
    question: 'question',
    reveal: 'result',
    ended: 'ended'
  }

  /** 重新連線後套用伺服器同步回來的完整房間快照（重新整理 / 直接開啟房間頁時使用） */
  const setRoomState = (payload: RoomStateSyncPayload) => {
    const mappedPhase = BACKEND_PHASE_MAP[payload.phase]

    room.value = payload.room
    phase.value = mappedPhase
    isPaused.value = false

    if (mappedPhase === 'question' && payload.currentQuestion) {
      currentQuestion.value = payload.currentQuestion
      selectedAnswerIndex.value = null
      isCorrect.value = null
      correctAnswerIndex.value = null
      currentExplanation.value = ''
      pointsEarned.value = 0
      answeredCount.value = payload.answeredCount ?? 0
      totalCount.value = payload.totalCount ?? players.value.length
      isPaused.value = payload.isPaused ?? false
      const limit = payload.currentQuestion.timeLimit
      if (payload.remainingMs !== undefined && payload.remainingMs !== null) {
        _applyRemaining(payload.remainingMs, limit, isPaused.value)
      } else {
        _startTimer(limit)
      }
      if (payload.self) _applySelf(payload.self)
    } else if (mappedPhase === 'result' && payload.questionEnd) {
      if (payload.self) _applySelf(payload.self)
      correctAnswerIndex.value = payload.questionEnd.answerIndex
      currentExplanation.value = payload.questionEnd.explanation
      rankings.value = payload.questionEnd.rankings
      _startTimer(REVEAL_DURATION_SECONDS)
    } else if (mappedPhase === 'ended' && payload.rankings) {
      if (payload.self) _applySelf(payload.self)
      rankings.value = payload.rankings
      _stopTimer()
    }
  }

  /** 重連玩家本人的分數/名次/作答狀態（RejoinAsPlayer 的 RoomStateSync 才會帶） */
  const _applySelf = (self: NonNullable<RoomStateSyncPayload['self']>) => {
    myNickname.value = self.nickname
    myScore.value = self.score
    myRank.value = self.rank
    if (self.hasAnswered && self.selectedOptionIndex !== undefined && self.selectedOptionIndex !== null) {
      selectedAnswerIndex.value = self.selectedOptionIndex
    }
    if (self.lastAnswerResult) {
      isCorrect.value = self.lastAnswerResult.isCorrect
      correctAnswerIndex.value = self.lastAnswerResult.correctAnswerIndex
      pointsEarned.value = self.lastAnswerResult.pointsEarned
    }
  }

  const addPlayer = (player: Player) => {
    if (!room.value) return
    const exists = room.value.players.some(p => p.id === player.id)
    if (!exists) room.value.players.push(player)
  }

  /** 遊戲開始後玩家斷線：保留在名單上，標示為離線（Host 畫面灰顯） */
  const markPlayerDisconnected = (playerId: string) => {
    const player = room.value?.players.find(p => p.id === playerId)
    if (player) player.isConnected = false
  }

  /** 離線玩家重連：標示回在線；若名單上沒有（例如 Host 剛重新整理前漏事件）則補上 */
  const markPlayerReconnected = (reconnected: Player) => {
    if (!room.value) return
    const player = room.value.players.find(p => p.id === reconnected.id)
    if (player) {
      player.isConnected = true
    } else {
      room.value.players.push({ ...reconnected, isConnected: true })
    }
  }

  const setPauseState = (payload: PauseStatePayload) => {
    isPaused.value = payload.isPaused
    if (phase.value !== 'question') return
    _applyRemaining(payload.remainingMs, timeLimitTotal.value, payload.isPaused)
  }

  /** 送出答案被伺服器拒絕（例如剛好暫停）時，還原本地選取讓玩家可以重選 */
  const clearSelectedAnswer = () => {
    selectedAnswerIndex.value = null
  }

  const removePlayer = (playerId: string) => {
    if (!room.value) return
    room.value.players = room.value.players.filter(p => p.id !== playerId)
  }

  const setQuestion = (q: QuestionPayload) => {
    currentQuestion.value = q
    selectedAnswerIndex.value = null
    isCorrect.value = null
    correctAnswerIndex.value = null
    currentExplanation.value = ''
    pointsEarned.value = 0
    answeredCount.value = 0
    totalCount.value = players.value.length
    isPaused.value = false
    phase.value = 'question'
    _resetPlayersAnswered(false)
    _startTimer(q.timeLimit)
  }

  const selectAnswer = (index: number) => {
    if (selectedAnswerIndex.value !== null) return // 已作答
    selectedAnswerIndex.value = index
  }

  const setAnswerResult = (payload: AnswerResultPayload) => {
    isCorrect.value = payload.isCorrect
    correctAnswerIndex.value = payload.correctAnswerIndex
    pointsEarned.value = payload.pointsEarned
    _stopTimer()
  }

  const setAnswerProgress = (payload: AnswerProgressPayload) => {
    answeredCount.value = payload.answered
    totalCount.value = payload.total
    if (!room.value) return
    const player = room.value.players.find(p => p.id === payload.playerId)
    if (player) {
      player.hasAnswered = true
      player.selectedOptionIndex = payload.selectedOptionIndex
    }
  }

  const setQuestionEnd = (payload: QuestionEndPayload) => {
    correctAnswerIndex.value = payload.answerIndex
    currentExplanation.value = payload.explanation
    rankings.value = payload.rankings
    phase.value = 'result'
    isPaused.value = false
    _startTimer(REVEAL_DURATION_SECONDS)

    const me = payload.rankings.find(r => r.nickname === myNickname.value)
    if (me) {
      myScore.value = me.score
      myRank.value = me.rank
    }
  }

  const endGame = (finalRankings: RankEntry[]) => {
    rankings.value = finalRankings
    phase.value = 'ended'
    isPaused.value = false
    _stopTimer()

    const me = finalRankings.find(r => r.nickname === myNickname.value)
    if (me) {
      myScore.value = me.score
      myRank.value = me.rank
    }
  }

  const setMyNickname = (nickname: string) => {
    myNickname.value = nickname
  }

  const reset = () => {
    room.value = null
    phase.value = 'idle'
    isPaused.value = false
    currentQuestion.value = null
    selectedAnswerIndex.value = null
    isCorrect.value = null
    correctAnswerIndex.value = null
    currentExplanation.value = ''
    pointsEarned.value = 0
    rankings.value = []
    myNickname.value = ''
    myScore.value = 0
    myRank.value = 0
    answeredCount.value = 0
    totalCount.value = 0
    _stopTimer()
  }

  return {
    // state
    room, phase, currentQuestion, selectedAnswerIndex,
    isCorrect, correctAnswerIndex, currentExplanation, pointsEarned,
    timeLeft, timeLimitTotal, isPaused, rankings,
    myNickname, myScore, myRank,
    answeredCount, totalCount,
    // computed
    players, roomCode, playerCount, isTimerUrgent, timerPercent, myRankEntry, optionVoteCounts,
    isAllAnsweredEarly,
    // actions
    setRoom, setRoomState, addPlayer, removePlayer, markPlayerDisconnected, markPlayerReconnected,
    setPauseState, clearSelectedAnswer,
    setQuestion, selectAnswer,
    setAnswerResult, setAnswerProgress,
    setQuestionEnd, endGame,
    setMyNickname, reset
  }
})
