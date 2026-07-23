import type { Difficulty } from '~/types/quiz'
import type { QuizDto, QuizSummaryDto, PagedResult, CreateQuestionRequestItem, GenerateQuestionsResultDto, QuizForAttemptDto } from '~/types/api'

export const useQuizClientApi = () => {
  const api = useApi()

  return {
    fetchQuizzes: (params?: { tag?: string, search?: string, mine?: boolean }) =>
      api.get<PagedResult<QuizSummaryDto>>('/api/v1/quizzes', params),
    fetchQuizById: (id: string) =>
      api.get<QuizDto>(`/api/v1/quizzes/${id}`),
    fetchQuizPreview: (id: string) =>
      api.get<QuizForAttemptDto>(`/api/v1/quizzes/${id}/preview`),
    fetchTags: () =>
      api.get<string[]>('/api/v1/quizzes/tags'),
    updateQuiz: (id: string, payload: { title: string, description: string | null, tags: string[] }) =>
      api.put<QuizDto>(`/api/v1/quizzes/${id}`, payload),
    deleteQuiz: (id: string) =>
      api.del(`/api/v1/quizzes/${id}`),
    addQuestion: (quizId: string, question: CreateQuestionRequestItem) =>
      api.post<QuizDto>(`/api/v1/quizzes/${quizId}/questions`, question),
    updateQuestion: (quizId: string, questionId: string, question: CreateQuestionRequestItem) =>
      api.put<QuizDto>(`/api/v1/quizzes/${quizId}/questions/${questionId}`, question),
    removeQuestion: (quizId: string, questionId: string) =>
      api.del<QuizDto>(`/api/v1/quizzes/${quizId}/questions/${questionId}`),
    publishQuiz: (id: string) =>
      api.post<QuizDto>(`/api/v1/quizzes/${id}/publish`),
    unpublishQuiz: (id: string) =>
      api.post<QuizDto>(`/api/v1/quizzes/${id}/unpublish`),
    archiveQuiz: (id: string) =>
      api.post<QuizDto>(`/api/v1/quizzes/${id}/archive`),
    generateQuestions: (content: string, questionCount: 3 | 5 | 10, difficulty: Difficulty) =>
      api.post<GenerateQuestionsResultDto>('/api/v1/quizzes/generate-questions', { content, questionCount, difficulty }),
    saveQuiz: (payload: { title: string, description: string | null, coverEmoji: string, coverGradient: string, difficulty: Difficulty, tags: string[], questions: CreateQuestionRequestItem[] }) =>
      api.post<QuizDto>('/api/v1/quizzes', payload),
    fetchFavoriteStatus: (id: string) =>
      api.get<boolean>(`/api/v1/quizzes/${id}/favorite`),
    addFavorite: (id: string) =>
      api.put(`/api/v1/quizzes/${id}/favorite`),
    removeFavorite: (id: string) =>
      api.del(`/api/v1/quizzes/${id}/favorite`)
  }
}
