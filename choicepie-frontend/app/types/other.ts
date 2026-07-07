import type { Difficulty, Question } from './quiz'
import type { GameRoom, GamePhase } from './gameRoom'

/** Server → Client: 推送新題目（不含正確答案） */
export interface QuestionPayload {
  index: number
  total: number
  text: string
  options: string[]
  timeLimit: number
}

/** Server → Client: 題目結束後的排名快照 */
export interface QuestionEndPayload {
  answerIndex: number
  explanation: string
  rankings: RankEntry[]
}

/** Server → Client: 每位玩家自己的作答結果 */
export interface AnswerResultPayload {
  isCorrect: boolean
  correctAnswerIndex: number
  pointsEarned: number
}

/** Server → Client: 答題進度（僅 Host 收到，含所選選項供即時統計用） */
export interface AnswerProgressPayload {
  answered: number
  total: number
  connectionId: string
  selectedOptionIndex: number
}

export interface RankEntry {
  rank: number
  nickname: string
  score: number
  delta?: number // 本題得分增量
}

// ─────────────────────────────────────────────
// API Request / Response
// ─────────────────────────────────────────────

export interface ApiResponse<T> {
  data: T
  success: boolean
  message?: string
}

export interface GenerateQuestionsRequest {
  content: string
  questionCount: 3 | 5 | 10
  difficulty: Difficulty
}

export interface GenerateQuestionsResponse {
  questions: Question[]
  tokensUsed: number
}

export interface CreateRoomRequest {
  quizId?: string
  questions: Question[]
  timeLimit?: number
}

export interface CreateRoomResponse {
  roomCode: string
}

export interface JoinRoomRequest {
  roomCode: string
  nickname: string
}

/** Server → Client: 重新連線後同步完整房間狀態（用於重新整理 / 直接開啟房間頁） */
export interface RoomStateSyncPayload {
  phase: GamePhase
  room: GameRoom
  currentQuestion?: QuestionPayload
  answeredCount?: number
  totalCount?: number
  questionEnd?: QuestionEndPayload
  rankings?: RankEntry[]
}

// ─────────────────────────────────────────────
// UI Helpers
// ─────────────────────────────────────────────

export type OptionState = 'default' | 'selected' | 'correct' | 'wrong' | 'disabled'
