<template>
  <div class="flex flex-col gap-4 mt-4">
    <div
      v-for="(question, index) in questions"
      :key="question.id"
      class="rounded-2xl bg-white border border-neutral-200 p-4"
    >
      <div class="flex items-start gap-3">
        <span class="w-6 h-6 rounded-full bg-primary-500 text-white text-xs font-bold flex items-center justify-center shrink-0">
          {{ index + 1 }}
        </span>
        <div class="min-w-0 flex-1">
          <p class="text-lg font-bold">
            {{ question.text }}
          </p>
          <p
            v-if="question.explanation"
            class="text-base text-neutral-400 mt-1"
          >
            {{ question.explanation }}
          </p>
        </div>
      </div>
      <ul class="grid grid-cols-2 gap-1.5 mt-3">
        <li
          v-for="(option, optionIndex) in question.options"
          :key="optionIndex"
          class="text-base px-3 py-1.5 rounded-lg"
          :class="optionIndex === question.answerIndex
            ? 'bg-success-100 text-success-800 font-semibold'
            : 'bg-neutral-100 text-neutral-600'"
        >
          {{ option }}
        </li>
      </ul>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { QuestionDto } from '~/types/api'

interface Props {
  questions: QuestionDto[]
}

defineProps<Props>()
</script>

<script lang="ts">
export default {
  name: 'AdminQuizQuestionList'
}
</script>

<style scoped lang="scss"></style>
