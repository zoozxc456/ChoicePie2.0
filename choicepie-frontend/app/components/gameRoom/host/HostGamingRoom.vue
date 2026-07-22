<template>
  <div class="grid grid-cols-[1fr_280px] gap-6">
    <!-- Left: Current question status -->
    <div class="flex flex-col gap-4">
      <div class="rounded-2xl p-6 bg-white border border-cp-border">
        <div class="flex items-center justify-between mb-4">
          <span class="text-xs font-semibold text-cp-text-muted">
            {{ t('host.playing.questionOf', { current: (gameStore.currentQuestion?.index ?? 0) + 1, total: gameStore.currentQuestion?.total }) }}
          </span>
          <span
            class="text-2xl font-bold tabular-nums"
            :class="gameStore.isTimerUrgent ? 'text-cp-danger' : 'text-cp-primary'"
          >
            {{ gameStore.timeLeft }}s
          </span>
        </div>

        <!-- Timer bar -->
        <div class="h-1.5 rounded-full overflow-hidden mb-5 bg-cp-surface-muted">
          <div
            class="h-full rounded-full transition-[width] duration-1000 ease-linear"
            :class="[gameStore.isTimerUrgent ? 'bg-cp-danger animate-pulse' : 'bg-cp-primary']"
            :style="{ width: `${gameStore.timerPercent}%` }"
          />
        </div>

        <p class="font-semibold leading-relaxed mb-4">
          {{ gameStore.currentQuestion?.text }}
        </p>

        <div class="grid grid-cols-2 gap-2">
          <div
            v-for="(opt, i) in gameStore.currentQuestion?.options"
            :key="i"
            class="relative rounded-xl p-3 text-sm font-medium overflow-hidden bg-cp-surface-muted border-[1.5px] border-cp-border"
          >
            <div
              class="absolute inset-y-0 left-0 transition-all duration-500 bg-cp-primary-light"
              :style="{ width: `${optionVotePercent(i)}%` }"
            />
            <div class="relative flex items-center justify-between gap-2">
              <span>
                <span class="font-bold mr-2 text-cp-primary">{{ ['A', 'B', 'C', 'D'][i] }}</span>
                {{ opt }}
              </span>
              <span class="text-xs font-bold shrink-0 text-cp-primary">{{ t('host.playing.voteCount', { count: gameStore.optionVoteCounts[i] ?? 0 }) }}</span>
            </div>
          </div>
        </div>
      </div>

      <!-- Answer Progress -->
      <div class="rounded-2xl p-5 bg-white border border-cp-border">
        <div class="flex items-center justify-between mb-2">
          <span class="text-sm font-semibold">{{ t('host.playing.answerProgress') }}</span>
          <span class="font-bold text-cp-primary">
            {{ gameStore.answeredCount }} / {{ gameStore.totalCount }}
          </span>
        </div>
        <div class="h-2 rounded-full overflow-hidden mb-4 bg-cp-surface-muted">
          <div
            class="h-full rounded-full transition-all duration-500 bg-cp-primary"
            :style="{ width: `${progressPercent}%` }"
          />
        </div>

        <TransitionGroup
          name="player-list"
          tag="div"
          class="flex flex-wrap gap-2"
        >
          <div
            v-for="player in gameStore.players"
            :key="player.id"
            class="player-chip"
            :class="{ 'player-chip-answered': player.hasAnswered }"
          >
            <span
              v-if="player.hasAnswered"
              class="font-bold"
            >{{ ['A', 'B', 'C', 'D'][player.selectedOptionIndex ?? 0] }}</span>
            <div
              v-else
              class="pending-dot"
            />
            {{ player.nickname }}
          </div>
        </TransitionGroup>
      </div>

      <!-- Host Controls -->
      <div class="flex gap-3">
        <UButton
          flex-1
          color="neutral"
          variant="outline"
          @click="handleSkip"
        >
          {{ t('host.playing.skipQuestion') }}
        </UButton>
        <UButton
          color="neutral"
          variant="outline"
          @click="gameRoom.pauseGame(props.code)"
        >
          {{ t('host.playing.pause') }}
        </UButton>
      </div>
    </div>

    <!-- Right: Live Rankings -->
    <div class="rounded-2xl p-5 bg-white border border-cp-border">
      <p class="text-sm font-semibold mb-4">
        {{ t('host.playing.liveRank') }}
      </p>

      <!-- 作答中：尚未公布排名 -->
      <div
        v-if="gameStore.phase === 'question'"
        class="flex flex-col items-center text-center py-6 gap-3"
      >
        <div class="text-4xl pie-spin">
          🥧
        </div>
        <p class="text-sm font-bold text-cp-primary">
          {{ t('host.playing.waitingForAnswers') }}
        </p>
        <p class="text-xs text-cp-text-muted">
          {{ t('host.playing.answeredSoFar', { answered: gameStore.answeredCount, total: gameStore.totalCount }) }}
        </p>
        <div class="flex gap-1.5">
          <span
            v-for="dot in 3"
            :key="dot"
            class="pie-dot"
            :style="{ animationDelay: `${(dot - 1) * 160}ms` }"
          />
        </div>
      </div>

      <!-- 已公布答案：即時排名 -->
      <TransitionGroup
        v-else
        name="rank"
        tag="div"
        class="flex flex-col gap-2"
      >
        <div
          v-for="entry in gameStore.rankings.slice(0, 8)"
          :key="entry.nickname"
          class="flex items-center gap-3 p-2 rounded-xl"
          :class="entry.rank <= 3 ? 'bg-cp-surface-muted' : ''"
        >
          <span
            class="w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold shrink-0"
            :class="entry.rank === 1 ? 'bg-cp-primary text-white'
              : entry.rank === 2 ? 'bg-cp-info text-white'
                : entry.rank === 3 ? 'bg-cp-warning text-white'
                  : 'bg-cp-surface-muted text-cp-text-muted'"
          >{{ entry.rank }}</span>
          <span class="flex-1 text-sm font-medium truncate">{{ entry.nickname }}</span>
          <span class="text-sm font-bold tabular-nums text-cp-primary">
            {{ entry.score.toLocaleString() }}
          </span>
        </div>
      </TransitionGroup>
    </div>
  </div>
</template>

<script lang="ts" setup>
interface Props {
  code: string
}

const props = defineProps<Props>()
const { t } = useI18n()
const gameStore = useGameStore()
const gameRoom = useGameRoom()

const progressPercent = computed(() =>
  gameStore.totalCount > 0
    ? (gameStore.answeredCount / gameStore.totalCount) * 100
    : 0
)

const optionVotePercent = (index: number) => {
  if (gameStore.answeredCount === 0) return 0
  return ((gameStore.optionVoteCounts[index] ?? 0) / gameStore.answeredCount) * 100
}

const handleSkip = () => {
  gameRoom.skipQuestion(props.code)
}
</script>

<style scoped lang="scss">
.player-list-enter-active { transition: all 0.3s ease; }
.player-list-enter-from { opacity: 0; transform: scale(0.8); }
.rank-move { transition: transform 0.5s ease; }

.pie-spin {
  animation: pie-wobble 1.4s ease-in-out infinite;
}

.pie-dot {
  width: 6px;
  height: 6px;
  border-radius: 9999px;
  background: var(--cp-primary);
  animation: pie-bounce 1s ease-in-out infinite;
}

@keyframes pie-wobble {
  0%, 100% { transform: rotate(-8deg) scale(1); }
  50% { transform: rotate(8deg) scale(1.08); }
}

@keyframes pie-bounce {
  0%, 80%, 100% { opacity: 0.3; transform: translateY(0); }
  40% { opacity: 1; transform: translateY(-4px); }
}
</style>
