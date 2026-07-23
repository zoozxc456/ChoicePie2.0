import { vi } from 'vitest'

export const quizClientMock = {
  fetchQuizzes: vi.fn(),
  fetchQuizById: vi.fn(),
  fetchQuizPreview: vi.fn(),
  fetchTags: vi.fn(),
  updateQuiz: vi.fn(),
  deleteQuiz: vi.fn(),
  addQuestion: vi.fn(),
  updateQuestion: vi.fn(),
  removeQuestion: vi.fn(),
  publishQuiz: vi.fn(),
  unpublishQuiz: vi.fn(),
  archiveQuiz: vi.fn(),
  fetchFavoriteStatus: vi.fn(),
  addFavorite: vi.fn(),
  removeFavorite: vi.fn(),
  fetchComments: vi.fn(),
  addComment: vi.fn(),
  fetchRelatedQuizzes: vi.fn(),
  generateQuestions: vi.fn(),
  saveQuiz: vi.fn()
}

vi.mock('~/services/quiz/client', () => ({
  useQuizClientApi: () => quizClientMock
}))
