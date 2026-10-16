<template>
  <div class="rounded-t-2xl px-10 pt-10 pb-8 flex gap-8 items-end flex-wrap bg-cp-secondary">
    <QuizCoverThumbnail
      :cover-image-url="quiz.coverImageUrl"
      :cover-emoji="quiz.coverEmoji"
      :cover-gradient="quiz.coverGradient"
      size="xl"
    />

    <div class="flex-1 min-w-60 flex flex-col gap-2.5 pb-1">
      <p
        data-testid="hero-eyebrow"
        class="text-xs font-semibold text-white"
      >
        {{ eyebrow }}
      </p>
      <h1 class="text-4xl font-extrabold text-white leading-tight">
        {{ quiz.title }}
      </h1>
      <p
        data-testid="hero-meta"
        class="text-[13px] text-white/60"
      >
        {{ t('libraryDetail.creatorLine', { name: quiz.creatorName }) }} · {{ t('libraryDetail.questions', { count: quiz.questionCount }) }} · {{ t('libraryDetail.challenges', { count: quiz.challengeCount.toLocaleString() }) }}
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
import QuizCoverThumbnail from '~/components/library/QuizCoverThumbnail.vue'
import { DIFFICULTY_LABEL, type Quiz } from '~/types/quiz'

interface Props {
  quiz: Quiz
}

const props = defineProps<Props>()

const { t } = useI18n()

const eyebrow = computed(() =>
  [DIFFICULTY_LABEL[props.quiz.difficulty], props.quiz.tags[0]].filter(Boolean).join(' · ')
)
</script>

<script lang="ts">
export default {
  name: 'QuizDetailHero'
}
</script>

<style scoped lang="scss"></style>
