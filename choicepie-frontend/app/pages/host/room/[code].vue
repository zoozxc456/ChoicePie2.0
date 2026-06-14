<template>
  <div class="max-w-5xl mx-auto px-6 py-8">
    <!-- ─── WAITING ─── -->
    <template v-if="gameStore.phase === 'waiting'">
      <div class="flex items-center justify-between mb-6">
        <div>
          <h1 class="text-2xl font-bold mb-1">
            {{ t('host.waiting.title') }}
          </h1>
          <p style="color: var(--cp-text-secondary); font-size: 14px;">
            {{ t('host.waiting.subtitle') }}
          </p>
        </div>
        <NuxtLink
          :to="bigscreenUrl"
          target="_blank"
          class="flex items-center gap-2 text-sm font-medium px-4 py-2 rounded-lg"
          style="border: 1.5px solid var(--cp-border); color: var(--cp-text-secondary);"
        >
          {{ t('host.waiting.bigscreen') }}
        </NuxtLink>
      </div>

      <div class="grid grid-cols-[1fr_280px] gap-6">
        <!-- Left: Room info + Players -->
        <div class="flex flex-col gap-4">
          <!-- Room Code -->
          <div
            class="rounded-2xl p-6 bg-white"
            style="border: 1px solid var(--cp-border);"
          >
            <p
              class="text-xs font-semibold mb-2"
              style="color: var(--cp-text-muted);"
            >
              {{ t('host.waiting.roomCode') }}
            </p>
            <div class="room-code">
              {{ code }}
            </div>
            <p
              class="text-xs mt-2"
              style="color: var(--cp-text-muted);"
            >
              {{ joinUrl }}
            </p>
          </div>

          <!-- Player List -->
          <div
            class="rounded-2xl p-6 bg-white flex-1"
            style="border: 1px solid var(--cp-border);"
          >
            <div class="flex items-center justify-between mb-4">
              <p class="text-sm font-semibold">
                {{ t('host.waiting.joinedPlayers') }}
              </p>
              <span
                class="text-lg font-bold"
                style="color: var(--cp-primary);"
              >{{ t('host.waiting.playerCount', { count: gameStore.playerCount }) }}</span>
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
              class="text-center py-8"
              style="color: var(--cp-text-muted); font-size: 13px;"
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
          <div
            class="rounded-2xl p-5 bg-white w-full flex flex-col items-center"
            style="border: 1px solid var(--cp-border);"
          >
            <div
              class="rounded-xl overflow-hidden mb-3"
              style="background: var(--cp-secondary); padding: 12px;"
            >
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
            <p
              class="text-xs"
              style="color: var(--cp-text-muted);"
            >
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
          <div
            class="rounded-2xl p-6 bg-white"
            style="border: 1px solid var(--cp-border);"
          >
            <div class="flex items-center justify-between mb-4">
              <span
                class="text-xs font-semibold"
                style="color: var(--cp-text-muted);"
              >
                {{ t('host.playing.questionOf', { current: (gameStore.currentQuestion?.index ?? 0) + 1, total: gameStore.currentQuestion?.total }) }}
              </span>
              <span
                class="text-2xl font-bold tabular-nums"
                :style="`color: ${gameStore.isTimerUrgent ? 'var(--cp-danger)' : 'var(--cp-primary)'};`"
              >
                {{ gameStore.timeLeft }}s
              </span>
            </div>

            <!-- Timer bar -->
            <div
              class="rounded-full overflow-hidden mb-5"
              style="height: 6px; background: var(--cp-surface-muted);"
            >
              <div
                class="h-full rounded-full transition-all"
                :class="{ 'animate-pulse': gameStore.isTimerUrgent }"
                :style="`
                  width: ${gameStore.timerPercent}%;
                  background: ${gameStore.isTimerUrgent ? 'var(--cp-danger)' : 'var(--cp-primary)'};
                  transition: width 1s linear;
                `"
              />
            </div>

            <p class="font-semibold leading-relaxed mb-4">
              {{ gameStore.currentQuestion?.text }}
            </p>

            <div class="grid grid-cols-2 gap-2">
              <div
                v-for="(opt, i) in gameStore.currentQuestion?.options"
                :key="i"
                class="rounded-xl p-3 text-sm font-medium"
                :style="`background: var(--cp-surface-muted); border: 1.5px solid var(--cp-border);`"
              >
                <span
                  class="font-bold mr-2"
                  style="color: var(--cp-primary);"
                >{{ ['A', 'B', 'C', 'D'][i] }}</span>
                {{ opt }}
              </div>
            </div>
          </div>

          <!-- Answer Progress -->
          <div
            class="rounded-2xl p-5 bg-white"
            style="border: 1px solid var(--cp-border);"
          >
            <div class="flex items-center justify-between mb-2">
              <span class="text-sm font-semibold">{{ t('host.playing.answerProgress') }}</span>
              <span
                class="font-bold"
                style="color: var(--cp-primary);"
              >
                {{ gameStore.answeredCount }} / {{ gameStore.totalCount }}
              </span>
            </div>
            <div
              class="rounded-full overflow-hidden"
              style="height: 8px; background: var(--cp-surface-muted);"
            >
              <div
                class="h-full rounded-full transition-all duration-500"
                :style="`width: ${progressPercent}%; background: var(--cp-primary);`"
              />
            </div>
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
        <div
          class="rounded-2xl p-5 bg-white"
          style="border: 1px solid var(--cp-border);"
        >
          <p class="text-sm font-semibold mb-4">
            {{ t('host.playing.liveRank') }}
          </p>
          <TransitionGroup
            name="rank"
            tag="div"
            class="flex flex-col gap-2"
          >
            <div
              v-for="(entry, i) in gameStore.rankings.slice(0, 8)"
              :key="entry.nickname"
              class="flex items-center gap-3 p-2 rounded-xl"
              :style="i < 3 ? 'background: var(--cp-surface-muted);' : ''"
            >
              <span
                class="w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold shrink-0"
                :style="i === 0 ? 'background: var(--cp-primary); color: white;'
                  : i === 1 ? 'background: var(--cp-info); color: white;'
                    : i === 2 ? 'background: var(--cp-warning); color: white;'
                      : 'background: var(--cp-surface-muted); color: var(--cp-text-muted);'"
              >{{ i + 1 }}</span>
              <span class="flex-1 text-sm font-medium truncate">{{ entry.nickname }}</span>
              <span
                class="text-sm font-bold tabular-nums"
                style="color: var(--cp-primary);"
              >
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
        <p style="color: var(--cp-text-secondary);">
          {{ t('host.ended.finalRank') }}
        </p>
      </div>

      <!-- Podium -->
      <div class="flex items-end justify-center gap-4 mb-8">
        <div
          v-for="(entry, i) in [gameStore.rankings[1], gameStore.rankings[0], gameStore.rankings[2]].filter(Boolean)"
          :key="entry?.nickname"
          class="flex flex-col items-center gap-2"
        >
          <div
            class="w-20 h-20 rounded-full flex items-center justify-center text-2xl font-black text-white"
            :style="[1, 0, 2][i] === 0 ? `background: var(--cp-primary); box-shadow: 0 0 24px rgba(248,147,29,0.5);`
              : [1, 0, 2][i] === 1 ? `background: var(--cp-info);`
                : `background: var(--cp-warning);`"
          >
            {{ [1, 0, 2][i] === 0 ? '🥇' : [1, 0, 2][i] === 1 ? '🥈' : '🥉' }}
          </div>
          <p class="font-bold text-sm">
            {{ entry?.nickname }}
          </p>
          <p
            class="text-xs"
            style="color: var(--cp-text-muted);"
          >
            {{ entry?.score.toLocaleString() }}
          </p>
          <div
            class="rounded-t-xl w-24 flex items-end justify-center pb-2 text-white font-black text-lg"
            :style="`
              height: ${[1, 0, 2][i] === 0 ? '80px' : [1, 0, 2][i] === 1 ? '60px' : '40px'};
              background: ${[1, 0, 2][i] === 0 ? 'var(--cp-primary)' : [1, 0, 2][i] === 1 ? 'var(--cp-info)' : 'var(--cp-warning)'};
            `"
          >
            {{ [2, 1, 3][[1, 0, 2][i] as number] }}
          </div>
        </div>
      </div>

      <!-- Full list -->
      <div
        class="rounded-2xl bg-white overflow-hidden mb-6"
        style="border: 1px solid var(--cp-border);"
      >
        <div
          v-for="(entry, i) in gameStore.rankings"
          :key="entry.nickname"
          class="flex items-center gap-4 px-5 py-3"
          :style="i < gameStore.rankings.length - 1 ? 'border-bottom: 1px solid var(--cp-border);' : ''"
        >
          <span
            class="w-8 text-center font-bold"
            style="color: var(--cp-text-muted);"
          >{{ i + 1 }}</span>
          <span class="flex-1 font-medium">{{ entry.nickname }}</span>
          <span
            class="font-bold tabular-nums"
            style="color: var(--cp-primary);"
          >
            {{ entry.score.toLocaleString() }}
          </span>
        </div>
      </div>

      <UButton
        block
        size="lg"
        color="primary"
        @click="$router.push('/host/new')"
      >
        {{ t('host.ended.hostAgain') }}
      </UButton>
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

const handleStart = async () => {
  await gameRoom.startGame(code.value)
}

const handleSkip = async () => {
  await gameRoom.skipQuestion(code.value)
}

onUnmounted(() => gameRoom.disconnect())
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
</style>
