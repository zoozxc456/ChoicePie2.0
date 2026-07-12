<template>
  <div class="bg-white rounded-2xl border border-neutral-200 overflow-hidden p-4 flex flex-col gap-3">
    <div class="flex items-center gap-3">
      <div
        class="w-12 h-12 rounded-xl flex items-center justify-center text-2xl shrink-0"
        :style="quiz.coverGradient"
      >
        {{ props.quiz.coverEmoji }}
      </div>
      <div class="min-w-0 flex-1">
        <p class="text-sm font-bold truncate block hover:underline">
          {{ quiz.title }}
        </p>

        <p class="text-xs text-neutral-400 mt-0.5">
          {{ t('myQuizzes.card.questions', { count: quiz.questionCount }) }}
        </p>
      </div>
      <span
        class="text-[11px] px-2 py-1 rounded-full font-semibold whitespace-nowrap shrink-0"
        :class="statusBadgeClass"
      >
        {{ statusLabel }}
      </span>
    </div>

    <div class="flex justify-end">
      <UDropdownMenu
        :items="cardActions(quiz)"
        :content="{ align: 'end' }"
      >
        <UButton
          icon="i-lucide-more-horizontal"
          size="xs"
          color="neutral"
          variant="ghost"
          class="rounded-full"
          :loading="props.isLoading"
        />
      </UDropdownMenu>
    </div>
  </div>
</template>

<script lang="ts" setup>
import type { DropdownMenuItem } from '@nuxt/ui'
import type { Quiz } from '~/types/quiz'

interface Props {
  quiz: Quiz
  isLoading: boolean
}

interface Emits {
  (e: 'publish' | 'unpublish' | 'archive' | 'delete', quizId: string): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const { t } = useI18n()

const statusLabel = computed(() => ({
  Published: t('myQuizzes.status.published'),
  Draft: t('myQuizzes.status.draft'),
  Archived: t('myQuizzes.status.archived')
}[props.quiz.status] ?? props.quiz.status))

const statusBadgeClass = computed(() => ({
  Published: 'bg-success-100 text-success-800',
  Draft: 'bg-neutral-100 text-neutral-600',
  Archived: 'bg-warning-100 text-warning-800'
}[props.quiz.status] ?? 'bg-neutral-100 text-neutral-600'))

const cardActions = (quiz: Quiz): DropdownMenuItem[] => ([
  {
    label: t('myQuizzes.actions.edit'),
    icon: 'i-lucide-pencil',
    to: `/library/mine/${quiz.id}/edit`
  },
  quiz.status !== 'Published' && {
    label: t('myQuizzes.actions.publish'),
    icon: 'i-lucide-upload',
    onSelect: () => emit('publish', quiz.id)
  },
  quiz.status === 'Published' && {
    label: t('myQuizzes.actions.unpublish'),
    icon: 'i-lucide-eye-off',
    onSelect: () => emit('unpublish', quiz.id)
  },
  quiz.status !== 'Archived' && {
    label: t('myQuizzes.actions.archive'),
    icon: 'i-lucide-archive',
    onSelect: () => emit('archive', quiz.id)
  },
  {
    label: t('myQuizzes.actions.delete'),
    icon: 'i-lucide-trash-2',
    color: 'error',
    onSelect: () => emit('delete', quiz.id)
  }
] as (DropdownMenuItem | false)[]).filter((item): item is DropdownMenuItem => !!item)
</script>

<script lang="ts">
export default {
  name: 'MineQuizCard'
}
</script>

<style scoped lang="scss"></style>
