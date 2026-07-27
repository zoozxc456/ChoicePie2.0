import { useAdminQuizReportClientApi } from '~/services/admin/quizReport'
import type { AdminListQuizReportsQuery, PagedResult, QuizReportDto } from '~/types/api'

export const useAdminQuizReportStore = defineStore('adminQuizReport', () => {
  const adminQuizReportApi = useAdminQuizReportClientApi()

  const reports = ref<PagedResult<QuizReportDto> | null>(null)
  const isLoading = ref(false)
  const isResolving = ref(false)
  const isDismissing = ref(false)
  const error = ref<string | null>(null)

  const fetchReports = async (query?: AdminListQuizReportsQuery) => {
    isLoading.value = true
    error.value = null
    try {
      reports.value = await adminQuizReportApi.fetchReports(query)
      return reports.value
    } catch (e) {
      error.value = '無法載入檢舉列表'
      console.error(e)
      throw e
    } finally {
      isLoading.value = false
    }
  }

  const resolveReport = async (id: string, note?: string | null) => {
    isResolving.value = true
    error.value = null
    try {
      await adminQuizReportApi.resolveReport(id, note)
      if (reports.value) {
        reports.value = {
          ...reports.value,
          items: reports.value.items.map(r => r.id === id ? { ...r, status: 'Resolved' } : r)
        }
      }
    } catch (e) {
      error.value = '處理檢舉失敗，請稍後再試'
      console.error(e)
      throw e
    } finally {
      isResolving.value = false
    }
  }

  const dismissReport = async (id: string, note?: string | null) => {
    isDismissing.value = true
    error.value = null
    try {
      await adminQuizReportApi.dismissReport(id, note)
      if (reports.value) {
        reports.value = {
          ...reports.value,
          items: reports.value.items.map(r => r.id === id ? { ...r, status: 'Dismissed' } : r)
        }
      }
    } catch (e) {
      error.value = '駁回檢舉失敗，請稍後再試'
      console.error(e)
      throw e
    } finally {
      isDismissing.value = false
    }
  }

  return {
    reports,
    isLoading,
    isResolving,
    isDismissing,
    error,
    fetchReports,
    resolveReport,
    dismissReport
  }
})
