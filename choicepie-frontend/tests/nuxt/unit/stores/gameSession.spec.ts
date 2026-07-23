import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import type { GameSessionSummaryDto, GameSessionDetailDto, PagedResult } from '~/types/api'
import { gameSessionClientMock } from './mocks/gameSessionClient.mock'

const { fetchHostedSessions, fetchPlayedSessions, fetchSessionById } = gameSessionClientMock

const { useGameSessionStore } = await import('~/stores/gameSession')

const summary: GameSessionSummaryDto = {
  id: 'session-1',
  roomCode: 'ABCD',
  quizId: 'quiz-1',
  quizTitle: 'Sample Quiz',
  coverEmoji: '📝',
  coverGradient: 'linear-gradient(135deg,#0f3460,#533483)',
  playedAtUtc: '2026-01-01T00:00:00Z',
  playerCount: 4,
  questionCount: 5,
  topPlayerName: 'Alice',
  topPlayerScore: 500,
  myRank: 1,
  myScore: 500
}

const pagedSummary: PagedResult<GameSessionSummaryDto> = {
  pageNumber: 1,
  pageSize: 20,
  totalCount: 1,
  items: [summary]
}

const detail: GameSessionDetailDto = {
  id: 'session-1',
  roomCode: 'ABCD',
  quizId: 'quiz-1',
  quizTitle: 'Sample Quiz',
  coverEmoji: '📝',
  coverGradient: 'linear-gradient(135deg,#0f3460,#533483)',
  playedAtUtc: '2026-01-01T00:00:00Z',
  playerCount: 4,
  questionCount: 5,
  isHost: true,
  rankings: [{ rank: 1, nickname: 'Alice', score: 500 }],
  myRank: 1,
  myScore: 500,
  myWrongAnswers: []
}

describe('useGameSessionStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    vi.spyOn(console, 'error').mockImplementation(() => {})
  })

  describe('fetchHostedSessions', () => {
    it('成功時儲存 hostedSessions', async () => {
      fetchHostedSessions.mockResolvedValue(pagedSummary)
      const store = useGameSessionStore()

      await store.fetchHostedSessions()

      expect(store.hostedSessions).toEqual([summary])
      expect(store.isLoading).toBe(false)
      expect(store.error).toBeNull()
    })

    it('失敗時設定 error 訊息並保留空陣列', async () => {
      fetchHostedSessions.mockRejectedValue(new Error('boom'))
      const store = useGameSessionStore()

      await store.fetchHostedSessions()

      expect(store.error).toBe('無法載入主持紀錄')
      expect(store.hostedSessions).toEqual([])
      expect(store.isLoading).toBe(false)
    })
  })

  describe('fetchPlayedSessions', () => {
    it('成功時儲存 playedSessions', async () => {
      fetchPlayedSessions.mockResolvedValue(pagedSummary)
      const store = useGameSessionStore()

      await store.fetchPlayedSessions()

      expect(store.playedSessions).toEqual([summary])
    })

    it('失敗時設定 error 訊息', async () => {
      fetchPlayedSessions.mockRejectedValue(new Error('boom'))
      const store = useGameSessionStore()

      await store.fetchPlayedSessions()

      expect(store.error).toBe('無法載入遊玩紀錄')
      expect(store.playedSessions).toEqual([])
    })
  })

  describe('fetchSessionById', () => {
    it('成功時儲存 currentSession 並回傳資料', async () => {
      fetchSessionById.mockResolvedValue(detail)
      const store = useGameSessionStore()

      const result = await store.fetchSessionById('session-1')

      expect(fetchSessionById).toHaveBeenCalledWith('session-1')
      expect(result).toEqual(detail)
      expect(store.currentSession).toEqual(detail)
    })

    it('失敗時設定 error 訊息並拋出例外', async () => {
      fetchSessionById.mockRejectedValue(new Error('not found'))
      const store = useGameSessionStore()

      await expect(store.fetchSessionById('session-1')).rejects.toThrow('not found')

      expect(store.error).toBe('無法載入這場遊戲紀錄')
      expect(store.currentSession).toBeNull()
      expect(store.isLoading).toBe(false)
    })
  })
})
