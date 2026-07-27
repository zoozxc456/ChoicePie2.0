import type { AdminListQuizzesQuery, AdminQuizDetailDto, CommentDto, PagedResult, QuizSummaryDto } from '~/types/api'

export const useAdminQuizClientApi = () => {
  const api = useApi()

  return {
    fetchQuizzes: (query?: AdminListQuizzesQuery) =>
      api.get<PagedResult<QuizSummaryDto>>('/api/v1/admin/quizzes', query),
    fetchQuizById: (id: string) =>
      api.get<AdminQuizDetailDto>(`/api/v1/admin/quizzes/${id}`),
    fetchQuizComments: (id: string, pageNumber = 1, pageSize = 20) =>
      api.get<PagedResult<CommentDto>>(`/api/v1/admin/quizzes/${id}/comments`, { pageNumber, pageSize }),
    takeDownQuiz: (id: string, reason: string) =>
      api.post(`/api/v1/admin/quizzes/${id}/takedown`, { reason }),
    restoreQuiz: (id: string) =>
      api.post(`/api/v1/admin/quizzes/${id}/restore`)
  }
}
