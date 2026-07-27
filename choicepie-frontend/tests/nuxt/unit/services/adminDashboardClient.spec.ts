import { describe, it, expect, beforeEach, vi } from 'vitest'
import { apiMock } from './mocks/useApi.mock'
import { useAdminDashboardClientApi } from '~/services/admin/dashboard'

describe('useAdminDashboardClientApi', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('fetchSummary 呼叫正確路徑', () => {
    const client = useAdminDashboardClientApi()

    client.fetchSummary()

    expect(apiMock.get).toHaveBeenCalledWith('/api/v1/admin/dashboard/summary')
  })
})
