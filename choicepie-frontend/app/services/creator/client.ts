import type { CreatorProfileDto } from '~/types/api'

export const useCreatorClientApi = () => {
  const api = useApi()

  return {
    fetchCreatorProfile: (id: string) =>
      api.get<CreatorProfileDto>(`/api/v1/creators/${id}`),
    followCreator: (id: string) =>
      api.put(`/api/v1/creators/${id}/follow`),
    unfollowCreator: (id: string) =>
      api.del(`/api/v1/creators/${id}/follow`)
  }
}
