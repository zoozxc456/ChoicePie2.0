import type { CreatorProfileDto, FollowedCreatorDto, PagedResult, QuizSummaryDto } from '~/types/api'

export const useCreatorClientApi = () => {
  const api = useApi()

  return {
    fetchCreatorProfile: (id: string) =>
      api.get<CreatorProfileDto>(`/api/v1/creators/${id}`),
    fetchCreatorQuizzes: (id: string, page = 1, pageSize = 12) =>
      api.get<PagedResult<QuizSummaryDto>>(`/api/v1/creators/${id}/quizzes`, { page, pageSize }),
    fetchFollowing: (page = 1, pageSize = 20) =>
      api.get<PagedResult<FollowedCreatorDto>>('/api/v1/me/following', { page, pageSize }),
    fetchFollowingQuizzes: (page = 1, pageSize = 12) =>
      api.get<PagedResult<QuizSummaryDto>>('/api/v1/me/following/quizzes', { page, pageSize }),
    followCreator: (id: string) =>
      api.put(`/api/v1/creators/${id}/follow`),
    unfollowCreator: (id: string) =>
      api.del(`/api/v1/creators/${id}/follow`)
  }
}
