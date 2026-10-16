<template>
  <div class="rounded-2xl bg-white border border-neutral-200 p-4">
    <div class="flex items-start justify-between gap-3">
      <div class="min-w-0 flex-1">
        <p class="text-lg font-bold truncate">
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
          data-testid="report-description"
          class="text-xs text-neutral-500 mt-1"
        >
          {{ report.description }}
        </p>
      </div>
      <StatusBadge
        data-testid="report-status"
        :tone="quizReportStatusTone(report.status)"
        class="shrink-0"
      >
        {{ t(`adminQuizReports.status.${report.status.toLowerCase()}`) }}
      </StatusBadge>
    </div>

    <div
      v-if="report.status === 'Pending'"
      class="flex justify-end gap-2 mt-3"
    >
      <UButton
        data-testid="report-dismiss"
        size="sm"
        color="neutral"
        variant="soft"
        @click="emit('dismiss')"
      >
        {{ t('adminQuizReports.dismissAction') }}
      </UButton>
      <UButton
        data-testid="report-resolve"
        size="sm"
        color="error"
        variant="soft"
        @click="emit('resolve')"
      >
        {{ t('adminQuizReports.resolveAction') }}
      </UButton>
    </div>
  </div>
</template>

<script setup lang="ts">
import StatusBadge from '~/components/common/StatusBadge.vue'
import type { QuizReportDto } from '~/types/api'

interface Props {
  report: QuizReportDto
}

defineProps<Props>()

const emit = defineEmits<{
  resolve: []
  dismiss: []
}>()

const { t } = useI18n()
</script>

<script lang="ts">
export default {
  name: 'AdminQuizReportCard'
}
</script>

<style scoped lang="scss"></style>
