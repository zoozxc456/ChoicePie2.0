<template>
  <div class="max-w-7xl mx-auto px-6 py-8">
    <div class="mb-7">
      <h1 class="text-xl font-bold">
        {{ t('history.title') }}
      </h1>
      <p class="text-sm text-cp-text-secondary mt-1">
        {{ t('history.subtitle') }}
      </p>
    </div>

    <!-- Empty state -->
    <div
      v-if="!games.length"
      class="text-center py-20"
    >
      <p class="text-sm text-neutral-400 mb-4">
        {{ t('history.empty') }}
      </p>
      <NuxtLink to="/library/new">
        <UButton
          color="primary"
          size="lg"
          class="rounded-full px-6 font-bold"
        >
          {{ t('history.emptyCta') }}
        </UButton>
      </NuxtLink>
    </div>

    <!-- Games list -->
    <div
      v-else
      class="grid gap-4 grid-cols-[repeat(auto-fill,minmax(280px,1fr))]"
    >
      <div
        v-for="game in games"
        :key="game.id"
        class="bg-white rounded-2xl border border-cp-border overflow-hidden flex gap-4 p-4"
      >
        <div
          class="shrink-0 w-16 h-16 rounded-xl flex items-center justify-center text-3xl"
          :style="game.coverGradient"
        >
          {{ game.coverEmoji }}
        </div>
        <div class="min-w-0 flex-1">
          <p class="text-sm font-semibold truncate">
            {{ game.quizTitle }}
          </p>
          <p class="text-xs text-neutral-400 mt-1">
            {{ formatDate(game.playedAt) }}
          </p>
          <div class="flex items-center gap-3 mt-2 text-xs text-cp-text-secondary">
            <span>{{ t('history.card.players', { count: game.playerCount }) }}</span>
            <span>{{ t('history.card.questions', { count: game.questionCount }) }}</span>
          </div>
          <p class="text-xs font-semibold text-primary-500 mt-2 truncate">
            🏆 {{ t('history.card.topPlayer', { name: game.topPlayerName, score: game.topPlayerScore }) }}
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { mockHostedGames } from '~/mocks/history'

definePageMeta({ layout: 'content' })

const { t, locale } = useI18n()

const games = ref(mockHostedGames)

const formatDate = (iso: string) =>
  new Date(iso).toLocaleDateString(locale.value === 'en' ? 'en-US' : 'zh-TW', {
    year: 'numeric',
    month: 'short',
    day: 'numeric'
  })
</script>

<script lang="ts">
export default {
  name: 'HistoryPage'
}
</script>

<style scoped lang="scss">
</style>
