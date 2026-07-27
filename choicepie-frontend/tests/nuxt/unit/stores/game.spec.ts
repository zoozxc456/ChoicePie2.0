import { describe, it, expect, beforeEach, afterEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { useGameStore } from '~/stores/game'
import type { GameRoom, Player } from '~/types/gameRoom'
import type {
  QuestionPayload,
  RoomStateSyncPayload,
  AnswerProgressPayload,
  QuestionEndPayload,
  AnswerResultPayload
} from '~/types/other'

const makePlayer = (overrides: Partial<Player> = {}): Player => ({
  id: 'p1',
  nickname: 'Alice',
  score: 0,
  rank: 0,
  hasAnswered: false,
  ...overrides
})

const makeRoom = (overrides: Partial<GameRoom> = {}): GameRoom => ({
  roomCode: 'ABCD',
  quizTitle: 'Sample Quiz',
  status: 'waiting',
  players: [makePlayer()],
  currentQuestionIndex: 0,
  totalQuestions: 3,
  hostConnectionId: 'host-1',
  ...overrides
})

const makeQuestion = (overrides: Partial<QuestionPayload> = {}): QuestionPayload => ({
  index: 0,
  total: 3,
  text: 'What is 1+1?',
  options: ['1', '2', '3', '4'],
  timeLimit: 20,
  ...overrides
})

describe('useGameStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  describe('setRoom', () => {
    it('儲存房間資料並將 phase 設為 waiting', () => {
      const store = useGameStore()
      const room = makeRoom()

      store.setRoom(room)

      expect(store.room).toEqual(room)
      expect(store.phase).toBe('waiting')
      expect(store.playerCount).toBe(1)
      expect(store.roomCode).toBe('ABCD')
    })
  })

  describe('addPlayer', () => {
    it('新增未存在的玩家', () => {
      const store = useGameStore()
      store.setRoom(makeRoom({ players: [] }))

      store.addPlayer(makePlayer({ id: 'p2', nickname: 'Bob' }))

      expect(store.players).toHaveLength(1)
      expect(store.players[0]?.nickname).toBe('Bob')
    })

    it('已存在的玩家 id 不重複加入', () => {
      const store = useGameStore()
      store.setRoom(makeRoom({ players: [makePlayer({ id: 'p1' })] }))

      store.addPlayer(makePlayer({ id: 'p1', nickname: 'Duplicate' }))

      expect(store.players).toHaveLength(1)
      expect(store.players[0]?.nickname).toBe('Alice')
    })

    it('room 尚未建立時不做任何事', () => {
      const store = useGameStore()

      store.addPlayer(makePlayer())

      expect(store.room).toBeNull()
    })
  })

  describe('setQuestion', () => {
    it('設定題目並重置作答狀態、啟動計時器', () => {
      const store = useGameStore()
      store.setRoom(makeRoom())
      store.selectAnswer(2)

      store.setQuestion(makeQuestion({ timeLimit: 15 }))

      expect(store.phase).toBe('question')
      expect(store.currentQuestion?.text).toBe('What is 1+1?')
      expect(store.selectedAnswerIndex).toBeNull()
      expect(store.correctAnswerIndex).toBeNull()
      expect(store.answeredCount).toBe(0)
      expect(store.totalCount).toBe(store.players.length)
      expect(store.timeLeft).toBe(15)
      expect(store.timeLimitTotal).toBe(15)
    })

    it('重置所有玩家的 hasAnswered / selectedOptionIndex', () => {
      const store = useGameStore()
      store.setRoom(makeRoom({
        players: [makePlayer({ id: 'p1', hasAnswered: true, selectedOptionIndex: 1 })]
      }))

      store.setQuestion(makeQuestion())

      expect(store.players[0]?.hasAnswered).toBe(false)
      expect(store.players[0]?.selectedOptionIndex).toBeUndefined()
    })

    it('計時器會隨時間遞減', () => {
      const store = useGameStore()
      store.setRoom(makeRoom())
      store.setQuestion(makeQuestion({ timeLimit: 5 }))

      vi.advanceTimersByTime(3000)

      expect(store.timeLeft).toBe(2)
    })

    it('倒數到 0 時停止遞減', () => {
      const store = useGameStore()
      store.setRoom(makeRoom())
      store.setQuestion(makeQuestion({ timeLimit: 2 }))

      vi.advanceTimersByTime(10000)

      expect(store.timeLeft).toBe(0)
    })
  })

  describe('selectAnswer', () => {
    it('第一次選擇會記錄索引', () => {
      const store = useGameStore()

      store.selectAnswer(1)

      expect(store.selectedAnswerIndex).toBe(1)
    })

    it('已作答後再次選擇不會覆蓋', () => {
      const store = useGameStore()

      store.selectAnswer(1)
      store.selectAnswer(3)

      expect(store.selectedAnswerIndex).toBe(1)
    })
  })

  describe('setAnswerProgress', () => {
    it('更新已答人數並標記玩家已作答', () => {
      const store = useGameStore()
      store.setRoom(makeRoom({ players: [makePlayer({ id: 'p1' })] }))

      const payload: AnswerProgressPayload = {
        answered: 1,
        total: 2,
        playerId: 'p1',
        selectedOptionIndex: 2
      }
      store.setAnswerProgress(payload)

      expect(store.answeredCount).toBe(1)
      expect(store.totalCount).toBe(2)
      expect(store.players[0]?.hasAnswered).toBe(true)
      expect(store.players[0]?.selectedOptionIndex).toBe(2)
    })
  })

  describe('isAllAnsweredEarly', () => {
    it('作答階段且所有人已答完、時間未到時為 true', () => {
      const store = useGameStore()
      store.setRoom(makeRoom())
      store.setQuestion(makeQuestion({ timeLimit: 10 }))

      store.setAnswerProgress({ answered: 1, total: 1, playerId: 'p1', selectedOptionIndex: 0 })

      expect(store.isAllAnsweredEarly).toBe(true)
    })

    it('尚有人未作答時為 false', () => {
      const store = useGameStore()
      store.setRoom(makeRoom({ players: [makePlayer({ id: 'p1' }), makePlayer({ id: 'p2' })] }))
      store.setQuestion(makeQuestion({ timeLimit: 10 }))

      store.setAnswerProgress({ answered: 1, total: 2, playerId: 'p1', selectedOptionIndex: 0 })

      expect(store.isAllAnsweredEarly).toBe(false)
    })

    it('非 question phase 時為 false', () => {
      const store = useGameStore()
      store.setRoom(makeRoom())

      expect(store.isAllAnsweredEarly).toBe(false)
    })
  })

  describe('optionVoteCounts', () => {
    it('統計各選項已作答玩家的票數', () => {
      const store = useGameStore()
      store.setRoom(makeRoom({
        players: [
          makePlayer({ id: 'p1', selectedOptionIndex: 0 }),
          makePlayer({ id: 'p2', selectedOptionIndex: 0 }),
          makePlayer({ id: 'p3', selectedOptionIndex: 2 })
        ]
      }))
      store.currentQuestion = makeQuestion({ options: ['a', 'b', 'c', 'd'] })

      expect(store.optionVoteCounts).toEqual([2, 0, 1, 0])
    })

    it('未作答的玩家不列入計算', () => {
      const store = useGameStore()
      store.setRoom(makeRoom({ players: [makePlayer({ id: 'p1' })] }))
      store.currentQuestion = makeQuestion({ options: ['a', 'b'] })

      expect(store.optionVoteCounts).toEqual([0, 0])
    })
  })

  describe('setAnswerResult', () => {
    it('記錄本人作答結果並停止計時器', () => {
      const store = useGameStore()
      store.setRoom(makeRoom())
      store.setQuestion(makeQuestion({ timeLimit: 10 }))

      const payload: AnswerResultPayload = {
        isCorrect: true,
        correctAnswerIndex: 1,
        pointsEarned: 100
      }
      store.setAnswerResult(payload)

      expect(store.isCorrect).toBe(true)
      expect(store.correctAnswerIndex).toBe(1)
      expect(store.pointsEarned).toBe(100)

      const before = store.timeLeft
      vi.advanceTimersByTime(5000)
      expect(store.timeLeft).toBe(before)
    })
  })

  describe('setQuestionEnd', () => {
    it('切換到 result phase 並更新自己的排名', () => {
      const store = useGameStore()
      store.setRoom(makeRoom())
      store.setMyNickname('Alice')

      const payload: QuestionEndPayload = {
        answerIndex: 2,
        explanation: '因為...',
        rankings: [{ rank: 1, nickname: 'Alice', score: 300 }]
      }
      store.setQuestionEnd(payload)

      expect(store.phase).toBe('result')
      expect(store.correctAnswerIndex).toBe(2)
      expect(store.currentExplanation).toBe('因為...')
      expect(store.myScore).toBe(300)
      expect(store.myRank).toBe(1)
      expect(store.timeLeft).toBe(5)
    })

    it('找不到自己時不更新分數/排名', () => {
      const store = useGameStore()
      store.setMyNickname('Nobody')

      store.setQuestionEnd({
        answerIndex: 0,
        explanation: '',
        rankings: [{ rank: 1, nickname: 'Alice', score: 300 }]
      })

      expect(store.myScore).toBe(0)
      expect(store.myRank).toBe(0)
    })
  })

  describe('endGame', () => {
    it('切換到 ended phase 並停止計時器', () => {
      const store = useGameStore()
      store.setMyNickname('Alice')

      store.endGame([{ rank: 1, nickname: 'Alice', score: 500 }])

      expect(store.phase).toBe('ended')
      expect(store.myScore).toBe(500)
      expect(store.myRank).toBe(1)
    })
  })

  describe('setRoomState (重新連線同步)', () => {
    it('question phase：套用題目並啟動計時器', () => {
      const store = useGameStore()
      const payload: RoomStateSyncPayload = {
        phase: 'question',
        room: makeRoom(),
        currentQuestion: makeQuestion({ timeLimit: 12 }),
        answeredCount: 1,
        totalCount: 4
      }

      store.setRoomState(payload)

      expect(store.phase).toBe('question')
      expect(store.currentQuestion?.timeLimit).toBe(12)
      expect(store.answeredCount).toBe(1)
      expect(store.totalCount).toBe(4)
      expect(store.timeLeft).toBe(12)
    })

    it('reveal phase 對應為 result，並帶入題目結束資料', () => {
      const store = useGameStore()
      const payload: RoomStateSyncPayload = {
        phase: 'reveal',
        room: makeRoom(),
        questionEnd: {
          answerIndex: 1,
          explanation: 'exp',
          rankings: [{ rank: 1, nickname: 'Alice', score: 200 }]
        }
      }

      store.setRoomState(payload)

      expect(store.phase).toBe('result')
      expect(store.correctAnswerIndex).toBe(1)
      expect(store.currentExplanation).toBe('exp')
      expect(store.rankings).toHaveLength(1)
      expect(store.timeLeft).toBe(5)
    })

    it('ended phase：套用最終排名並停止計時器', () => {
      const store = useGameStore()
      const payload: RoomStateSyncPayload = {
        phase: 'ended',
        room: makeRoom(),
        rankings: [{ rank: 1, nickname: 'Alice', score: 999 }]
      }

      store.setRoomState(payload)

      expect(store.phase).toBe('ended')
      expect(store.rankings).toEqual([{ rank: 1, nickname: 'Alice', score: 999 }])
    })

    it('lobby phase 對應為 waiting', () => {
      const store = useGameStore()

      store.setRoomState({ phase: 'lobby', room: makeRoom() })

      expect(store.phase).toBe('waiting')
    })
  })

  describe('reset', () => {
    it('將所有狀態還原成初始值', () => {
      const store = useGameStore()
      store.setRoom(makeRoom())
      store.setQuestion(makeQuestion())
      store.setMyNickname('Alice')
      store.endGame([{ rank: 1, nickname: 'Alice', score: 999 }])

      store.reset()

      expect(store.room).toBeNull()
      expect(store.phase).toBe('idle')
      expect(store.currentQuestion).toBeNull()
      expect(store.selectedAnswerIndex).toBeNull()
      expect(store.rankings).toEqual([])
      expect(store.myNickname).toBe('')
      expect(store.myScore).toBe(0)
      expect(store.myRank).toBe(0)
      expect(store.answeredCount).toBe(0)
      expect(store.totalCount).toBe(0)
    })
  })
})
