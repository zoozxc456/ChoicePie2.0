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
