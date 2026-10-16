<template>
  <div class="rounded-2xl bg-white border border-neutral-200 p-5">
    <p class="text-lg font-bold mb-3">
      {{ t('adminMemberDetail.quizzesTitle') }}
    </p>
    <LoadingSpinner
      v-if="isLoading"
      size="sm"
    />
    <p
      v-else-if="!quizzes.length"
      class="text-base text-neutral-400 text-center py-8"
    >
      {{ t('adminMemberDetail.quizzesEmpty') }}
    </p>
    <ul
      v-else
      class="flex flex-col divide-y divide-neutral-100"
    >
      <li
        v-for="quiz in quizzes"
        :key="quiz.id"
      >
        <NuxtLink
          :to="`/admin/quizzes/${quiz.id}`"
          class="py-3 flex items-center justify-between gap-3 group"
        >
          <span class="text-base font-medium truncate group-hover:text-primary-500 group-hover:underline">
            {{ quiz.title }}
          </span>
          <StatusBadge
            :tone="quizStatusTone(quiz.status)"
            class="shrink-0"
          >
            {{ statusLabel(quiz.status) }}
          </StatusBadge>
        </NuxtLink>
      </li>
    </ul>
  </div>
</template>

<script setup lang="ts">
import StatusBadge from '~/components/common/StatusBadge.vue'
import LoadingSpinner from '~/components/common/LoadingSpinner.vue'
import type { QuizSummaryDto } from '~/types/api'

interface Props {
  quizzes: QuizSummaryDto[]
  isLoading: boolean
}

defineProps<Props>()

const { t } = useI18n()

const statusLabel = (status: string) => ({
  draft: t('adminQuizzes.status.draft'),
  published: t('adminQuizzes.status.published'),
  archived: t('adminQuizzes.status.archived'),
  deleted: t('adminQuizzes.status.deleted'),
  takendown: t('adminQuizzes.status.takendown')
}[status] ?? status)
</script>

<script lang="ts">
export default {
  name: 'AdminMemberQuizList'
}
</script>

<style scoped lang="scss"></style>
