<template>
  <section
    v-if="quizzes.length"
    class="bg-cp-surface rounded-2xl border border-cp-border p-5"
  >
    <h2 class="text-base font-bold mb-3 text-cp-text-primary">
      {{ t('libraryDetail.related.title') }}
    </h2>
    <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
      <NuxtLink
        v-for="quiz in quizzes"
        :key="quiz.id"
        data-testid="related-quiz"
        :to="`/library/${quiz.id}`"
        class="flex items-center gap-3 p-3 rounded-xl hover:bg-cp-surface-muted transition-colors"
      >
        <QuizCoverThumbnail
          :cover-image-url="quiz.coverImageUrl"
          :cover-emoji="quiz.coverEmoji"
          :cover-gradient="quiz.coverGradient"
          size="sm"
          rounded="lg"
        />
        <div class="flex-1 min-w-0">
          <p class="text-sm font-semibold truncate text-cp-text-primary">
            {{ quiz.title }}
          </p>
          <p class="text-xs text-cp-text-muted">
            {{ t('libraryDetail.questions', { count: quiz.questionCount }) }} · {{ t('libraryDetail.challenges', { count: quiz.challengeCount.toLocaleString() }) }}
          </p>
        </div>
      </NuxtLink>
    </div>
  </section>
</template>

<script setup lang="ts">
import QuizCoverThumbnail from '~/components/library/QuizCoverThumbnail.vue'
import type { Quiz } from '~/types/quiz'

interface Props {
  quizzes: Quiz[]
}

defineProps<Props>()

const { t } = useI18n()
</script>

<script lang="ts">
export default {
  name: 'RelatedQuizList'
}
</script>

<style scoped lang="scss"></style>
