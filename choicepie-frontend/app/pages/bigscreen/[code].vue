<template>
  <div class="flex flex-col min-h-screen p-8">
    <!-- Header -->
    <div class="flex items-center justify-between mb-6">
      <div class="flex items-center gap-3">
        <span class="text-2xl font-bold text-cp-primary">🥧 ChoicePie</span>
        <span class="px-3 py-1 rounded-full text-sm font-bold tracking-widest bg-white/10">
          {{ code }}
        </span>
      </div>
      <div class="flex items-center gap-2 text-sm text-white/50">
        <span>{{ t('common.players', { count: gameStore.playerCount }) }}</span>
      </div>
    </div>

    <!-- ─── WAITING ─── -->
    <template v-if="gameStore.phase === 'waiting' || gameStore.phase === 'idle'">
      <div class="flex-1 flex flex-col items-center justify-center text-center gap-8">
        <div>
          <p class="text-xl mb-4 text-white/60">
            {{ t('bigscreen.waiting.scanQR') }}
          </p>
          <div class="rounded-2xl p-4 inline-block bg-white">
            <img
              :src="`https://api.qrserver.com/v1/create-qr-code/?size=260x260&data=${encodeURIComponent(`${origin}/join?code=${code}`)}&bgcolor=ffffff&color=2d3748`"
              alt="QR Code"
              width="260"
              height="260"
            >
          </div>
        </div>
        <div>
          <p class="text-lg mb-2 text-white/50">
            {{ t('bigscreen.waiting.orEnterCode') }}
          </p>
          <p class="text-8xl font-black tracking-[0.2em] text-cp-primary">
            {{ code }}
          </p>
          <p class="text-lg mt-2 text-white/40">
            {{ t('bigscreen.waiting.joinUrl') }}
          </p>
        </div>
        <p class="text-2xl font-bold text-white/70">
          {{ t('bigscreen.waiting.playerCount', { count: gameStore.playerCount }) }}
        </p>
      </div>
    </template>

    <!-- ─── QUESTION ─── -->
    <template v-else-if="gameStore.phase === 'question' || gameStore.phase === 'result'">
      <!-- Question header -->
      <div class="flex items-center justify-between mb-6">
        <span class="text-lg font-semibold text-white/50">
          {{ t('bigscreen.playing.questionOf', { current: (gameStore.currentQuestion?.index ?? 0) + 1, total: gameStore.currentQuestion?.total }) }}
        </span>

        <!-- Timer -->
        <div
          class="w-20 h-20 rounded-full flex items-center justify-center text-4xl font-black border-4 tabular-nums"
          :class="gameStore.isTimerUrgent
            ? 'border-cp-danger text-cp-danger animate-[pulse_0.5s_ease-in-out_infinite_alternate]'
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

      <!-- Question text -->
      <p class="text-3xl font-bold text-white text-center mb-10 leading-relaxed quiz-text">
        {{ gameStore.currentQuestion?.text }}
      </p>

      <!-- Options grid -->
      <div class="grid grid-cols-2 gap-4 mb-8">
        <div
          v-for="(opt, i) in gameStore.currentQuestion?.options"
          :key="i"
          class="rounded-2xl p-5 flex items-center gap-4 border-2 transition-all duration-500"
          :class="[optionBorderClass(i), optionBackgroundClass(i), gameStore.correctAnswerIndex === i ? 'scale-[1.02]' : '']"
        >
          <span
            class="w-12 h-12 rounded-xl flex items-center justify-center text-xl font-black shrink-0 text-white"
            :class="optionColorClasses[i]"
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
        <span class="text-sm text-white/50">
          {{ t('bigscreen.playing.answeredOf', { answered: gameStore.answeredCount, total: gameStore.totalCount }) }}
        </span>
        <div class="flex-1 rounded-full overflow-hidden h-1.5 bg-white/10">
          <div
            class="h-full rounded-full transition-all duration-500 bg-cp-primary"
            :style="{ width: `${gameStore.totalCount > 0 ? (gameStore.answeredCount / gameStore.totalCount) * 100 : 0}%` }"
          />
        </div>
      </div>
    </template>

    <!-- ─── ENDED ─── -->
    <template v-else-if="gameStore.phase === 'ended'">
      <div class="flex-1 flex flex-col items-center justify-center">
        <p class="text-2xl font-bold mb-10 text-white/60">
          {{ t('bigscreen.ended.title') }}
        </p>

        <!-- Podium -->
        <div class="flex items-end justify-center gap-6 mb-12">
          <div
            v-for="podium in podiumEntries"
            :key="podium.entry?.nickname"
            class="flex flex-col items-center gap-3"
          >
            <p class="text-2xl">
              {{ podium.meta.medal }}
            </p>
            <p class="text-xl font-bold text-white">
              {{ podium.entry?.nickname }}
            </p>
            <p class="font-bold text-cp-primary">
              {{ podium.entry?.score.toLocaleString() }}
            </p>
            <div
              class="w-36 rounded-t-2xl flex items-start justify-center pt-3 text-white text-4xl font-black"
              :class="podium.meta.colorClass"
              :style="{ height: podium.meta.height }"
            >
              {{ podium.meta.place }}
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
            <p class="text-sm text-white/40">
              第 {{ i + 4 }} 名
            </p>
            <p class="font-bold text-white">
              {{ entry.nickname }}
            </p>
            <p class="text-sm text-cp-primary">
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
const optionColorClasses = ['bg-cp-danger', 'bg-cp-info', 'bg-cp-primary', 'bg-cp-success']
const optionBorderClasses = ['border-cp-danger/40', 'border-cp-info/40', 'border-cp-primary/40', 'border-cp-success/40']
const optionBgClasses = ['bg-cp-danger/15', 'bg-cp-info/15', 'bg-cp-primary/15', 'bg-cp-success/15']

const optionBorderClass = (index: number): string => {
  const correct = gameStore.correctAnswerIndex
  if (correct === null) return optionBorderClasses[index] ?? 'border-white/20'
  if (index === correct) return 'border-cp-success'
  return 'border-white/10'
}

const optionBackgroundClass = (index: number): string => {
  const correct = gameStore.correctAnswerIndex
  if (correct === null) return optionBgClasses[index] ?? 'bg-white/10'
  if (index === correct) return 'bg-cp-success/25'
  return 'bg-white/[0.03]'
}

// 頒獎台名次視覺（依原始名次 0=金 1=銀 2=銅），版面呈現順序為 銀/金/銅
const PODIUM_META = [
  { medal: '🥇', place: 2, colorClass: 'bg-cp-primary', height: '140px' },
  { medal: '🥈', place: 1, colorClass: 'bg-cp-info', height: '100px' },
  { medal: '🥉', place: 3, colorClass: 'bg-cp-warning', height: '70px' }
]
const podiumEntries = computed(() =>
  [1, 0, 2]
    .map(rank => ({ rank, entry: gameStore.rankings[rank], meta: PODIUM_META[rank]! }))
    .filter(p => p.entry)
)

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
