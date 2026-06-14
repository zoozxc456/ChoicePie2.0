export interface Player {
  connectionId: string
  nickname: string
  score: number
  rank: number
  hasAnswered: boolean
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
