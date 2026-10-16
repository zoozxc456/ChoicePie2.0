<template>
  <section class="bg-cp-surface rounded-2xl border border-cp-border p-5">
    <h2 class="text-base font-bold mb-3 text-cp-text-primary">
      {{ t('libraryDetail.questionList') }}
    </h2>
    <ol class="flex flex-col">
      <li
        v-for="(question, i) in questions"
        :key="question.id"
        data-testid="question-item"
        class="flex items-center gap-4 px-3 py-2.5 rounded-xl hover:bg-cp-surface-muted transition-colors"
      >
        <span
          data-testid="question-number"
          class="w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold shrink-0 bg-cp-primary-light text-cp-primary"
        >
          {{ i + 1 }}
        </span>
        <span class="flex-1 text-sm font-medium text-cp-text-primary">
          {{ question.text }}
        </span>
      </li>
    </ol>
  </section>
</template>

<script setup lang="ts">
import type { Question } from '~/types/quiz'

interface Props {
  questions: Question[]
}

defineProps<Props>()

const { t } = useI18n()
</script>

<script lang="ts">
export default {
  name: 'QuizQuestionList'
}
</script>

<style scoped lang="scss"></style>
