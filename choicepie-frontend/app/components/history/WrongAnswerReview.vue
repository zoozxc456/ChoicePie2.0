<template>
  <section class="rounded-2xl bg-white border border-cp-border p-5">
    <h2 class="text-sm font-bold mb-1">
      {{ t('history.detail.review.title') }}
    </h2>
    <p
      data-testid="review-subtitle"
      class="text-xs text-cp-text-muted mb-4"
    >
      {{ wrongAnswers.length
        ? t('history.detail.review.subtitle', { count: wrongAnswers.length })
        : t('history.detail.review.empty') }}
    </p>

    <div class="flex flex-col gap-4">
      <div
        v-for="(qa, i) in wrongAnswers"
        :key="i"
        data-testid="review-item"
        class="rounded-xl border border-cp-border p-4"
      >
        <p class="text-sm font-semibold mb-3 leading-relaxed">
          {{ i + 1 }}. {{ qa.questionText }}
        </p>
        <div class="flex flex-col gap-1.5 mb-3">
          <div
            v-for="(option, oi) in qa.options"
            :key="oi"
            data-testid="review-option"
            class="text-xs rounded-lg px-3 py-2 border"
            :class="optionClass(qa, oi)"
          >
            {{ option }}
            <span
              v-if="oi === qa.myAnswerIndex"
              data-testid="my-answer-tag"
              class="font-semibold"
            > — {{ t('history.detail.review.yourAnswer') }}</span>
            <span
              v-if="oi === qa.correctAnswerIndex"
              data-testid="correct-answer-tag"
              class="font-semibold"
            > — {{ t('history.detail.review.correctAnswer') }}</span>
          </div>
        </div>
        <div
          data-testid="review-explanation"
          class="rounded-lg bg-cp-surface-muted px-3 py-2.5 text-xs leading-relaxed text-cp-text-secondary"
        >
          <span class="font-semibold text-cp-text-primary">{{ t('history.detail.review.explanation') }}：</span>
          {{ qa.explanation }}
        </div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import type { GameSessionWrongAnswerDto } from '~/types/api'

interface Props {
  wrongAnswers: GameSessionWrongAnswerDto[]
}

defineProps<Props>()

const { t } = useI18n()

const optionClass = (qa: GameSessionWrongAnswerDto, optionIndex: number) => {
  if (optionIndex === qa.correctAnswerIndex) return 'border-cp-success bg-cp-success-bg text-cp-success'
  if (optionIndex === qa.myAnswerIndex) return 'border-cp-danger bg-cp-danger-bg text-cp-danger'
  return 'border-cp-border text-cp-text-secondary'
}
</script>

<script lang="ts">
export default {
  name: 'WrongAnswerReview'
}
</script>

<style scoped lang="scss"></style>
