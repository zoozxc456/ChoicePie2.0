<template>
  <div class="flex items-center gap-3 px-8 py-5 bg-cp-surface rounded-b-2xl flex-wrap border border-t-0 border-cp-border">
    <button
      data-testid="action-start"
      type="button"
      :aria-label="t('libraryDetail.modal.title')"
      class="w-11 h-11 rounded-full flex items-center justify-center text-lg text-white shrink-0 bg-cp-primary cursor-pointer transition-transform hover:scale-110"
      @click="emit('start')"
    >
      <UIcon name="i-lucide-play" />
    </button>

    <button
      v-if="isOwner"
      data-testid="action-solo"
      type="button"
      :class="PILL_CLASS"
      class="bg-cp-surface-muted"
      :disabled="isStartingAttempt"
      @click="emit('soloPractice')"
    >
      {{ t('libraryDetail.soloPractice') }}
    </button>

    <button
      data-testid="action-favorite"
      type="button"
      :aria-pressed="isFavorited"
      :class="[PILL_CLASS, isFavorited ? 'bg-error-100 text-error-800' : 'bg-cp-surface-muted']"
      :disabled="isTogglingFavorite"
      @click="emit('toggleFavorite')"
    >
      <UIcon
        name="i-lucide-heart"
        :class="{ 'fill-current': isFavorited }"
      />
      {{ isFavorited ? t('libraryDetail.favorite.remove') : t('libraryDetail.favorite.add') }}
    </button>

    <template v-if="isOwner">
      <button
        v-if="isPublished"
        data-testid="action-unpublish"
        type="button"
        :class="PILL_CLASS"
        class="bg-cp-surface-muted"
        :disabled="isTogglingStatus"
        @click="emit('unpublish')"
      >
        <UIcon name="i-lucide-eye-off" />
        {{ t('libraryDetail.status.unpublishAction') }}
      </button>
      <button
        v-else
        data-testid="action-publish"
        type="button"
        :class="PILL_CLASS"
        class="text-white bg-cp-primary"
        :disabled="isTogglingStatus"
        @click="emit('publish')"
      >
        <UIcon name="i-lucide-upload" />
        {{ t('libraryDetail.status.publishAction') }}
      </button>
    </template>

    <ShareMenu
      :quiz-id="quiz.id"
      :quiz-title="quiz.title"
    />

    <button
      v-if="!isOwner"
      data-testid="action-report"
      type="button"
      :class="PILL_CLASS"
      class="bg-cp-surface-muted"
      :disabled="hasReported"
      @click="emit('report')"
    >
      {{ hasReported ? t('libraryDetail.report.reported') : t('libraryDetail.report.action') }}
    </button>

    <div class="ml-auto flex gap-2 items-center">
      <StatusBadge
        v-if="isOwner"
        data-testid="badge-status"
        :tone="quizStatusTone(quiz.status)"
      >
        {{ statusLabel }}
      </StatusBadge>
      <StatusBadge
        data-testid="badge-pass-rate"
        tone="success"
      >
        {{ t('libraryDetail.passRateBadge', { rate: quiz.passRate }) }}
      </StatusBadge>
      <StatusBadge :tone="difficultyTone(quiz.difficulty)">
        {{ DIFFICULTY_LABEL[quiz.difficulty] }}
      </StatusBadge>
    </div>
  </div>
</template>

<script setup lang="ts">
import StatusBadge from '~/components/common/StatusBadge.vue'
import ShareMenu from '~/components/library/ShareMenu.vue'
import { DIFFICULTY_LABEL, type Quiz } from '~/types/quiz'

interface Props {
  quiz: Quiz
  isOwner: boolean
  isFavorited: boolean
  isTogglingFavorite?: boolean
  hasReported?: boolean
  isStartingAttempt?: boolean
  isTogglingStatus?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  isTogglingFavorite: false,
  hasReported: false,
  isStartingAttempt: false,
  isTogglingStatus: false
})

const emit = defineEmits<{
  start: []
  soloPractice: []
  toggleFavorite: []
  publish: []
  unpublish: []
  report: []
}>()

const { t } = useI18n()

const PILL_CLASS = 'h-10 px-4 rounded-full text-[13px] font-semibold whitespace-nowrap cursor-pointer disabled:opacity-60 inline-flex items-center gap-1.5'

const isPublished = computed(() => props.quiz.status === 'published')

const statusLabel = computed(() => ({
  published: t('libraryDetail.status.published'),
  draft: t('libraryDetail.status.draft'),
  archived: t('libraryDetail.status.archived')
}[props.quiz.status] ?? props.quiz.status))
</script>

<script lang="ts">
export default {
  name: 'QuizDetailActionBar'
}
</script>

<style scoped lang="scss"></style>
