<template>
  <div class="max-w-4xl mx-auto">
    <h1 class="text-lg font-extrabold mb-4">
      {{ t('adminQuizReports.title') }}
    </h1>

    <select
      v-model="statusFilter"
      class="w-full rounded-xl border border-neutral-200 px-4 py-2 text-sm mb-4"
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

    <div
      v-if="adminQuizReportStore.isLoading"
      class="flex justify-center py-16"
    >
      <UIcon
        name="i-lucide-loader-2"
        class="animate-spin text-3xl text-primary-500"
      />
    </div>

    <p
      v-else-if="!reports.length"
      class="text-sm text-neutral-400 text-center py-16"
    >
      {{ t('adminQuizReports.empty') }}
    </p>

    <div
      v-else
      class="flex flex-col gap-3"
    >
      <div
        v-for="report in reports"
        :key="report.id"
        class="rounded-2xl bg-white border border-neutral-200 p-4"
      >
        <div class="flex items-start justify-between gap-3">
          <div class="min-w-0 flex-1">
            <p class="text-sm font-bold truncate">
              {{ report.quizTitle }}
            </p>
            <p class="text-xs text-neutral-400 mt-0.5">
              {{ t('adminQuizReports.reporterLine', { name: report.reporterName }) }}
            </p>
            <p class="text-xs text-neutral-500 mt-1">
              {{ t(`adminQuizReports.reasons.${report.reason}`) }}
            </p>
            <p
              v-if="report.description"
              class="text-xs text-neutral-500 mt-1"
            >
              {{ report.description }}
            </p>
          </div>
          <span
            class="text-[11px] px-2 py-1 rounded-full font-semibold whitespace-nowrap shrink-0"
            :class="statusBadgeClass(report.status)"
          >
            {{ t(`adminQuizReports.status.${report.status.toLowerCase()}`) }}
          </span>
        </div>

        <div
          v-if="report.status === 'Pending'"
          class="flex justify-end gap-2 mt-3"
        >
          <UButton
            size="sm"
            color="neutral"
            variant="soft"
            @click="openModal(report.id, 'dismiss')"
          >
            {{ t('adminQuizReports.dismissAction') }}
          </UButton>
          <UButton
            size="sm"
            color="error"
            variant="soft"
            @click="openModal(report.id, 'resolve')"
          >
            {{ t('adminQuizReports.resolveAction') }}
          </UButton>
        </div>
      </div>
    </div>

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
definePageMeta({ layout: 'admin', middleware: ['admin-auth'] })

const { t } = useI18n()
const adminQuizReportStore = useAdminQuizReportStore()

const statusFilter = ref('Pending')
const isModalOpen = ref(false)
const modalAction = ref<'resolve' | 'dismiss'>('resolve')
const targetReportId = ref<string | null>(null)

const reports = computed(() => adminQuizReportStore.reports?.items ?? [])

await adminQuizReportStore.fetchReports({ status: statusFilter.value })

const handleStatusChange = () => {
  adminQuizReportStore.fetchReports({ status: statusFilter.value })
}

const statusBadgeClass = (status: string) => ({
  Pending: 'bg-warning-100 text-warning-800',
  Resolved: 'bg-error-100 text-error-800',
  Dismissed: 'bg-neutral-100 text-neutral-600'
}[status] ?? 'bg-neutral-100 text-neutral-600')

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
