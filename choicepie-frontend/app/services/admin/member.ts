import type {
  AdminListMembersQuery,
  AdminMemberCommentDto,
  AdminMemberDetailDto,
  AdminMemberSummaryDto,
  GameSessionSummaryDto,
  PagedResult,
  QuizSummaryDto
} from '~/types/api'

export const useAdminMemberClientApi = () => {
  const api = useApi()

  return {
    fetchMembers: (query?: AdminListMembersQuery) =>
      api.get<PagedResult<AdminMemberSummaryDto>>('/api/v1/admin/members', query),
    fetchMemberById: (id: string) =>
      api.get<AdminMemberDetailDto>(`/api/v1/admin/members/${id}`),
    fetchMemberQuizzes: (id: string, pageNumber = 1, pageSize = 20) =>
      api.get<PagedResult<QuizSummaryDto>>(`/api/v1/admin/members/${id}/quizzes`, { pageNumber, pageSize }),
    fetchMemberHostedSessions: (id: string, pageNumber = 1, pageSize = 20) =>
      api.get<PagedResult<GameSessionSummaryDto>>(`/api/v1/admin/members/${id}/hosted-sessions`, { pageNumber, pageSize }),
    fetchMemberPlayedSessions: (id: string, pageNumber = 1, pageSize = 20) =>
      api.get<PagedResult<GameSessionSummaryDto>>(`/api/v1/admin/members/${id}/played-sessions`, { pageNumber, pageSize }),
    fetchMemberComments: (id: string, pageNumber = 1, pageSize = 20) =>
      api.get<PagedResult<AdminMemberCommentDto>>(`/api/v1/admin/members/${id}/comments`, { pageNumber, pageSize }),
    suspendMember: (id: string, reason: string, until: string | null) =>
      api.post(`/api/v1/admin/members/${id}/suspend`, { reason, until }),
    unsuspendMember: (id: string) =>
      api.post(`/api/v1/admin/members/${id}/unsuspend`)
  }
}
