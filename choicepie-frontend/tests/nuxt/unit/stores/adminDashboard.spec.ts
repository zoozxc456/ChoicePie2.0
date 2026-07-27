import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import type { AdminDashboardSummaryDto } from '~/types/api'
import { adminDashboardClientMock } from './mocks/adminDashboardClient.mock'

const { fetchSummary } = adminDashboardClientMock

const { useAdminDashboardStore } = await import('~/stores/adminDashboard')

const makeSummary = (overrides: Partial<AdminDashboardSummaryDto> = {}): AdminDashboardSummaryDto => ({
  pendingQuizReportCount: 2,
  totalMemberCount: 100,
  suspendedMemberCount: 3,
  newMemberCountLast7Days: 5,
  totalQuizCount: 50,
  takenDownQuizCount: 1,
  newQuizCountLast7Days: 4,
  takenDownQuizCountLast7Days: 1,
  newMembersByDay: [{ date: '2026-01-01', count: 5 }],
  newQuizzesByDay: [{ date: '2026-01-01', count: 4 }],
  takenDownQuizzesByDay: [{ date: '2026-01-01', count: 1 }],
  topQuizzes: [],
  generatedAt: '2026-01-01T00:00:00Z',
  ...overrides
})

describe('useAdminDashboardStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    vi.spyOn(console, 'error').mockImplementation(() => {})
  })

  describe('fetchSummary', () => {
    it('成功時儲存 summary 並回傳資料', async () => {
      const summary = makeSummary()
      fetchSummary.mockResolvedValue(summary)
      const store = useAdminDashboardStore()

      const result = await store.fetchSummary()

      expect(fetchSummary).toHaveBeenCalled()
      expect(result).toEqual(summary)
      expect(store.summary).toEqual(summary)
      expect(store.isLoading).toBe(false)
    })

    it('失敗時設定 error 並往外拋出', async () => {
      fetchSummary.mockRejectedValue(new Error('boom'))
      const store = useAdminDashboardStore()

      await expect(store.fetchSummary()).rejects.toThrow('boom')

      expect(store.error).toBe('無法載入儀表板摘要')
      expect(store.isLoading).toBe(false)
    })
  })
})
