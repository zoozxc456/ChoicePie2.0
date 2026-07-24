import type { AdminListMembersQuery, AdminMemberSummaryDto, PagedResult } from '~/types/api'

export const useAdminMemberClientApi = () => {
  const api = useApi()

  return {
    fetchMembers: (query?: AdminListMembersQuery) =>
      api.get<PagedResult<AdminMemberSummaryDto>>('/api/v1/admin/members', query),
    suspendMember: (id: string, reason: string, until: string | null) =>
      api.post(`/api/v1/admin/members/${id}/suspend`, { reason, until }),
    unsuspendMember: (id: string) =>
      api.post(`/api/v1/admin/members/${id}/unsuspend`)
  }
}
