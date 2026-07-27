import { vi } from 'vitest'

export const adminDashboardClientMock = {
  fetchSummary: vi.fn()
}

vi.mock('~/services/admin/dashboard', () => ({
  useAdminDashboardClientApi: () => adminDashboardClientMock
}))
