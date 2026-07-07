<template>
  <div class="max-w-5xl mx-auto px-6 py-8">
    <!-- ─── WAITING ─── -->
    <template v-if="gameStore.phase === 'waiting'">
      <div class="flex items-center justify-between mb-6">
        <div>
          <h1 class="text-2xl font-bold mb-1">
            {{ t('host.waiting.title') }}
          </h1>
          <p class="text-sm text-cp-text-secondary">
            {{ t('host.waiting.subtitle') }}
          </p>
        </div>
        <NuxtLink
          :to="bigscreenUrl"
          target="_blank"
          class="flex items-center gap-2 text-sm font-medium px-4 py-2 rounded-lg border-[1.5px] border-cp-border text-cp-text-secondary"
        >
          {{ t('host.waiting.bigscreen') }}
        </NuxtLink>
      </div>

      <div class="grid grid-cols-[1fr_280px] gap-6">
        <!-- Left: Room info + Players -->
        <div class="flex flex-col gap-4">
          <!-- Room Code -->
          <div class="rounded-2xl p-6 bg-white border border-cp-border">
            <p class="text-xs font-semibold mb-2 text-cp-text-muted">
              {{ t('host.waiting.roomCode') }}
            </p>
            <div class="room-code">
              {{ code }}
            </div>
            <p class="text-xs mt-2 text-cp-text-muted">
              {{ joinUrl }}
            </p>
          </div>

          <!-- Player List -->
          <div class="rounded-2xl p-6 bg-white flex-1 border border-cp-border">
            <div class="flex items-center justify-between mb-4">
              <p class="text-sm font-semibold">
                {{ t('host.waiting.joinedPlayers') }}
              </p>
              <span class="text-lg font-bold text-cp-primary">{{ t('host.waiting.playerCount', { count: gameStore.playerCount }) }}</span>
            </div>

            <TransitionGroup
              name="player-list"
              tag="div"
              class="flex flex-wrap gap-2"
            >
              <div
                v-for="player in gameStore.players"
                :key="player.connectionId"
                class="player-chip"
              >
                <div class="online-dot" />
                {{ player.nickname }}
              </div>
            </TransitionGroup>

            <div
              v-if="gameStore.playerCount === 0"
              class="text-center py-8 text-sm text-cp-text-muted"
            >
              {{ t('host.waiting.noPlayers') }}
            </div>
          </div>

          <!-- Actions -->
          <UButton
            block
            size="lg"
            color="primary"
            :disabled="gameStore.playerCount === 0"
            @click="handleStart"
          >
            {{ t('host.waiting.startGame', { count: gameStore.playerCount }) }}
          </UButton>
          <UButton
            block
            size="md"
            color="neutral"
            variant="ghost"
          >
            {{ t('host.waiting.waitMore') }}
          </UButton>
        </div>

        <!-- Right: QR Code -->
        <div class="flex flex-col items-center justify-start pt-2">
          <div class="rounded-2xl p-5 bg-white w-full flex flex-col items-center border border-cp-border">
            <div class="rounded-xl overflow-hidden mb-3 bg-cp-secondary p-3">
              <img
                :src="qrUrl"
                alt="QR Code"
                width="200"
                height="200"
                class="block rounded"
              >
            </div>
            <p class="text-sm font-semibold mb-1">
              {{ t('host.waiting.scanToJoin') }}
            </p>
            <p class="text-xs text-cp-text-muted">
              choicepie.app/join
            </p>
          </div>
        </div>
      </div>
    </template>

    <!-- ─── PLAYING / RESULT ─── -->
    <template v-else-if="gameStore.phase === 'question' || gameStore.phase === 'result'">
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
                :key="player.connectionId"
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
              @click="gameRoom.pauseGame(code)"
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
              v-for="(entry, i) in gameStore.rankings.slice(0, 8)"
              :key="entry.nickname"
              class="flex items-center gap-3 p-2 rounded-xl"
              :class="i < 3 ? 'bg-cp-surface-muted' : ''"
            >
              <span
                class="w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold shrink-0"
                :class="i === 0 ? 'bg-cp-primary text-white'
                  : i === 1 ? 'bg-cp-info text-white'
                    : i === 2 ? 'bg-cp-warning text-white'
                      : 'bg-cp-surface-muted text-cp-text-muted'"
              >{{ i + 1 }}</span>
              <span class="flex-1 text-sm font-medium truncate">{{ entry.nickname }}</span>
              <span class="text-sm font-bold tabular-nums text-cp-primary">
                {{ entry.score.toLocaleString() }}
              </span>
            </div>
          </TransitionGroup>
        </div>
      </div>
    </template>

    <!-- ─── ENDED ─── -->
    <template v-else-if="gameStore.phase === 'ended'">
      <div class="text-center mb-8">
        <h1 class="text-3xl font-bold mb-2">
          {{ t('host.ended.title') }}
        </h1>
        <p class="text-cp-text-secondary">
          {{ t('host.ended.finalRank') }}
        </p>
      </div>

      <!-- Podium -->
      <div class="flex items-end justify-center gap-4 mb-8">
        <div
          v-for="podium in podiumEntries"
          :key="podium.entry?.nickname"
          class="flex flex-col items-center gap-2"
        >
          <div
            class="w-20 h-20 rounded-full flex items-center justify-center text-2xl font-black text-white"
            :class="[podium.meta.colorClass, podium.rank === 0 ? podium.meta.glowClass : '']"
          >
            {{ podium.meta.medal }}
          </div>
          <p class="font-bold text-sm">
            {{ podium.entry?.nickname }}
          </p>
          <p class="text-xs text-cp-text-muted">
            {{ podium.entry?.score.toLocaleString() }}
          </p>
          <div
            class="rounded-t-xl w-24 flex items-end justify-center pb-2 text-white font-black text-lg"
            :class="podium.meta.colorClass"
            :style="{ height: podium.meta.height }"
          >
            {{ podium.meta.place }}
          </div>
        </div>
      </div>

      <!-- Full list -->
      <div class="rounded-2xl bg-white overflow-hidden mb-6 border border-cp-border">
        <div
          v-for="(entry, i) in gameStore.rankings"
          :key="entry.nickname"
          class="flex items-center gap-4 px-5 py-3"
          :class="i < gameStore.rankings.length - 1 ? 'border-b border-cp-border' : ''"
        >
          <span class="w-8 text-center font-bold text-cp-text-muted">{{ i + 1 }}</span>
          <span class="flex-1 font-medium">{{ entry.nickname }}</span>
          <span class="font-bold tabular-nums text-cp-primary">
            {{ entry.score.toLocaleString() }}
          </span>
        </div>
      </div>

      <UButton
        block
        size="lg"
        color="primary"
        @click="$router.push('/library/new')"
      >
        {{ t('host.ended.hostAgain') }}
      </UButton>
    </template>

    <!-- ─── REJOIN FAILED ─── -->
    <template v-else-if="rejoinFailed">
      <div class="flex flex-col items-center justify-center py-24 gap-3 text-center">
        <p class="text-2xl font-bold">
          {{ t('host.rejoinFailed.title') }}
        </p>
        <p class="text-sm text-cp-text-secondary">
          {{ t('host.rejoinFailed.desc') }}
        </p>
        <UButton
          size="lg"
          color="primary"
          class="mt-2"
          @click="$router.push('/library')"
        >
          {{ t('host.rejoinFailed.backToLibrary') }}
        </UButton>
      </div>
    </template>

    <!-- ─── RECONNECTING ─── -->
    <template v-else>
      <div class="flex flex-col items-center justify-center py-24 gap-3">
        <UIcon
          name="i-lucide-loader-2"
          class="animate-spin text-4xl text-cp-primary"
        />
        <p class="text-sm text-cp-text-secondary">
          {{ t('host.reconnecting') }}
        </p>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'default', middleware: ['auth'] })

const { t } = useI18n()
const route = useRoute()
const code = computed(() => (route.params.code as string).toUpperCase())

const gameStore = useGameStore()
const gameRoom = useGameRoom()

const origin = useRequestURL().origin
const joinUrl = computed(() => `${origin}/join?code=${code.value}`)
const qrUrl = computed(() =>
  `https://api.qrserver.com/v1/create-qr-code/?size=240x240&data=${encodeURIComponent(joinUrl.value)}&bgcolor=ffffff&color=2d3748`
)

const bigscreenUrl = computed(() => `/bigscreen/${code.value}`)

const progressPercent = computed(() =>
  gameStore.totalCount > 0
    ? (gameStore.answeredCount / gameStore.totalCount) * 100
    : 0
)

const optionVotePercent = (index: number) => {
  if (gameStore.answeredCount === 0) return 0
  return ((gameStore.optionVoteCounts[index] ?? 0) / gameStore.answeredCount) * 100
}

// 頒獎台名次視覺（依原始名次 0=金 1=銀 2=銅）
const PODIUM_META = [
  { medal: '🥇', place: 2, colorClass: 'bg-cp-primary', glowClass: 'rank-1', height: '80px' },
  { medal: '🥈', place: 1, colorClass: 'bg-cp-info', glowClass: 'rank-2', height: '60px' },
  { medal: '🥉', place: 3, colorClass: 'bg-cp-warning', glowClass: 'rank-3', height: '40px' }
]
// 版面呈現順序為 銀/金/銅（原始名次 1/0/2）
const podiumEntries = computed(() =>
  [1, 0, 2]
    .map(rank => ({ rank, entry: gameStore.rankings[rank], meta: PODIUM_META[rank]! }))
    .filter(p => p.entry)
)

const rejoinFailed = ref(false)

// 暫時用假資料模擬遊戲進行，之後接上真實後端（SignalR）後移除
const MOCK_QUESTIONS = Array.from({ length: 5 }, (_, i) => ({
  index: i,
  total: 5,
  text: `範例題目 ${i + 1}？`,
  options: ['選項 A', '選項 B', '選項 C', '選項 D'],
  timeLimit: 20,
  answerIndex: i % 4,
  explanation: `這是第 ${i + 1} 題的範例解析說明。`
}))

const mockQuestionIndex = ref(0)
let _mockAdvanceTimeout: ReturnType<typeof setTimeout> | null = null
let _mockAnswerTimeouts: ReturnType<typeof setTimeout>[] = []

const _clearMockAnswerTimeouts = () => {
  _mockAnswerTimeouts.forEach(clearTimeout)
  _mockAnswerTimeouts = []
}

const _scheduleMockAnswers = () => {
  const total = gameStore.players.length
  const optionCount = gameStore.currentQuestion?.options.length ?? 4
  gameStore.players.forEach((player, i) => {
    const delay = 1500 + i * 2500 + Math.random() * 1500
    const selectedOptionIndex = Math.floor(Math.random() * optionCount)
    _mockAnswerTimeouts.push(setTimeout(() => {
      gameStore.setAnswerProgress({
        connectionId: player.connectionId,
        answered: gameStore.answeredCount + 1,
        total,
        selectedOptionIndex
      })
    }, delay))
  })
}

const mockShowResult = () => {
  const q = MOCK_QUESTIONS[mockQuestionIndex.value]!
  const rankings = gameStore.players
    .map((p, i) => ({ rank: i + 1, nickname: p.nickname, score: (i === 0 ? 300 : 150) * (mockQuestionIndex.value + 1) }))
    .sort((a, b) => b.score - a.score)
    .map((r, i) => ({ ...r, rank: i + 1 }))

  gameStore.setQuestionEnd({
    answerIndex: q.answerIndex,
    explanation: q.explanation,
    rankings
  })
}

const mockShowNextQuestion = () => {
  if (mockQuestionIndex.value >= MOCK_QUESTIONS.length) {
    const finalRankings = gameStore.rankings.length
      ? gameStore.rankings
      : gameStore.players.map((p, i) => ({ rank: i + 1, nickname: p.nickname, score: 0 }))
    gameStore.endGame(finalRankings)
    return
  }

  const q = MOCK_QUESTIONS[mockQuestionIndex.value]!
  gameStore.setQuestion(q)
  mockQuestionIndex.value++
  _mockAdvanceTimeout = setTimeout(mockShowResult, q.timeLimit * 1000)
  _scheduleMockAnswers()
}

const handleStart = () => {
  mockQuestionIndex.value = 0
  mockShowNextQuestion()
}

const handleSkip = () => {
  if (_mockAdvanceTimeout) clearTimeout(_mockAdvanceTimeout)
  _clearMockAnswerTimeouts()
  if (gameStore.phase === 'question') {
    mockShowResult()
  } else {
    mockShowNextQuestion()
  }
}

onMounted(() => {
  // 若非從建立房間流程 client-side 導航過來（例如重新整理、直接開啟連結），
  // gameStore 尚未有此房間的狀態，先用假房間資料填充
  if (gameStore.roomCode !== code.value) {
    gameStore.setRoom({
      roomCode: code.value,
      quizTitle: 'Kubernetes 核心概念入門',
      status: 'waiting',
      players: [
        { connectionId: 'mock-1', nickname: '小美', score: 0, rank: 0, hasAnswered: false },
        { connectionId: 'mock-2', nickname: 'Leo', score: 0, rank: 0, hasAnswered: false }
      ],
      currentQuestionIndex: 0,
      totalQuestions: MOCK_QUESTIONS.length,
      hostConnectionId: 'mock-host'
    })
  }
})

onUnmounted(() => {
  if (_mockAdvanceTimeout) clearTimeout(_mockAdvanceTimeout)
  _clearMockAnswerTimeouts()
  gameRoom.disconnect()
})
</script>

<script lang="ts">
export default {
  name: 'HostRoomPage'
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
