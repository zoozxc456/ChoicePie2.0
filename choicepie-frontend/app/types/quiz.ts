export type Difficulty = 'beginner' | 'intermediate' | 'expert'

export const DIFFICULTY_LABEL: Record<Difficulty, string> = {
  beginner: '入門',
  intermediate: '進階',
  expert: '專家'
}

export interface Question {
  id: string
  text: string
  options: string[]
  answerIndex: number
  explanation: string
}

export interface Quiz {
  id: string
  title: string
  description?: string
  coverEmoji: string
  coverGradient: string // e.g. 'linear-gradient(135deg,#0f3460,#533483)'
  difficulty: Difficulty
  questionCount: number
  challengeCount: number
  passRate: number // 0–100
  creatorId: string
  creatorName: string
  creatorAvatar?: string
  questions: Question[]
  tags: string[]
  isPublic: boolean
  createdAt: string
  updatedAt: string
}
