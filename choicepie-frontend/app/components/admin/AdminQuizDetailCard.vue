<template>
  <div class="relative grid grid-cols-[1fr_152px] rounded-2xl bg-white border border-neutral-200 overflow-hidden">
    <div class="p-6 min-w-0 flex flex-col">
      <p class="text-lg font-bold">
        {{ quiz.title }}
      </p>
      <p class="text-base text-neutral-500 mt-2">
        {{ quiz.description || t('adminQuizzes.noDescription') }}
      </p>

      <div class="grid grid-cols-2 sm:grid-cols-4 gap-3 mt-6">
        <div class="rounded-xl bg-primary-50 p-4">
          <p class="text-xs text-primary-600 font-medium">
            {{ t('adminQuizDetail.questionCount') }}
          </p>
          <p class="text-xl font-bold text-primary-700 mt-1">
            {{ quiz.questionCount }}
          </p>
        </div>
        <div class="rounded-xl bg-warning-50 p-4">
          <p class="text-xs text-warning-600 font-medium">
            {{ t('adminQuizDetail.challengeCount') }}
          </p>
          <p class="text-xl font-bold text-warning-700 mt-1">
            {{ quiz.challengeCount }}
          </p>
        </div>
        <div class="rounded-xl bg-success-50 p-4">
          <p class="text-xs text-success-600 font-medium">
            {{ t('adminQuizDetail.passRate') }}
          </p>
          <p class="text-xl font-bold text-success-700 mt-1">
            {{ quiz.passRate }}%
          </p>
        </div>
        <div class="rounded-xl bg-error-50 p-4">
          <p class="text-xs text-error-600 font-medium">
            {{ t('adminQuizDetail.favoriteCount') }}
          </p>
          <p class="text-xl font-bold text-error-700 mt-1">
            {{ quiz.favoriteCount }}
          </p>
        </div>
      </div>

      <div class="flex items-center justify-between mt-6">
        <NuxtLink
          :to="`/admin/members/${quiz.creatorId}`"
          class="flex items-center gap-2 group"
        >
          <img
            v-if="quiz.creatorAvatar"
            :src="quiz.creatorAvatar"
            class="w-8 h-8 rounded-full object-cover shrink-0"
            alt=""
          >
          <div
            v-else
            class="w-8 h-8 rounded-full bg-neutral-100 flex items-center justify-center text-xs font-bold text-neutral-400 shrink-0"
          >
            {{ quiz.creatorName.charAt(0) }}
          </div>
          <span class="text-base text-neutral-500 group-hover:text-primary-500 group-hover:underline">
            {{ quiz.creatorName }}
          </span>
        </NuxtLink>

        <UButton
          v-if="quiz.status !== 'takendown'"
          size="sm"
          color="error"
          variant="soft"
          @click="emit('take-down')"
        >
          {{ t('adminQuizzes.takeDownAction') }}
        </UButton>
        <UButton
          v-else
          size="sm"
          color="primary"
          variant="soft"
          :loading="isRestoring"
          @click="emit('restore')"
        >
          {{ t('adminQuizzes.restoreAction') }}
        </UButton>
      </div>
    </div>

    <AdminStatusStub
      size="lg"
      :icon="statusIcon(quiz.status)"
      :label="statusLabel(quiz.status)"
      :border-class="statusBorderClass(quiz.status)"
      :bg-class="statusStubBgClass(quiz.status)"
      :text-class="statusTextClass(quiz.status)"
    >
      <template
        v-if="quiz.status === 'takendown'"
        #reason
      >
        <p class="w-full mt-1 pt-2.5 border-t border-dashed border-error-200 text-sm leading-relaxed text-error-600">
          {{ quiz.takedownReason }}
        </p>
      </template>
    </AdminStatusStub>
  </div>
</template>

<script setup lang="ts">
import type { AdminQuizDetailDto } from '~/types/api'

interface Props {
  quiz: AdminQuizDetailDto
  isRestoring: boolean
}

defineProps<Props>()

const emit = defineEmits<{
  'take-down': []
  'restore': []
}>()

const { t } = useI18n()

const statusLabel = (status: string) => ({
  draft: t('adminQuizzes.status.draft'),
  published: t('adminQuizzes.status.published'),
  archived: t('adminQuizzes.status.archived'),
  deleted: t('adminQuizzes.status.deleted'),
  takendown: t('adminQuizzes.status.takendown')
}[status] ?? status)

const statusIcon = (status: string) => ({
  draft: 'i-lucide-file-edit',
  published: 'i-lucide-check-circle-2',
  archived: 'i-lucide-archive',
  deleted: 'i-lucide-trash-2',
  takendown: 'i-lucide-shield-off'
}[status] ?? 'i-lucide-circle')

const statusBorderClass = (status: string) => ({
  draft: 'border-neutral-200',
  published: 'border-success-200',
  archived: 'border-warning-200',
  deleted: 'border-neutral-200',
  takendown: 'border-error-200'
}[status] ?? 'border-neutral-200')

const statusStubBgClass = (status: string) => ({
  draft: 'bg-neutral-100',
  published: 'bg-success-50',
  archived: 'bg-warning-50',
  deleted: 'bg-neutral-100',
  takendown: 'bg-error-50'
}[status] ?? 'bg-neutral-100')

const statusTextClass = (status: string) => ({
  draft: 'text-neutral-600',
  published: 'text-success-700',
  archived: 'text-warning-700',
  deleted: 'text-neutral-600',
  takendown: 'text-error-700'
}[status] ?? 'text-neutral-600')
</script>

<script lang="ts">
export default {
  name: 'AdminQuizDetailCard'
}
</script>

<style scoped lang="scss"></style>
