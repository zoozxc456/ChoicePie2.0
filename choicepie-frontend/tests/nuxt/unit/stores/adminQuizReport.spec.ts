import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import type { PagedResult, QuizReportDto } from '~/types/api'
import { adminQuizReportClientMock } from './mocks/adminQuizReportClient.mock'

const { fetchReports, resolveReport, dismissReport } = adminQuizReportClientMock

const { useAdminQuizReportStore } = await import('~/stores/adminQuizReport')

const makeReport = (overrides: Partial<QuizReportDto> = {}): QuizReportDto => ({
  id: 'report-1',
  quizId: 'quiz-1',
  quizTitle: 'Sample Quiz',
  reporterId: 'member-1',
  reporterName: 'Reporter',
  reason: 'Spam',
  description: null,
  status: 'Pending',
  resolvedBy: null,
  resolvedAt: null,
  resolutionNote: null,
  createdAt: '2026-01-01T00:00:00Z',
  ...overrides
})

const pagedResult: PagedResult<QuizReportDto> = {
  pageNumber: 1,
  pageSize: 20,
  totalCount: 1,
  items: [makeReport()]
}

describe('useAdminQuizReportStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    vi.spyOn(console, 'error').mockImplementation(() => {})
  })

  describe('fetchReports', () => {
    it('成功時儲存 reports 並回傳資料', async () => {
      fetchReports.mockResolvedValue(pagedResult)
      const store = useAdminQuizReportStore()

      const result = await store.fetchReports({ status: 'Pending' })

      expect(fetchReports).toHaveBeenCalledWith({ status: 'Pending' })
      expect(result).toEqual(pagedResult)
      expect(store.reports).toEqual(pagedResult)
    })

    it('失敗時設定 error 並往外拋出', async () => {
      fetchReports.mockRejectedValue(new Error('boom'))
      const store = useAdminQuizReportStore()

      await expect(store.fetchReports()).rejects.toThrow('boom')

      expect(store.error).toBe('無法載入檢舉列表')
      expect(store.isLoading).toBe(false)
    })
  })

  describe('resolveReport', () => {
    it('成功時將該檢舉狀態更新為 Resolved', async () => {
      fetchReports.mockResolvedValue(pagedResult)
      resolveReport.mockResolvedValue(undefined)
      const store = useAdminQuizReportStore()
      await store.fetchReports()

      await store.resolveReport('report-1', 'removed')

      expect(resolveReport).toHaveBeenCalledWith('report-1', 'removed')
      expect(store.reports?.items[0]?.status).toBe('Resolved')
    })

    it('失敗時設定 error 並往外拋出', async () => {
      resolveReport.mockRejectedValue(new Error('boom'))
      const store = useAdminQuizReportStore()

      await expect(store.resolveReport('report-1')).rejects.toThrow('boom')

      expect(store.error).toBe('處理檢舉失敗，請稍後再試')
    })
  })

  describe('dismissReport', () => {
    it('成功時將該檢舉狀態更新為 Dismissed', async () => {
      fetchReports.mockResolvedValue(pagedResult)
      dismissReport.mockResolvedValue(undefined)
      const store = useAdminQuizReportStore()
      await store.fetchReports()

      await store.dismissReport('report-1', 'not a violation')

      expect(dismissReport).toHaveBeenCalledWith('report-1', 'not a violation')
      expect(store.reports?.items[0]?.status).toBe('Dismissed')
    })

    it('失敗時設定 error 並往外拋出', async () => {
      dismissReport.mockRejectedValue(new Error('boom'))
      const store = useAdminQuizReportStore()

      await expect(store.dismissReport('report-1')).rejects.toThrow('boom')

      expect(store.error).toBe('駁回檢舉失敗，請稍後再試')
    })
  })
})
