import type { RankEntry } from './other'

export interface Player {
  connectionId: string
  nickname: string
  score: number
  rank: number
  hasAnswered: boolean
  /** 該玩家本題所選的選項索引（僅 Host 可見，公布答案前不應揭露給其他玩家） */
  selectedOptionIndex?: number
}

export type RoomStatus = 'waiting' | 'playing' | 'ended'

export type GamePhase = 'idle' | 'waiting' | 'question' | 'result' | 'ended'

export interface GameRoom {
  roomCode: string
  quizId?: string
  quizTitle: string
  status: RoomStatus
  players: Player[]
  currentQuestionIndex: number
  totalQuestions: number
  hostConnectionId: string
}

/** 使用者過去主持過的一場遊戲摘要 */
export interface HostedGameSummary {
  id: string
  quizId: string
  quizTitle: string
  coverEmoji: string
  coverGradient: string
  playerCount: number
  questionCount: number
  topPlayerName: string
  topPlayerScore: number
  playedAt: string
  rankings: RankEntry[]
}

/** 玩家答錯的一道題目，供賽後複習使用 */
export interface WrongAnswerReview {
  questionText: string
  options: string[]
  myAnswerIndex: number
  correctAnswerIndex: number
  explanation: string
}

/** 使用者過去以玩家身分參加過的一場遊戲摘要 */
export interface PlayedGameSummary {
  id: string
  quizTitle: string
  coverEmoji: string
  coverGradient: string
  playerCount: number
  questionCount: number
  myRank: number
  myScore: number
  playedAt: string
  rankings: RankEntry[]
  wrongAnswers: WrongAnswerReview[]
}
