<template>
  <section
    v-if="questions.length"
    class="rounded-2xl bg-white overflow-hidden border border-cp-border p-5"
  >
    <h2 class="text-sm font-bold mb-4">
      {{ t('history.detail.breakdown.title') }}
    </h2>
    <div class="flex flex-col gap-4">
      <div
        v-for="(question, qi) in questions"
        :key="qi"
        class="rounded-xl border border-cp-border p-4"
      >
        <div class="flex items-start justify-between gap-3 mb-3">
          <p class="text-sm font-semibold leading-relaxed">
            {{ qi + 1 }}. {{ question.questionText }}
          </p>
          <div class="shrink-0 flex flex-col items-end gap-1">
            <StatusBadge
              data-testid="correct-rate"
              tone="primary"
            >
              {{ t('history.detail.breakdown.correctRate', { rate: percentOf(question.correctCount, question) }) }}
            </StatusBadge>
            <span
              v-if="question.averageAnswerTimeMs !== null"
              data-testid="average-time"
              class="text-xs text-cp-text-muted whitespace-nowrap"
            >
              {{ t('history.detail.breakdown.averageAnswerTime', { seconds: (question.averageAnswerTimeMs / 1000).toFixed(1) }) }}
            </span>
          </div>
        </div>

        <p
          v-if="!question.answeredCount"
          data-testid="no-answers"
          class="text-xs text-cp-text-muted"
        >
          {{ t('history.detail.breakdown.noAnswers') }}
        </p>
        <div
          v-else
          class="flex flex-col gap-2"
        >
          <div
            v-for="(option, oi) in question.options"
            :key="oi"
            data-testid="breakdown-option"
            class="relative rounded-lg border overflow-hidden px-3 py-2"
            :class="option.isCorrect ? 'border-cp-success' : 'border-cp-border'"
          >
            <div
              class="absolute inset-y-0 left-0"
              :class="option.isCorrect ? 'bg-cp-success-bg' : 'bg-cp-surface-muted'"
              :style="{ width: `${percentOf(option.pickedCount, question)}%` }"
            />
            <div class="relative flex items-center justify-between gap-3 text-xs">
              <span :class="option.isCorrect ? 'font-semibold text-cp-success' : 'text-cp-text-secondary'">
                {{ option.text }}
              </span>
              <span
                data-testid="option-stat"
                class="shrink-0 tabular-nums text-cp-text-muted"
              >
                {{ option.pickedCount }} · {{ percentOf(option.pickedCount, question) }}%
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </section>
</template>

<script setup lang="ts">
import StatusBadge from '~/components/common/StatusBadge.vue'
import type { GameSessionQuestionBreakdownDto } from '~/types/api'

interface Props {
  questions: GameSessionQuestionBreakdownDto[]
}

defineProps<Props>()

const { t } = useI18n()

const percentOf = (count: number, question: GameSessionQuestionBreakdownDto) =>
  question.answeredCount ? Math.round((count / question.answeredCount) * 100) : 0
</script>

<script lang="ts">
export default {
  name: 'QuestionBreakdownList'
}
</script>

<style scoped lang="scss"></style>
