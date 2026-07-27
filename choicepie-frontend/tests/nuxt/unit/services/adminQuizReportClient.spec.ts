import { describe, it, expect, beforeEach, vi } from 'vitest'
import { apiMock } from './mocks/useApi.mock'
import { useAdminQuizReportClientApi } from '~/services/admin/quizReport'

describe('useAdminQuizReportClientApi', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('fetchReports 帶入查詢參數', () => {
    const client = useAdminQuizReportClientApi()

    client.fetchReports({ status: 'Pending' })

    expect(apiMock.get).toHaveBeenCalledWith('/api/v1/admin/quiz-reports', { status: 'Pending' })
  })

  it('resolveReport 呼叫正確路徑與 body', () => {
    const client = useAdminQuizReportClientApi()

    client.resolveReport('report1', 'removed')

    expect(apiMock.post).toHaveBeenCalledWith('/api/v1/admin/quiz-reports/report1/resolve', { note: 'removed' })
  })

  it('dismissReport 呼叫正確路徑與 body', () => {
    const client = useAdminQuizReportClientApi()

    client.dismissReport('report1', 'not a violation')

    expect(apiMock.post).toHaveBeenCalledWith('/api/v1/admin/quiz-reports/report1/dismiss', { note: 'not a violation' })
  })
})
