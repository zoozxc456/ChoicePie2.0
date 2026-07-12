import type { GameSessionSummaryDto, GameSessionDetailDto, PagedResult } from '~/types/api'

export const useGameSessionClientApi = () => {
  const api = useApi()

  return {
    fetchHostedSessions: () =>
      api.get<PagedResult<GameSessionSummaryDto>>('/api/v1/game-sessions/hosted'),
    fetchPlayedSessions: () =>
      api.get<PagedResult<GameSessionSummaryDto>>('/api/v1/game-sessions/played'),
    fetchSessionById: (id: string) =>
      api.get<GameSessionDetailDto>(`/api/v1/game-sessions/${id}`)
  }
}
