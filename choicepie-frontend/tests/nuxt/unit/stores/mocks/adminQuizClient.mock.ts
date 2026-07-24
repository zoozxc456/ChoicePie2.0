import { vi } from 'vitest'

export const adminQuizClientMock = {
  fetchQuizzes: vi.fn(),
  takeDownQuiz: vi.fn(),
  restoreQuiz: vi.fn()
}

vi.mock('~/services/admin/quiz', () => ({
  useAdminQuizClientApi: () => adminQuizClientMock
}))
