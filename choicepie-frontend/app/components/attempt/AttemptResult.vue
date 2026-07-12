<template>
  <div>
    <div
      class="rounded-2xl p-6 mb-6 text-center border-2"
      :class="result.passed ? 'bg-success-50 border-success-500' : 'bg-error-50 border-error-500'"
    >
      <div class="text-4xl mb-2">
        {{ result.passed ? '🎉' : '😅' }}
      </div>
      <p
        class="text-2xl font-black mb-1"
        :class="result.passed ? 'text-success-500' : 'text-error-500'"
      >
        {{ t('attempt.result.score', { score: result.score ?? 0 }) }}
      </p>
      <p class="text-sm text-neutral-600">
        {{ result.quizTitle }}
      </p>
    </div>

    <p class="text-sm font-bold mb-3">
      {{ t('attempt.result.reviewTitle') }}
    </p>

    <div class="flex flex-col gap-4 mb-8">
      <div
        v-for="(answer, i) in result.answers"
        :key="answer.questionId"
        class="rounded-2xl p-4 border"
        :class="answer.isCorrect ? 'border-success-200 bg-success-50' : 'border-error-200 bg-error-50'"
      >
        <p class="text-sm font-semibold mb-2">
          {{ t('attempt.result.questionIndex', { index: i + 1 }) }}：{{ answer.questionText }}
        </p>
        <p
          class="text-xs font-bold mb-1"
          :class="answer.isCorrect ? 'text-success-500' : 'text-error-500'"
        >
          {{ answer.isCorrect ? t('attempt.result.correct') : t('attempt.result.wrong') }}
        </p>
        <p
          v-if="answer.explanation"
          class="text-xs text-neutral-600 leading-relaxed"
        >
          {{ answer.explanation }}
        </p>
      </div>
    </div>

    <div class="flex gap-3">
      <NuxtLink
        :to="`/library/${result.quizId}`"
        class="flex-1"
      >
        <UButton
          block
          size="lg"
          color="neutral"
          variant="ghost"
          class="rounded-xl font-bold"
        >
          {{ t('attempt.result.backToQuiz') }}
        </UButton>
      </NuxtLink>
      <NuxtLink
        to="/library"
        class="flex-1"
      >
        <UButton
          block
          size="lg"
          color="primary"
          class="rounded-xl font-bold"
        >
          {{ t('attempt.result.backToLibrary') }}
        </UButton>
      </NuxtLink>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { QuizAttemptResultDto } from '~/types/api'

defineProps<{ result: QuizAttemptResultDto }>()

const { t } = useI18n()
</script>

<style scoped lang="scss"></style>
