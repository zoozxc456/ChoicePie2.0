<template>
  <div class="flex flex-col min-h-screen p-8">
    <!-- Header -->
    <div class="flex items-center justify-between mb-6">
      <div class="flex items-center gap-3">
        <span
          class="text-2xl font-bold"
          style="color: var(--cp-primary);"
        >🥧 ChoicePie</span>
        <span
          class="px-3 py-1 rounded-full text-sm font-bold tracking-widest"
          style="background: rgba(255,255,255,0.1);"
        >
          {{ code }}
        </span>
      </div>
      <div
        class="flex items-center gap-2 text-sm"
        style="color: rgba(255,255,255,0.5);"
      >
        <span>{{ t('common.players', { count: gameStore.playerCount }) }}</span>
      </div>
    </div>

    <!-- ─── WAITING ─── -->
    <template v-if="gameStore.phase === 'waiting' || gameStore.phase === 'idle'">
      <div class="flex-1 flex flex-col items-center justify-center text-center gap-8">
        <div>
          <p
            class="text-xl mb-4"
            style="color: rgba(255,255,255,0.6);"
          >
            {{ t('bigscreen.waiting.scanQR') }}
          </p>
          <div
            class="rounded-2xl p-4 inline-block"
            style="background: white;"
          >
            <img
              :src="`https://api.qrserver.com/v1/create-qr-code/?size=260x260&data=${encodeURIComponent(`${origin}/join?code=${code}`)}&bgcolor=ffffff&color=2d3748`"
              alt="QR Code"
              width="260"
              height="260"
            >
          </div>
        </div>
        <div>
          <p
            class="text-lg mb-2"
            style="color: rgba(255,255,255,0.5);"
          >
            {{ t('bigscreen.waiting.orEnterCode') }}
          </p>
          <p
            class="text-8xl font-black tracking-widest"
            style="color: var(--cp-primary); letter-spacing: 0.2em;"
          >
            {{ code }}
          </p>
          <p
            class="text-lg mt-2"
            style="color: rgba(255,255,255,0.4);"
          >
            {{ t('bigscreen.waiting.joinUrl') }}
          </p>
        </div>
        <p
          class="text-2xl font-bold"
          style="color: rgba(255,255,255,0.7);"
        >
          {{ t('bigscreen.waiting.playerCount', { count: gameStore.playerCount }) }}
        </p>
      </div>
    </template>

    <!-- ─── QUESTION ─── -->
    <template v-else-if="gameStore.phase === 'question' || gameStore.phase === 'result'">
      <!-- Question header -->
      <div class="flex items-center justify-between mb-6">
        <span
          class="text-lg font-semibold"
          style="color: rgba(255,255,255,0.5);"
        >
          {{ t('bigscreen.playing.questionOf', { current: (gameStore.currentQuestion?.index ?? 0) + 1, total: gameStore.currentQuestion?.total }) }}
        </span>

        <!-- Timer -->
        <div
          class="w-20 h-20 rounded-full flex items-center justify-center text-4xl font-black border-4 tabular-nums"
          :style="`
            border-color: ${gameStore.isTimerUrgent ? 'var(--cp-danger)' : 'var(--cp-primary)'};
            color: ${gameStore.isTimerUrgent ? 'var(--cp-danger)' : 'var(--cp-primary)'};
            ${gameStore.isTimerUrgent ? 'animation: pulse 0.5s ease-in-out infinite alternate;' : ''}
          `"
        >
          {{ gameStore.timeLeft }}
        </div>
      </div>

      <!-- Timer bar -->
      <div
        class="rounded-full overflow-hidden mb-8"
        style="height: 8px; background: rgba(255,255,255,0.1);"
      >
        <div
          class="h-full rounded-full"
          :style="`
            width: ${gameStore.timerPercent}%;
            background: ${gameStore.isTimerUrgent ? 'var(--cp-danger)' : 'var(--cp-primary)'};
            transition: width 1s linear;
          `"
        />
      </div>

      <!-- Question text -->
      <p class="text-3xl font-bold text-white text-center mb-10 leading-relaxed quiz-text">
        {{ gameStore.currentQuestion?.text }}
      </p>

      <!-- Options grid -->
      <div class="grid grid-cols-2 gap-4 mb-8">
        <div
          v-for="(opt, i) in gameStore.currentQuestion?.options"
          :key="i"
          class="rounded-2xl p-5 flex items-center gap-4 transition-all duration-500"
          :style="`
            border: 2px solid ${optionBorderColor(i)};
            background: ${optionBackground(i)};
            ${gameStore.correctAnswerIndex === i ? 'transform: scale(1.02);' : ''}
          `"
        >
          <span
            class="w-12 h-12 rounded-xl flex items-center justify-center text-xl font-black shrink-0"
            :style="`background: ${optionColors[i]}; color: white;`"
          >
            {{ optionLetters[i] }}
          </span>
          <span class="text-xl font-semibold text-white">{{ opt }}</span>
          <span
            v-if="gameStore.correctAnswerIndex === i"
            class="ml-auto text-2xl"
          >✓</span>
        </div>
      </div>

      <!-- Answer progress -->
      <div class="flex items-center gap-4">
        <span
          class="text-sm"
          style="color: rgba(255,255,255,0.5);"
        >
          {{ t('bigscreen.playing.answeredOf', { answered: gameStore.answeredCount, total: gameStore.totalCount }) }}
        </span>
        <div
          class="flex-1 rounded-full overflow-hidden"
          style="height: 6px; background: rgba(255,255,255,0.1);"
        >
          <div
            class="h-full rounded-full transition-all duration-500"
            :style="`
              width: ${gameStore.totalCount > 0 ? (gameStore.answeredCount / gameStore.totalCount) * 100 : 0}%;
              background: var(--cp-primary);
            `"
          />
        </div>
      </div>
    </template>

    <!-- ─── ENDED ─── -->
    <template v-else-if="gameStore.phase === 'ended'">
      <div class="flex-1 flex flex-col items-center justify-center">
        <p
          class="text-2xl font-bold mb-10"
          style="color: rgba(255,255,255,0.6);"
        >
          {{ t('bigscreen.ended.title') }}
        </p>

        <!-- Podium -->
        <div class="flex items-end justify-center gap-6 mb-12">
          <div
            v-for="(entry, i) in [gameStore.rankings[1], gameStore.rankings[0], gameStore.rankings[2]].filter(Boolean)"
            :key="entry?.nickname"
            class="flex flex-col items-center gap-3"
          >
            <p class="text-2xl">
              {{ [1, 0, 2][i] === 0 ? '🥇' : [1, 0, 2][i] === 1 ? '🥈' : '🥉' }}
            </p>
            <p class="text-xl font-bold text-white">
              {{ entry?.nickname }}
            </p>
            <p
              style="color: var(--cp-primary);"
              class="font-bold"
            >
              {{ entry?.score.toLocaleString() }}
            </p>
            <div
              class="w-36 rounded-t-2xl flex items-start justify-center pt-3 text-white text-4xl font-black"
              :style="`
                height: ${[1, 0, 2][i] === 0 ? '140px' : [1, 0, 2][i] === 1 ? '100px' : '70px'};
                background: ${[1, 0, 2][i] === 0 ? 'var(--cp-primary)' : [1, 0, 2][i] === 1 ? 'var(--cp-info)' : 'var(--cp-warning)'};
              `"
            >
              {{ [2, 1, 3][[1, 0, 2][i] as number] }}
            </div>
          </div>
        </div>

        <!-- Other players -->
        <div class="flex gap-6 flex-wrap justify-center">
          <div
            v-for="(entry, i) in gameStore.rankings.slice(3)"
            :key="entry.nickname"
            class="text-center"
          >
            <p
              class="text-sm"
              style="color: rgba(255,255,255,0.4);"
            >
              第 {{ i + 4 }} 名
            </p>
            <p class="font-bold text-white">
              {{ entry.nickname }}
            </p>
            <p
              class="text-sm"
              style="color: var(--cp-primary);"
            >
              {{ entry.score.toLocaleString() }}
            </p>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'big-screen' })

const { t } = useI18n()
const route = useRoute()
const code = computed(() => (route.params.code as string).toUpperCase())
const origin = useRequestURL().origin

const gameStore = useGameStore()
const gameRoom = useGameRoom()

const optionLetters = ['A', 'B', 'C', 'D']
const optionColors = ['#e53e3e', '#3182ce', '#f8931d', '#4caf50']
const optionBg = ['rgba(229,62,62,0.15)', 'rgba(49,130,206,0.15)', 'rgba(248,147,29,0.15)', 'rgba(76,175,80,0.15)']

const optionBorderColor = (index: number): string => {
  const correct = gameStore.correctAnswerIndex
  if (correct === null) return (optionColors[index] ?? '#ffffff') + '60'
  if (index === correct) return '#4caf50'
  return 'rgba(255,255,255,0.1)'
}

const optionBackground = (index: number): string => {
  const correct = gameStore.correctAnswerIndex
  if (correct === null) return optionBg[index] ?? 'rgba(255,255,255,0.1)'
  if (index === correct) return 'rgba(76,175,80,0.25)'
  return 'rgba(255,255,255,0.03)'
}

onMounted(async () => {
  await gameRoom.connect()
})

onUnmounted(() => gameRoom.disconnect())
</script>

<script lang="ts">
export default {
  name: 'BigscreenPage'
}
</script>

<style scoped lang="scss">
@keyframes pulse {
  from { opacity: 1; }
  to { opacity: 0.5; }
}
</style>
