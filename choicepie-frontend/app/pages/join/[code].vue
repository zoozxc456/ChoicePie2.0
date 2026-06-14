<template>
  <div class="max-w-md mx-auto px-4 py-6">
    <!-- ─── WAITING ─── -->
    <template v-if="gameStore.phase === 'waiting'">
      <div class="text-center mb-6">
        <div class="text-5xl mb-3">
          🥧
        </div>
        <h1 class="text-xl font-bold mb-1">
          {{ t('room.waiting.title') }}
        </h1>
        <p
          class="text-sm"
          style="color: var(--cp-text-secondary);"
        >
          {{ t('room.waiting.subtitle') }}
        </p>
      </div>

      <div
        class="rounded-2xl p-5 bg-white mb-4"
        style="border: 1px solid var(--cp-border);"
      >
        <div class="text-center mb-4">
          <p
            class="text-xs font-semibold mb-1"
            style="color: var(--cp-text-muted);"
          >
            {{ t('room.waiting.yourNickname') }}
          </p>
          <p
            class="text-2xl font-bold"
            style="color: var(--cp-primary);"
          >
            {{ gameStore.myNickname }}
          </p>
        </div>
        <div class="text-center mb-4">
          <p
            class="text-xs font-semibold mb-1"
            style="color: var(--cp-text-muted);"
          >
            {{ t('room.waiting.roomCode') }}
          </p>
          <p
            class="text-xl font-bold tracking-widest"
            style="color: var(--cp-secondary);"
          >
            {{ code }}
          </p>
        </div>

        <!-- Player list -->
        <p
          class="text-xs font-semibold mb-2"
          style="color: var(--cp-text-muted);"
        >
          {{ t('room.waiting.playerCount', { count: gameStore.playerCount }) }}
        </p>
        <div class="flex flex-wrap gap-2">
          <div
            v-for="player in gameStore.players"
            :key="player.connectionId"
            class="player-chip"
            :style="player.nickname === gameStore.myNickname
              ? 'background: var(--cp-primary-light); border-color: var(--cp-primary-border);'
              : ''"
          >
            <div class="online-dot" />
            <span :style="player.nickname === gameStore.myNickname ? 'color: var(--cp-primary); font-weight: 600;' : ''">
              {{ player.nickname }}
            </span>
          </div>
        </div>
      </div>

      <div
        class="flex items-center justify-center gap-2 text-sm"
        style="color: var(--cp-text-muted);"
      >
        <UIcon
          name="i-lucide-loader-2"
          class="animate-spin"
        />
        {{ t('room.waiting.waitingHost') }}
      </div>
    </template>

    <!-- ─── QUESTION ─── -->
    <template v-else-if="gameStore.phase === 'question'">
      <!-- Progress + Timer -->
      <div class="flex items-center justify-between mb-3">
        <span
          class="text-sm font-semibold"
          style="color: var(--cp-text-secondary);"
        >
          {{ t('room.question.questionOf', { current: (gameStore.currentQuestion?.index ?? 0) + 1, total: gameStore.currentQuestion?.total }) }}
        </span>
        <span
          class="text-2xl font-black tabular-nums w-14 text-right"
          :style="`color: ${gameStore.isTimerUrgent ? 'var(--cp-danger)' : 'var(--cp-primary)'};`"
        >
          {{ gameStore.timeLeft }}
        </span>
      </div>

      <!-- Timer bar -->
      <div
        class="rounded-full overflow-hidden mb-5"
        style="height: 6px; background: var(--cp-surface-muted);"
      >
        <div
          class="h-full rounded-full"
          :class="{ 'animate-pulse': gameStore.isTimerUrgent }"
          :style="`
            width: ${gameStore.timerPercent}%;
            background: ${gameStore.isTimerUrgent ? 'var(--cp-danger)' : 'var(--cp-primary)'};
            transition: width 1s linear;
          `"
        />
      </div>

      <!-- Question -->
      <p class="text-base font-semibold leading-relaxed mb-5 quiz-text">
        {{ gameStore.currentQuestion?.text }}
      </p>

      <!-- Options -->
      <div class="flex flex-col gap-3">
        <button
          v-for="(opt, i) in gameStore.currentQuestion?.options"
          :key="i"
          class="flex items-center gap-3 p-4 rounded-2xl text-left transition-all w-full option-card"
          :style="optionStyle(i)"
          :disabled="gameStore.selectedAnswerIndex !== null"
          @click="handleAnswer(i)"
        >
          <span
            class="w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold shrink-0"
            style="background: var(--cp-surface-muted);"
          >
            {{ optionLetters[i] }}
          </span>
          <span
            class="text-sm font-medium"
            :style="optionTextStyle(i)"
          >{{ opt }}</span>
        </button>
      </div>

      <!-- Answered state -->
      <div
        v-if="gameStore.selectedAnswerIndex !== null && gameStore.correctAnswerIndex === null"
        class="text-center mt-5 text-sm font-semibold"
        style="color: var(--cp-text-secondary);"
      >
        {{ t('room.question.answered') }}
      </div>
    </template>

    <!-- ─── RESULT ─── -->
    <template v-else-if="gameStore.phase === 'result'">
      <!-- Answer feedback -->
      <div
        class="rounded-2xl p-5 mb-4 text-center"
        :style="gameStore.isCorrect
          ? 'background: var(--cp-success-bg); border: 2px solid var(--cp-success);'
          : 'background: var(--cp-danger-bg); border: 2px solid var(--cp-danger);'"
      >
        <div class="text-4xl mb-2">
          {{ gameStore.isCorrect ? '🎉' : '😅' }}
        </div>
        <p
          class="text-xl font-black mb-1"
          :style="`color: ${gameStore.isCorrect ? 'var(--cp-success)' : 'var(--cp-danger)'};`"
        >
          {{ gameStore.isCorrect ? t('room.result.correct') : t('room.result.wrong') }}
        </p>
        <p
          v-if="gameStore.isCorrect && gameStore.pointsEarned > 0"
          class="font-bold text-lg"
          style="color: var(--cp-primary);"
        >
          {{ t('room.result.points', { points: gameStore.pointsEarned.toLocaleString() }) }}
        </p>
      </div>

      <!-- Explanation -->
      <div
        v-if="gameStore.currentExplanation"
        class="rounded-2xl p-4 mb-4 explanation-text"
        style="background: var(--cp-surface-muted); border: 1px solid var(--cp-border); font-size: 14px; line-height: 1.7;"
      >
        <p
          class="text-xs font-semibold mb-2"
          style="color: var(--cp-text-muted);"
        >
          {{ t('room.result.explanation') }}
        </p>
        {{ gameStore.currentExplanation }}
      </div>

      <!-- Current rank -->
      <div
        class="rounded-2xl p-4 bg-white text-center"
        style="border: 1px solid var(--cp-border);"
      >
        <p
          class="text-xs font-semibold mb-1"
          style="color: var(--cp-text-muted);"
        >
          {{ t('room.result.currentRank') }}
        </p>
        <p
          class="text-3xl font-black"
          style="color: var(--cp-primary);"
        >
          {{ t('room.result.rank', { rank: gameStore.myRank }) }}
        </p>
        <p
          class="font-bold"
          style="color: var(--cp-text-secondary);"
        >
          {{ gameStore.myScore.toLocaleString() }}
        </p>
      </div>

      <p
        class="text-center text-sm mt-4"
        style="color: var(--cp-text-muted);"
      >
        {{ t('room.result.waitingNext') }}
      </p>
    </template>

    <!-- ─── ENDED ─── -->
    <template v-else-if="gameStore.phase === 'ended'">
      <div class="text-center mb-6">
        <div class="text-5xl mb-3">
          🏆
        </div>
        <h1 class="text-2xl font-bold mb-1">
          {{ t('room.ended.title') }}
        </h1>
      </div>

      <!-- My result -->
      <div
        class="rounded-2xl p-5 mb-5 text-center"
        style="background: var(--cp-primary-light); border: 2px solid var(--cp-primary-border);"
      >
        <p
          class="text-xs font-semibold mb-1"
          style="color: var(--cp-primary);"
        >
          {{ t('room.ended.yourResult') }}
        </p>
        <p
          class="text-4xl font-black mb-1"
          style="color: var(--cp-primary);"
        >
          {{ t('room.result.rank', { rank: gameStore.myRank }) }}
        </p>
        <p
          class="text-xl font-bold"
          style="color: var(--cp-secondary);"
        >
          {{ gameStore.myScore.toLocaleString() }}
        </p>
      </div>

      <!-- Full rankings -->
      <div
        class="rounded-2xl bg-white overflow-hidden mb-4"
        style="border: 1px solid var(--cp-border);"
      >
        <div
          v-for="(entry, i) in gameStore.rankings"
          :key="entry.nickname"
          class="flex items-center gap-3 px-4 py-3 transition-colors"
          :style="`
            ${entry.nickname === gameStore.myNickname ? 'background: var(--cp-primary-light);' : ''}
            ${i < gameStore.rankings.length - 1 ? 'border-bottom: 1px solid var(--cp-border);' : ''}
          `"
        >
          <span
            class="w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold shrink-0 text-white"
            :style="i === 0 ? 'background: var(--cp-primary);'
              : i === 1 ? 'background: var(--cp-info);'
                : i === 2 ? 'background: var(--cp-warning);'
                  : 'background: var(--cp-disabled); color: var(--cp-text-muted);'"
          >
            {{ i + 1 }}
          </span>
          <span
            class="flex-1 font-medium text-sm"
            :style="entry.nickname === gameStore.myNickname ? 'color: var(--cp-primary); font-weight: 700;' : ''"
          >
            {{ entry.nickname }}
            <span
              v-if="entry.nickname === gameStore.myNickname"
              class="text-xs ml-1"
            >{{ t('common.you') }}</span>
          </span>
          <span class="font-bold tabular-nums text-sm">{{ entry.score.toLocaleString() }}</span>
        </div>
      </div>

      <UButton
        block
        size="lg"
        variant="outline"
        @click="$router.push('/join')"
      >
        {{ t('room.ended.playAgain') }}
      </UButton>
    </template>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'default' })

const { t } = useI18n()
const route = useRoute()
const code = computed(() => (route.params.code as string).toUpperCase())

const gameStore = useGameStore()
const gameRoom = useGameRoom()

onMounted(async () => {
  if (!gameStore.myNickname) {
    await navigateTo(`/join?code=${code.value}`)
  }
})

const handleAnswer = async (index: number) => {
  if (gameStore.selectedAnswerIndex !== null) return
  gameStore.selectAnswer(index)
  await gameRoom.submitAnswer(code.value, index)
}

const optionLetters = ['A', 'B', 'C', 'D']

const optionStyle = (index: number): string => {
  const selected = gameStore.selectedAnswerIndex
  const correct = gameStore.correctAnswerIndex
  const phase = gameStore.phase

  if (phase === 'result' || (selected !== null && correct !== null)) {
    if (index === correct) return 'border-color: var(--cp-success); background: var(--cp-success-bg);'
    if (index === selected && index !== correct) return 'border-color: var(--cp-danger); background: var(--cp-danger-bg);'
    return 'opacity: 0.5; border-color: var(--cp-border);'
  }

  if (selected === index) return 'border-color: var(--cp-primary); background: var(--cp-primary-light);'

  return 'border-color: var(--cp-border);'
}

const optionTextStyle = (index: number): string => {
  const selected = gameStore.selectedAnswerIndex
  const correct = gameStore.correctAnswerIndex
  const phase = gameStore.phase

  if ((phase === 'result' || correct !== null) && index === correct) return 'color: #2e7d32; font-weight: 700;'
  if (selected === index && index !== correct) return 'color: var(--cp-danger);'
  return 'color: var(--cp-text-primary);'
}

onUnmounted(() => gameRoom.disconnect())
</script>

<script lang="ts">
export default {
  name: 'JoinRoomPage'
}
</script>

<style scoped lang="scss">
</style>
