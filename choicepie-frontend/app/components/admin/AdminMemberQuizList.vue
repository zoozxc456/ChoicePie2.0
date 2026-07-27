<template>
  <div class="rounded-2xl bg-white border border-neutral-200 p-5">
    <p class="text-lg font-bold mb-3">
      {{ t('adminMemberDetail.quizzesTitle') }}
    </p>
    <div
      v-if="isLoading"
      class="flex justify-center py-8"
    >
      <UIcon
        name="i-lucide-loader-2"
        class="animate-spin text-2xl text-primary-500"
      />
    </div>
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
          <span
            class="text-[11px] px-2 py-0.5 rounded-full font-semibold whitespace-nowrap shrink-0"
            :class="statusBadgeClass(quiz.status)"
          >
            {{ statusLabel(quiz.status) }}
          </span>
        </NuxtLink>
      </li>
    </ul>
  </div>
</template>

<script setup lang="ts">
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

const statusBadgeClass = (status: string) => ({
  draft: 'bg-neutral-100 text-neutral-600',
  published: 'bg-success-100 text-success-800',
  archived: 'bg-warning-100 text-warning-800',
  deleted: 'bg-neutral-100 text-neutral-600',
  takendown: 'bg-error-100 text-error-800'
}[status] ?? 'bg-neutral-100 text-neutral-600')
</script>

<script lang="ts">
export default {
  name: 'AdminMemberQuizList'
}
</script>

<style scoped lang="scss"></style>
