<template>
  <div class="rounded-2xl bg-white border border-neutral-200 p-6">
    <p class="text-lg font-extrabold mb-3">
      {{ t('adminDashboard.topQuizzes.title') }}
    </p>
    <p
      v-if="quizzes.length === 0"
      data-testid="top-quiz-empty"
      class="text-base text-neutral-500"
    >
      {{ t('adminDashboard.topQuizzes.empty') }}
    </p>
    <ul
      v-else
      class="divide-y divide-neutral-100"
    >
      <li
        v-for="(quiz, index) in quizzes"
        :key="quiz.id"
        class="flex items-center gap-3 py-3"
      >
        <span
          data-testid="top-quiz-position"
          class="w-6 text-base font-bold text-neutral-400 text-center shrink-0"
        >
          {{ index + 1 }}
        </span>
        <QuizCoverThumbnail
          :cover-image-url="quiz.coverImageUrl"
          :cover-emoji="quiz.coverEmoji"
          :cover-gradient="quiz.coverGradient"
          size="xs"
          rounded="lg"
        />
        <span class="flex-1 min-w-0">
          <span class="block text-base font-bold truncate">{{ quiz.title }}</span>
          <span class="block text-xs text-neutral-500 truncate">{{ quiz.creatorName }}</span>
        </span>
        <span class="text-base font-bold text-primary-500 whitespace-nowrap">
          {{ t('adminDashboard.topQuizzes.challengeCount', { count: quiz.challengeCount }) }}
        </span>
      </li>
    </ul>
  </div>
</template>

<script setup lang="ts">
import QuizCoverThumbnail from '~/components/library/QuizCoverThumbnail.vue'
import type { QuizSummaryDto } from '~/types/api'

interface Props {
  /** 已依挑戰次數由高到低排序 */
  quizzes: QuizSummaryDto[]
}

defineProps<Props>()

const { t } = useI18n()
</script>

<script lang="ts">
export default {
  name: 'AdminTopQuizList'
}
</script>

<style scoped lang="scss"></style>
