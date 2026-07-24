import type { AdminListQuizReportsQuery, PagedResult, QuizReportDto } from '~/types/api'

export const useAdminQuizReportClientApi = () => {
  const api = useApi()

  return {
    fetchReports: (query?: AdminListQuizReportsQuery) =>
      api.get<PagedResult<QuizReportDto>>('/api/v1/admin/quiz-reports', query),
    resolveReport: (id: string, note?: string | null) =>
      api.post(`/api/v1/admin/quiz-reports/${id}/resolve`, { note }),
    dismissReport: (id: string, note?: string | null) =>
      api.post(`/api/v1/admin/quiz-reports/${id}/dismiss`, { note })
  }
}
