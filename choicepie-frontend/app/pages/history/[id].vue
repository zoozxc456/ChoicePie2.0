<template>
  <div
    v-if="!game"
    class="max-w-3xl mx-auto px-6 py-20 text-center"
  >
    <p class="text-sm text-cp-text-secondary mb-4">
      {{ t('history.detail.notFound') }}
    </p>
    <NuxtLink to="/history">
      <UButton
        color="primary"
        size="lg"
        class="rounded-full px-6 font-bold"
      >
        {{ t('history.detail.back') }}
      </UButton>
    </NuxtLink>
  </div>

  <div
    v-else
    class="max-w-6xl mx-auto px-6 py-8"
  >
    <NuxtLink
      to="/history"
      class="inline-flex items-center gap-1.5 text-sm font-semibold text-cp-text-secondary mb-6 hover:text-cp-primary"
    >
      <UIcon name="i-lucide-arrow-left" />
      {{ t('history.detail.back') }}
    </NuxtLink>

    <div class="grid grid-cols-1 lg:grid-cols-[minmax(0,320px)_1fr] gap-6 items-start">
      <!-- ─── Left: Game info ─── -->
      <div class="flex flex-col gap-5">
        <div class="rounded-2xl bg-white border border-cp-border p-6 text-center">
          <div
            class="mx-auto w-20 h-20 rounded-2xl flex items-center justify-center text-4xl mb-4"
            :style="game.coverGradient"
          >
            {{ game.coverEmoji }}
          </div>
          <span
            class="inline-block text-xs font-bold px-3 py-1 rounded-full mb-3"
            :class="isHost ? 'bg-cp-primary-light text-cp-primary' : 'bg-cp-info-bg text-cp-info'"
          >
            {{ isHost ? t('history.detail.hostBadge') : t('history.detail.playerBadge') }}
          </span>
          <h1 class="text-lg font-black mb-3">
            {{ game.quizTitle }}
          </h1>
          <div class="flex items-center justify-center gap-3 text-sm text-cp-text-secondary">
            <span>{{ t('history.detail.meta.players', { count: game.playerCount }) }}</span>
            <span>·</span>
            <span>{{ t('history.detail.meta.questions', { count: game.questionCount }) }}</span>
          </div>
          <p class="text-xs text-cp-text-muted mt-1.5">
            {{ t('history.detail.meta.playedAt', { date: formatDate(game.playedAt) }) }}
          </p>
        </div>

        <!-- Player: wrong-answer review -->
        <div
          v-if="!isHost && playedGame"
          class="rounded-2xl bg-white border border-cp-border p-5"
        >
          <h2 class="text-sm font-bold mb-1">
            {{ t('history.detail.review.title') }}
          </h2>
          <p class="text-xs text-cp-text-muted mb-4">
            {{ playedGame.wrongAnswers.length
              ? t('history.detail.review.subtitle', { count: playedGame.wrongAnswers.length })
              : t('history.detail.review.empty') }}
          </p>

          <div class="flex flex-col gap-4">
            <div
              v-for="(qa, i) in playedGame.wrongAnswers"
              :key="i"
              class="rounded-xl border border-cp-border p-4"
            >
              <p class="text-sm font-semibold mb-3 leading-relaxed">
                {{ i + 1 }}. {{ qa.questionText }}
              </p>
              <div class="flex flex-col gap-1.5 mb-3">
                <div
                  v-for="(opt, oi) in qa.options"
                  :key="oi"
                  class="text-xs rounded-lg px-3 py-2 border"
                  :class="reviewOptionClass(qa, oi)"
                >
                  {{ opt }}
                  <span
                    v-if="oi === qa.myAnswerIndex"
                    class="font-semibold"
                  > — {{ t('history.detail.review.yourAnswer') }}</span>
                  <span
                    v-if="oi === qa.correctAnswerIndex"
                    class="font-semibold"
                  > — {{ t('history.detail.review.correctAnswer') }}</span>
                </div>
              </div>
              <div class="rounded-lg bg-cp-surface-muted px-3 py-2.5 text-xs leading-relaxed text-cp-text-secondary">
                <span class="font-semibold text-cp-text-primary">{{ t('history.detail.review.explanation') }}：</span>
                {{ qa.explanation }}
              </div>
            </div>
          </div>
        </div>

        <!-- Host: placeholder for future analytics -->
        <div
          v-else-if="isHost"
          class="rounded-2xl bg-cp-surface-muted border border-dashed border-cp-border p-5 text-center"
        >
          <div class="text-2xl mb-2">
            🚧
          </div>
          <p class="text-sm font-bold mb-1">
            {{ t('history.detail.hostNote.title') }}
          </p>
          <p class="text-xs text-cp-text-muted leading-relaxed">
            {{ t('history.detail.hostNote.desc') }}
          </p>
        </div>
      </div>

      <!-- ─── Right: Podium + ranking ─── -->
      <div class="flex flex-col gap-6">
        <!-- Podium -->
        <div class="rounded-2xl bg-white border border-cp-border p-6">
          <h2 class="text-sm font-bold text-center mb-6">
            {{ t('history.detail.podiumTitle') }}
          </h2>
          <div class="flex items-end justify-center gap-4">
            <div
              v-for="podium in podiumEntries"
              :key="podium.entry?.nickname"
              class="podium-item flex flex-col items-center gap-2"
              :style="{ animationDelay: `${podium.order * 140}ms` }"
            >
              <div
                class="w-16 h-16 rounded-full flex items-center justify-center text-xl font-black text-white"
                :class="[podium.meta.colorClass, podium.rank === 0 ? podium.meta.glowClass : '']"
              >
                {{ podium.meta.medal }}
              </div>
              <p
                class="font-bold text-sm truncate max-w-24"
                :class="isMe(podium.entry?.nickname) ? 'text-cp-primary' : ''"
              >
                {{ podium.entry?.nickname }}{{ isMe(podium.entry?.nickname) ? t('history.detail.you') : '' }}
              </p>
              <p class="text-xs text-cp-text-muted">
                {{ podium.entry?.score.toLocaleString() }}
              </p>
              <div
                class="w-20 flex items-end justify-center"
                :style="{ height: podium.meta.height }"
              >
                <div
                  class="podium-bar rounded-t-xl w-20 h-full flex items-end justify-center pb-2 text-white font-black text-base"
                  :class="[podium.meta.colorClass, barsMounted ? 'podium-bar--grown' : '']"
                  :style="{ transitionDelay: `${podium.order * 140 + 120}ms` }"
                >
                  {{ podium.meta.place }}
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Full ranking -->
        <div class="rounded-2xl bg-white overflow-hidden border border-cp-border">
          <h2 class="text-sm font-bold px-5 pt-4 pb-3">
            {{ t('history.detail.fullRanking') }}
          </h2>
          <div
            v-for="(entry, i) in game.rankings"
            :key="entry.nickname"
            class="rank-row relative flex items-center gap-4 px-5 py-3 overflow-hidden"
            :class="[
              i < game.rankings.length - 1 ? 'border-b border-cp-border' : '',
              isMe(entry.nickname) ? 'bg-cp-primary-light' : ''
            ]"
            :style="{ animationDelay: `${i * 45}ms` }"
          >
            <div
              class="rank-row__bar absolute inset-y-0 left-0"
              :class="[rankBarClass(entry.rank), barsMounted ? 'rank-row__bar--grown' : '']"
              :style="{
                width: `${scorePercent(entry.score)}%`,
                transitionDelay: `${i * 45 + 200}ms`
              }"
            />
            <span
              class="relative w-8 h-8 rounded-full flex items-center justify-center text-xs font-black text-white shrink-0"
              :class="rankBarClass(entry.rank)"
            >
              {{ entry.rank <= 3 ? rankMedal(entry.rank) : entry.rank }}
            </span>
            <span
              class="relative flex-1 font-medium text-sm"
              :class="isMe(entry.nickname) ? 'text-cp-primary font-bold' : ''"
            >
              {{ entry.nickname }}{{ isMe(entry.nickname) ? t('history.detail.you') : '' }}
            </span>
            <span class="relative font-bold tabular-nums text-sm text-cp-primary">
              {{ entry.score.toLocaleString() }}
            </span>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { mockHostedGames, mockPlayedGames } from '~/mocks/history'

definePageMeta({ layout: 'content' })

const { t, locale } = useI18n()
const route = useRoute()

const id = computed(() => route.params.id as string)

const hostedGame = computed(() => mockHostedGames.find(g => g.id === id.value))
const playedGame = computed(() => mockPlayedGames.find(g => g.id === id.value))

const isHost = computed(() => !!hostedGame.value)
const game = computed(() => hostedGame.value ?? playedGame.value)

const isMe = (nickname?: string) => !isHost.value && nickname === '你'

// 頒獎台/排名條狀圖的動態高度、寬度無法用 CSS @keyframes 動畫（custom property 不會被插值），
// 改用掛載後切換內聯樣式搭配 CSS transition 觸發進場動畫。
// 巢狀兩層 rAF 確保瀏覽器先畫出 0 的初始狀態，避免與掛載同一幀導致 transition 被跳過
const barsMounted = ref(false)
onMounted(() => {
  requestAnimationFrame(() => {
    requestAnimationFrame(() => {
      barsMounted.value = true
    })
  })
})

const formatDate = (iso: string) =>
  new Date(iso).toLocaleDateString(locale.value === 'en' ? 'en-US' : 'zh-TW', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })

// 頒獎台名次視覺（依原始名次 0=金 1=銀 2=銅），與 host/room/[code].vue 共用同一套視覺語言
// order 為畫面呈現順序（銀/金/銅），用來讓進場動畫從中間的冠軍開始擴散
const PODIUM_META = [
  { medal: '🥇', place: 2, colorClass: 'bg-cp-primary', glowClass: 'rank-1', height: '70px' },
  { medal: '🥈', place: 1, colorClass: 'bg-cp-info', glowClass: 'rank-2', height: '52px' },
  { medal: '🥉', place: 3, colorClass: 'bg-cp-warning', glowClass: 'rank-3', height: '36px' }
]
const PODIUM_ORDER = [1, 0, 2]
const podiumEntries = computed(() =>
  PODIUM_ORDER
    .map((rank, order) => ({ rank, order, entry: game.value?.rankings[rank], meta: PODIUM_META[rank]! }))
    .filter(p => p.entry)
)

const topScore = computed(() => game.value?.rankings[0]?.score ?? 1)
const scorePercent = (score: number) => Math.max((score / topScore.value) * 100, 6)

const rankMedal = (rank: number) => ({ 1: '🥇', 2: '🥈', 3: '🥉' }[rank] ?? `${rank}`)

const rankBarClass = (rank: number) => ({
  1: 'bg-cp-primary',
  2: 'bg-cp-info',
  3: 'bg-cp-warning'
}[rank] ?? 'bg-neutral-300')

const reviewOptionClass = (qa: { myAnswerIndex: number, correctAnswerIndex: number }, optionIndex: number) => {
  if (optionIndex === qa.correctAnswerIndex) return 'border-cp-success bg-cp-success-bg text-[#2e7d32]'
  if (optionIndex === qa.myAnswerIndex) return 'border-cp-danger bg-cp-danger-bg text-cp-danger'
  return 'border-cp-border text-cp-text-secondary'
}
</script>

<script lang="ts">
export default {
  name: 'HistoryDetailPage'
}
</script>

<style scoped lang="scss">
@keyframes podium-pop {
  from { opacity: 0; transform: translateY(16px) scale(0.85); }
  to { opacity: 1; transform: translateY(0) scale(1); }
}
.podium-item {
  animation: podium-pop 480ms cubic-bezier(0.34, 1.56, 0.64, 1) both;
}

// 用 transform: scaleY 而非 height 做進場動畫：height/width transition 在 flex 版面中
// 每一幀都會觸發整行 reflow，多個項目交錯動畫時會明顯卡頓；scale 走合成層，不觸發版面重算
.podium-bar {
  transform-origin: bottom;
  transform: scaleY(0);
  will-change: transform;
  transition: transform 1s ease-out;
}
.podium-bar--grown {
  transform: scaleY(1);
}

@keyframes row-slide-in {
  from { opacity: 0; transform: translateX(-10px); }
  to { opacity: 1; transform: translateX(0); }
}
.rank-row {
  animation: row-slide-in 380ms ease both;
}

.rank-row__bar {
  opacity: 0.12;
  transform-origin: left;
  transform: scaleX(0);
  will-change: transform;
  transition: transform 1s ease-out;
}
.rank-row__bar--grown {
  transform: scaleX(1);
}

@media (prefers-reduced-motion: reduce) {
  .podium-item,
  .rank-row {
    animation: none;
  }
  .podium-bar,
  .rank-row__bar {
    transition: none;
    transform: none;
  }
}
</style>
