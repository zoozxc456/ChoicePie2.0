import type { Quiz } from '~/types/quiz'
import type { GameSessionDetailDto, GameSessionSummaryDto } from '~/types/api'

export const buildQuiz = (overrides: Partial<Quiz> = {}): Quiz => ({
  id: 'quiz-1',
  title: 'Space Trivia',
  description: 'All about space',
  coverImageUrl: null,
  coverEmoji: '🚀',
  coverGradient: 'primary',
  difficulty: 'intermediate',
  questionCount: 3,
  challengeCount: 1234,
  passRate: 72,
  creatorId: 'creator-1',
  creatorName: 'Alice',
  questions: [
    { id: 'q1', text: 'Closest planet to the sun?', options: ['Mercury', 'Venus', 'Earth', 'Mars'], answerIndex: 0, explanation: '' },
    { id: 'q2', text: 'Largest planet?', options: ['Mercury', 'Venus', 'Jupiter', 'Mars'], answerIndex: 2, explanation: '' }
  ],
  questionIds: ['q1', 'q2'],
  tags: ['science'],
  isPublic: true,
  status: 'published',
  createdAt: '2026-01-01T00:00:00Z',
  updatedAt: '2026-01-01T00:00:00Z',
  ...overrides
})

export const buildSessionSummary = (overrides: Partial<GameSessionSummaryDto> = {}): GameSessionSummaryDto => ({
  id: 'session-1',
  roomCode: 'ABCD12',
  quizId: 'quiz-1',
  quizTitle: 'Space Trivia',
  coverImageUrl: null,
  coverEmoji: '🚀',
  coverGradient: 'primary',
  playedAtUtc: '2026-01-15T12:00:00Z',
  playerCount: 8,
  questionCount: 5,
  topPlayerName: 'Alice',
  topPlayerScore: 4200,
  myRank: 2,
  myScore: 3100,
  ...overrides
})

export const buildSessionDetail = (overrides: Partial<GameSessionDetailDto> = {}): GameSessionDetailDto => ({
  id: 'session-1',
  roomCode: 'ABCD12',
  quizId: 'quiz-1',
  quizTitle: 'Space Trivia',
  coverImageUrl: null,
  coverEmoji: '🚀',
  coverGradient: 'primary',
  playedAtUtc: '2026-01-15T12:00:00Z',
  playerCount: 3,
  questionCount: 2,
  isHost: true,
  rankings: [
    { rank: 1, nickname: 'Alice', score: 4200 },
    { rank: 2, nickname: 'Bob', score: 3100 },
    { rank: 3, nickname: 'Cathy', score: 900 }
  ],
  myRank: null,
  myScore: null,
  myWrongAnswers: [],
  questionBreakdown: [],
  ...overrides
})
