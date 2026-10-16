<template>
  <div class="bg-cp-surface rounded-2xl overflow-hidden border border-cp-border hover:shadow-cp-md hover:border-transparent transition-all p-4 flex flex-col gap-3">
    <div class="flex items-center gap-3">
      <QuizCoverThumbnail
        :cover-image-url="quiz.coverImageUrl"
        :cover-emoji="quiz.coverEmoji"
        :cover-gradient="quiz.coverGradient"
        size="sm"
      />
      <div class="min-w-0 flex-1">
        <p class="text-sm font-bold truncate block hover:underline text-cp-text-primary">
          {{ quiz.title }}
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
import QuizCoverThumbnail from '~/components/library/QuizCoverThumbnail.vue'
import type { Quiz } from '~/types/quiz'

interface Props {
  quiz: Quiz
  isLoading: boolean
}

interface Emits {
  (e: 'publish' | 'unpublish' | 'archive' | 'unarchive' | 'delete', quizId: string): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const { t } = useI18n()

const statusLabel = computed(() => ({
  published: t('myQuizzes.status.published'),
  draft: t('myQuizzes.status.draft'),
  archived: t('myQuizzes.status.archived'),
  takendown: t('myQuizzes.status.takenDown')
}[props.quiz.status] ?? props.quiz.status))

const statusBadgeClass = computed(() => ({
  published: 'bg-success-100 text-success-800',
  draft: 'bg-neutral-100 text-neutral-600',
  archived: 'bg-warning-100 text-warning-800',
  takendown: 'bg-error-100 text-error-800'
}[props.quiz.status] ?? 'bg-neutral-100 text-neutral-600'))

const cardActions = (quiz: Quiz): DropdownMenuItem[] => ([
  {
    label: t('myQuizzes.actions.edit'),
    icon: 'i-lucide-pencil',
    to: `/library/mine/${quiz.id}/edit`
  },
  quiz.status !== 'published' && {
    label: t('myQuizzes.actions.publish'),
    icon: 'i-lucide-upload',
    onSelect: () => emit('publish', quiz.id)
  },
  quiz.status === 'published' && {
    label: t('myQuizzes.actions.unpublish'),
    icon: 'i-lucide-eye-off',
    onSelect: () => emit('unpublish', quiz.id)
  },
  quiz.status !== 'archived' && quiz.status !== 'takendown' && {
    label: t('myQuizzes.actions.archive'),
    icon: 'i-lucide-archive',
    onSelect: () => emit('archive', quiz.id)
  },
  quiz.status === 'archived' && {
    label: t('myQuizzes.actions.unarchive'),
    icon: 'i-lucide-archive-restore',
    onSelect: () => emit('unarchive', quiz.id)
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
