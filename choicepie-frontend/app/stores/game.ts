import { defineStore } from 'pinia'
import type { GameRoom, GamePhase, Player } from '~/types/gameRoom'
import type { QuestionPayload, RankEntry, AnswerResultPayload, QuestionEndPayload, RoomStateSyncPayload, AnswerProgressPayload, BackendGamePhase } from '~/types/other'

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

  const _startTimer = (seconds: number) => {
    _stopTimer()
    timeLeft.value = seconds
    timeLimitTotal.value = seconds
    _timerInterval = setInterval(() => {
      if (timeLeft.value > 0) {
        timeLeft.value--
      } else {
        _stopTimer()
      }
    }, 1000)
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

    if (mappedPhase === 'question' && payload.currentQuestion) {
      currentQuestion.value = payload.currentQuestion
      selectedAnswerIndex.value = null
      isCorrect.value = null
      correctAnswerIndex.value = null
      currentExplanation.value = ''
      pointsEarned.value = 0
      answeredCount.value = payload.answeredCount ?? 0
      totalCount.value = payload.totalCount ?? players.value.length
      _startTimer(payload.currentQuestion.timeLimit)
    } else if (mappedPhase === 'result' && payload.questionEnd) {
      correctAnswerIndex.value = payload.questionEnd.answerIndex
      currentExplanation.value = payload.questionEnd.explanation
      rankings.value = payload.questionEnd.rankings
      _startTimer(REVEAL_DURATION_SECONDS)
    } else if (mappedPhase === 'ended' && payload.rankings) {
      rankings.value = payload.rankings
      _stopTimer()
    }
  }

  const addPlayer = (player: Player) => {
    if (!room.value) return
    const exists = room.value.players.some(p => p.id === player.id)
    if (!exists) room.value.players.push(player)
  }

  /** 後端 PlayerLeft 事件目前傳的是 SignalR connectionId，不是 player id，故無法在此比對移除玩家（已知後端缺口） */
  const removePlayer = (_connectionId: string) => {
    if (!room.value) return
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
    timeLeft, timeLimitTotal, rankings,
    myNickname, myScore, myRank,
    answeredCount, totalCount,
    // computed
    players, roomCode, playerCount, isTimerUrgent, timerPercent, myRankEntry, optionVoteCounts,
    isAllAnsweredEarly,
    // actions
    setRoom, setRoomState, addPlayer, removePlayer,
    setQuestion, selectAnswer,
    setAnswerResult, setAnswerProgress,
    setQuestionEnd, endGame,
    setMyNickname, reset
  }
})
