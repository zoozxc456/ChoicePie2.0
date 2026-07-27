import type { AdminDashboardSummaryDto } from '~/types/api'

export const useAdminDashboardClientApi = () => {
  const api = useApi()

  return {
    fetchSummary: () => api.get<AdminDashboardSummaryDto>('/api/v1/admin/dashboard/summary')
  }
}
