import type { CreateMembershipTierRequest, MembershipTierDto, UpdateMembershipTierRequest } from '~/types/api'

export const useAdminMembershipTierClientApi = () => {
  const api = useApi()

  return {
    fetchTiers: () =>
      api.get<MembershipTierDto[]>('/api/v1/admin/membership-tiers'),
    createTier: (request: CreateMembershipTierRequest) =>
      api.post<MembershipTierDto>('/api/v1/admin/membership-tiers', request),
    updateTier: (id: string, request: UpdateMembershipTierRequest) =>
      api.put<MembershipTierDto>(`/api/v1/admin/membership-tiers/${id}`, request)
  }
}
