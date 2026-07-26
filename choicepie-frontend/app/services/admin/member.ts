import type { AdminListMembersQuery, AdminMemberDetailDto, AdminMemberSummaryDto, PagedResult } from '~/types/api'

export const useAdminMemberClientApi = () => {
  const api = useApi()

  return {
    fetchMembers: (query?: AdminListMembersQuery) =>
      api.get<PagedResult<AdminMemberSummaryDto>>('/api/v1/admin/members', query),
    fetchMemberById: (id: string) =>
      api.get<AdminMemberDetailDto>(`/api/v1/admin/members/${id}`),
    suspendMember: (id: string, reason: string, until: string | null) =>
      api.post(`/api/v1/admin/members/${id}/suspend`, { reason, until }),
    unsuspendMember: (id: string) =>
      api.post(`/api/v1/admin/members/${id}/unsuspend`)
  }
}
