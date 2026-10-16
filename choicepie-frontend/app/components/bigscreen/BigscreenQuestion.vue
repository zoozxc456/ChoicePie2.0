<template>
  <div>
    <div
      v-if="gameStore.isPaused"
      data-testid="paused-overlay"
      class="fixed inset-0 z-50 flex flex-col items-center justify-center gap-4 bg-black/75 text-white"
      role="status"
    >
      <p class="text-5xl font-black">
        {{ t('bigscreen.playing.paused') }}
      </p>
    </div>

    <!-- Question header -->
    <div class="flex items-center justify-between mb-6">
      <span
        data-testid="question-progress"
        class="text-lg font-semibold text-white/50"
      >
        {{ t('bigscreen.playing.questionOf', { current: (gameStore.currentQuestion?.index ?? 0) + 1, total: gameStore.currentQuestion?.total }) }}
      </span>
      <div
        data-testid="timer"
        class="w-20 h-20 rounded-full flex items-center justify-center text-4xl font-black border-4 tabular-nums"
        :class="gameStore.isTimerUrgent
          ? 'border-cp-danger text-cp-danger timer--urgent'
          : 'border-cp-primary text-cp-primary'"
      >
        {{ gameStore.timeLeft }}
      </div>
    </div>

    <!-- Timer bar -->
    <div class="rounded-full overflow-hidden mb-8 h-2 bg-white/10">
      <div
        class="h-full rounded-full transition-[width] duration-1000 ease-linear"
        :class="gameStore.isTimerUrgent ? 'bg-cp-danger' : 'bg-cp-primary'"
        :style="{ width: `${gameStore.timerPercent}%` }"
      />
    </div>

    <p
      data-testid="question-text"
      class="text-3xl font-bold text-white text-center mb-10 leading-relaxed quiz-text"
    >
      {{ gameStore.currentQuestion?.text }}
    </p>

    <BigscreenOptionGrid
      class="mb-8"
      :options="gameStore.currentQuestion?.options ?? []"
      :correct-answer-index="gameStore.correctAnswerIndex"
    />

    <!-- Answer progress -->
    <div class="flex items-center gap-4">
      <span class="text-sm text-white/50">
        {{ t('bigscreen.playing.answeredOf', { answered: gameStore.answeredCount, total: gameStore.totalCount }) }}
      </span>
      <div class="flex-1 rounded-full overflow-hidden h-1.5 bg-white/10">
        <div
          data-testid="answer-progress-bar"
          class="h-full rounded-full transition-all duration-500 bg-cp-primary"
          :style="{ width: `${answeredPercent}%` }"
        />
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import BigscreenOptionGrid from '~/components/bigscreen/BigscreenOptionGrid.vue'

const { t } = useI18n()
const gameStore = useGameStore()

const answeredPercent = computed(() =>
  gameStore.totalCount > 0 ? (gameStore.answeredCount / gameStore.totalCount) * 100 : 0
)
</script>

<script lang="ts">
export default {
  name: 'BigscreenQuestion'
}
</script>

<style scoped lang="scss">
@keyframes pulse {
  from { opacity: 1; }
  to { opacity: 0.5; }
}
.timer--urgent {
  animation: pulse 0.5s ease-in-out infinite alternate;
}
</style>
