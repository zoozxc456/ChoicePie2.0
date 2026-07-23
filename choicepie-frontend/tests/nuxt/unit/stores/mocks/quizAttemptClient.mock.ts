import { vi } from 'vitest'

export const quizAttemptClientMock = {
  startAttempt: vi.fn(),
  submitAnswer: vi.fn(),
  completeAttempt: vi.fn(),
  fetchAttemptById: vi.fn()
}

vi.mock('~/services/quizAttempt/client', () => ({
  useQuizAttemptClientApi: () => quizAttemptClientMock
}))
