<template>
  <div>
    <h1 class="text-lg font-extrabold mb-4">
      {{ t('adminQuizReports.title') }}
    </h1>

    <select
      v-model="statusFilter"
      class="w-full rounded-xl border border-neutral-200 px-4 py-2 text-base mb-4"
      @change="handleStatusChange"
    >
      <option value="Pending">
        {{ t('adminQuizReports.status.pending') }}
      </option>
      <option value="Resolved">
        {{ t('adminQuizReports.status.resolved') }}
      </option>
      <option value="Dismissed">
        {{ t('adminQuizReports.status.dismissed') }}
      </option>
    </select>

    <LoadingSpinner
      v-if="adminQuizReportStore.isLoading"
      size="md"
    />

    <p
      v-else-if="!reports.length"
      class="text-base text-neutral-400 text-center py-16"
    >
      {{ t('adminQuizReports.empty') }}
    </p>

    <div
      v-else
      class="flex flex-col gap-3"
    >
      <AdminQuizReportCard
        v-for="report in reports"
        :key="report.id"
        :report="report"
        @resolve="openModal(report.id, 'resolve')"
        @dismiss="openModal(report.id, 'dismiss')"
      />
    </div>

    <AdminPagination
      :page-number="pageNumber"
      :total-pages="totalPages"
      @update:page-number="handlePageChange"
    />

    <ResolveQuizReportModal
      :open="isModalOpen"
      :action="modalAction"
      :is-submitting="adminQuizReportStore.isResolving || adminQuizReportStore.isDismissing"
      @confirm="handleConfirm"
      @cancel="closeModal"
    />
  </div>
</template>

<script setup lang="ts">
import AdminQuizReportCard from '~/components/admin/AdminQuizReportCard.vue'
import ResolveQuizReportModal from '~/components/admin/ResolveQuizReportModal.vue'
import LoadingSpinner from '~/components/common/LoadingSpinner.vue'
import { totalPagesOf } from '~/utils/pagination'

definePageMeta({ layout: 'admin', middleware: ['admin-auth'] })

const { t } = useI18n()
const adminQuizReportStore = useAdminQuizReportStore()

const statusFilter = ref('Pending')
const isModalOpen = ref(false)
const modalAction = ref<'resolve' | 'dismiss'>('resolve')
const targetReportId = ref<string | null>(null)

const reports = computed(() => adminQuizReportStore.reports?.items ?? [])
const pageNumber = computed(() => adminQuizReportStore.reports?.pageNumber ?? 1)
const totalPages = computed(() => totalPagesOf(adminQuizReportStore.reports))

await adminQuizReportStore.fetchReports({ status: statusFilter.value })

const handleStatusChange = () => {
  adminQuizReportStore.fetchReports({ status: statusFilter.value, pageNumber: 1 })
}

const handlePageChange = (page: number) => {
  adminQuizReportStore.fetchReports({ status: statusFilter.value, pageNumber: page })
}

const openModal = (reportId: string, action: 'resolve' | 'dismiss') => {
  targetReportId.value = reportId
  modalAction.value = action
  isModalOpen.value = true
}

const closeModal = () => {
  isModalOpen.value = false
  targetReportId.value = null
}

const handleConfirm = async (note?: string) => {
  if (!targetReportId.value) return
  try {
    if (modalAction.value === 'resolve') {
      await adminQuizReportStore.resolveReport(targetReportId.value, note)
    } else {
      await adminQuizReportStore.dismissReport(targetReportId.value, note)
    }
    closeModal()
  } catch {
    // 錯誤已寫入 adminQuizReportStore.error，這裡不需要額外處理
  }
}
</script>

<script lang="ts">
export default {
  name: 'AdminQuizReportsPage'
}
</script>

<style scoped lang="scss"></style>
