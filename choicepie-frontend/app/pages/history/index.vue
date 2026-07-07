<template>
  <div class="max-w-7xl mx-auto px-6 py-8">
    <div class="mb-7 text-center">
      <h1 class="text-2xl font-black">
        {{ t('history.title') }}
      </h1>
      <p class="text-sm text-cp-text-secondary mt-1.5">
        {{ t('history.subtitle') }}
      </p>
    </div>

    <!-- Role Toggle -->
    <div class="flex justify-center mb-10">
      <div class="flex rounded-xl p-1 gap-1 bg-cp-surface-muted border border-cp-border">
        <button
          v-for="role in roles"
          :key="role.key"
          class="px-6 py-2 rounded-lg text-sm font-bold transition-all"
          :class="activeRole === role.key
            ? 'bg-white text-cp-primary shadow-cp-sm'
            : 'text-cp-text-secondary'"
          @click="activeRole = role.key"
        >
          {{ role.label }}
        </button>
      </div>
    </div>

    <!-- Host history -->
    <template v-if="activeRole === 'host'">
      <div
        v-if="!hostedGames.length"
        class="empty-state text-center py-20"
      >
        <div class="text-6xl mb-4 wobble-pie">
          🥧
        </div>
        <p class="font-bold text-base mb-1.5">
          {{ t('history.host.empty') }}
        </p>
        <p class="text-sm text-cp-text-secondary mb-6">
          {{ t('history.host.emptyDesc') }}
        </p>
        <NuxtLink to="/library/new">
          <UButton
            color="primary"
            size="lg"
            class="rounded-full px-6 font-bold"
          >
            {{ t('history.host.emptyCta') }}
          </UButton>
        </NuxtLink>
      </div>

      <div
        v-else
        class="grid gap-4 grid-cols-[repeat(auto-fill,minmax(300px,1fr))]"
      >
        <div
          v-for="game in hostedGames"
          :key="game.id"
          class="history-card bg-white rounded-2xl border border-cp-border overflow-hidden flex gap-4 p-4"
        >
          <div
            class="shrink-0 w-16 h-16 rounded-xl flex items-center justify-center text-3xl"
            :style="game.coverGradient"
          >
            {{ game.coverEmoji }}
          </div>
          <div class="min-w-0 flex-1">
            <p class="text-sm font-bold truncate">
              {{ game.quizTitle }}
            </p>
            <p class="text-xs text-cp-text-muted mt-1">
              {{ formatDate(game.playedAt) }}
            </p>
            <div class="flex items-center gap-3 mt-2 text-xs text-cp-text-secondary">
              <span>{{ t('history.host.card.players', { count: game.playerCount }) }}</span>
              <span>{{ t('history.host.card.questions', { count: game.questionCount }) }}</span>
            </div>
            <p class="text-xs font-bold text-primary-500 mt-2 truncate">
              {{ t('history.host.card.topPlayer', { name: game.topPlayerName, score: game.topPlayerScore }) }}
            </p>
          </div>
        </div>
      </div>
    </template>

    <!-- Player history -->
    <template v-else>
      <div
        v-if="!playedGames.length"
        class="empty-state text-center py-20"
      >
        <div class="text-6xl mb-4 wobble-pie">
          🥧
        </div>
        <p class="font-bold text-base mb-1.5">
          {{ t('history.player.empty') }}
        </p>
        <p class="text-sm text-cp-text-secondary mb-6">
          {{ t('history.player.emptyDesc') }}
        </p>
        <NuxtLink to="/join">
          <UButton
            color="primary"
            size="lg"
            class="rounded-full px-6 font-bold"
          >
            {{ t('history.player.emptyCta') }}
          </UButton>
        </NuxtLink>
      </div>

      <div
        v-else
        class="grid gap-4 grid-cols-[repeat(auto-fill,minmax(300px,1fr))]"
      >
        <div
          v-for="game in playedGames"
          :key="game.id"
          class="history-card relative bg-white rounded-2xl border border-cp-border overflow-hidden flex gap-4 p-4"
        >
          <span
            class="absolute top-3 right-3 w-9 h-9 rounded-full flex items-center justify-center text-xs font-black text-white shrink-0"
            :class="[rankBadgeClass(game.myRank), rankGlowClass(game.myRank)]"
          >
            {{ game.myRank <= 3 ? rankMedal(game.myRank) : `#${game.myRank}` }}
          </span>
          <div
            class="shrink-0 w-16 h-16 rounded-xl flex items-center justify-center text-3xl"
            :style="game.coverGradient"
          >
            {{ game.coverEmoji }}
          </div>
          <div class="min-w-0 flex-1 pr-8">
            <p class="text-sm font-bold truncate">
              {{ game.quizTitle }}
            </p>
            <p class="text-xs text-cp-text-muted mt-1">
              {{ formatDate(game.playedAt) }}
            </p>
            <div class="flex items-center gap-3 mt-2 text-xs text-cp-text-secondary">
              <span>{{ t('history.player.card.players', { count: game.playerCount }) }}</span>
              <span>{{ t('history.player.card.questions', { count: game.questionCount }) }}</span>
            </div>
            <p class="text-xs font-bold text-primary-500 mt-2">
              {{ t('history.player.card.myRank', { rank: game.myRank }) }} · {{ t('history.player.card.myScore', { score: game.myScore }) }}
            </p>
          </div>
        </div>
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import { mockHostedGames, mockPlayedGames } from '~/mocks/history'

definePageMeta({ layout: 'content' })

const { t, locale } = useI18n()

type RoleKey = 'host' | 'player'
const activeRole = ref<RoleKey>('host')

const roles = computed(() => [
  { key: 'host' as RoleKey, label: t('history.roles.host') },
  { key: 'player' as RoleKey, label: t('history.roles.player') }
])

const hostedGames = ref(mockHostedGames)
const playedGames = ref(mockPlayedGames)

const formatDate = (iso: string) =>
  new Date(iso).toLocaleDateString(locale.value === 'en' ? 'en-US' : 'zh-TW', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })

const rankMedal = (rank: number) => ({ 1: '🥇', 2: '🥈', 3: '🥉' }[rank] ?? `#${rank}`)

const rankBadgeClass = (rank: number) => ({
  1: 'bg-cp-primary',
  2: 'bg-cp-info',
  3: 'bg-cp-warning'
}[rank] ?? 'bg-neutral-400')

const rankGlowClass = (rank: number) => ({
  1: 'rank-1',
  2: 'rank-2',
  3: 'rank-3'
}[rank] ?? '')
</script>

<script lang="ts">
export default {
  name: 'HistoryPage'
}
</script>

<style scoped lang="scss">
.history-card {
  transition: transform 180ms ease, box-shadow 180ms ease;
}
.history-card:hover {
  transform: translateY(-2px);
  box-shadow: var(--cp-shadow-lg);
}

@keyframes wobble {
  0%, 100% { transform: rotate(0deg); }
  25% { transform: rotate(-8deg); }
  75% { transform: rotate(8deg); }
}
.wobble-pie {
  display: inline-block;
  animation: wobble 1.8s ease-in-out infinite;
}

@media (prefers-reduced-motion: reduce) {
  .wobble-pie {
    animation: none;
  }
}
</style>
