import type { AdminListQuizzesQuery, PagedResult, QuizSummaryDto } from '~/types/api'

export const useAdminQuizClientApi = () => {
  const api = useApi()

  return {
    fetchQuizzes: (query?: AdminListQuizzesQuery) =>
      api.get<PagedResult<QuizSummaryDto>>('/api/v1/admin/quizzes', query),
    takeDownQuiz: (id: string, reason: string) =>
      api.post(`/api/v1/admin/quizzes/${id}/takedown`, { reason }),
    restoreQuiz: (id: string) =>
      api.post(`/api/v1/admin/quizzes/${id}/restore`)
  }
}
