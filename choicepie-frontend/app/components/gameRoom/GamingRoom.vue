<template>
  <!-- Progress + Timer -->
  <div class="flex items-center justify-between mb-3">
    <span class="text-sm font-semibold text-neutral-600">
      {{ t('room.question.questionOf', {
        current: (gameStore.currentQuestion?.index ?? 0) + 1,
        total:
          gameStore.currentQuestion?.total
      }) }}
    </span>
    <span
      class="text-2xl font-black tabular-nums w-14 text-right"
      :class="gameStore.isTimerUrgent ? 'text-error-500' : 'text-primary-500'"
    >
      {{ gameStore.timeLeft }}
    </span>
  </div>

  <!-- Timer bar -->
  <div class="rounded-full overflow-hidden mb-5 h-1.5 bg-neutral-100">
    <div
      class="h-full rounded-full transition-[width] duration-1000 ease-linear"
      :class="gameStore.isTimerUrgent ? 'bg-error-500 animate-pulse' : 'bg-primary-500'"
      :style="`width: ${gameStore.timerPercent}%;`"
    />
  </div>

  <!-- Question -->
  <p class="text-base font-semibold leading-relaxed mb-5 quiz-text">
    {{ gameStore.currentQuestion?.text }}
  </p>

  <!-- Options -->
  <div class="flex flex-col gap-3">
    <QuizOption
      v-for="(opt, i) in gameStore.currentQuestion?.options"
      :key="i"
      :letter="optionLetters[i] ?? ''"
      :text="opt"
      :state="optionState(i)"
      :disabled="gameStore.selectedAnswerIndex !== null"
      @click="handleAnswer(i)"
    />
  </div>

  <!-- Answered state -->
  <div
    v-if="gameStore.selectedAnswerIndex !== null && gameStore.correctAnswerIndex === null"
    class="text-center mt-5 text-sm font-semibold text-neutral-600"
  >
    {{ t('room.question.answered') }}
  </div>
</template>

<script lang="ts" setup>
import QuizOption from '~/components/gameRoom/QuizOption.vue'
import type { QuizOptionState } from '~/components/gameRoom/QuizOption.vue'

interface Props {
  code: string
}

const props = defineProps<Props>()
const { t } = useI18n()
const gameStore = useGameStore()
const gameRoom = useGameRoom()
const optionLetters = ['A', 'B', 'C', 'D']

const handleAnswer = async (index: number) => {
  if (gameStore.selectedAnswerIndex !== null) return
  gameStore.selectAnswer(index)
  await gameRoom.submitAnswer(props.code, index)
}

const optionState = (index: number): QuizOptionState => {
  const selected = gameStore.selectedAnswerIndex
  const correct = gameStore.correctAnswerIndex

  if (selected !== null && correct !== null) {
    if (index === correct) return 'correct'
    if (index === selected && index !== correct) return 'wrong'
    return 'disabled'
  }

  if (selected === index) return 'selected'

  return 'default'
}
</script>

<style scoped lang="scss">
</style>
