import { useAdminDashboardClientApi } from '~/services/admin/dashboard'
import type { AdminDashboardSummaryDto } from '~/types/api'

export const useAdminDashboardStore = defineStore('adminDashboard', () => {
  const adminDashboardApi = useAdminDashboardClientApi()

  const summary = ref<AdminDashboardSummaryDto | null>(null)
  const isLoading = ref(false)
  const error = ref<string | null>(null)

  const fetchSummary = async () => {
    isLoading.value = true
    error.value = null
    try {
      summary.value = await adminDashboardApi.fetchSummary()
      return summary.value
    } catch (e) {
      error.value = '無法載入儀表板摘要'
      console.error(e)
      throw e
    } finally {
      isLoading.value = false
    }
  }

  return {
    summary,
    isLoading,
    error,
    fetchSummary
  }
})
