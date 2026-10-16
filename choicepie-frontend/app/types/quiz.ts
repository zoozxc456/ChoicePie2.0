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
  coverImageUrl: string | null
  coverEmoji: string
  coverGradient: string // 品牌色鍵：primary | secondary | success | danger | warning | info
  difficulty: Difficulty
  questionCount: number
  challengeCount: number
  passRate: number // 0–100
  creatorId: string
  creatorName: string
  creatorAvatar?: string
  // Empty when the viewer isn't the quiz owner - use questionIds for room creation instead.
  questions: Question[]
  questionIds: string[]
  tags: string[]
  isPublic: boolean
  status: string
  shareCount?: number
  createdAt: string
  updatedAt: string
}
