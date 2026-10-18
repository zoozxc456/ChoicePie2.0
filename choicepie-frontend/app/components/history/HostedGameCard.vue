<template>
  <NuxtLink
    :to="`/history/${session.id}`"
    class="history-card bg-white rounded-2xl border border-cp-border overflow-hidden flex gap-4 p-4"
  >
    <QuizCoverThumbnail
      :cover-image-url="session.coverImageUrl"
      :cover-emoji="session.coverEmoji"
      :cover-gradient="session.coverGradient"
      size="md"
    />
    <div class="min-w-0 flex-1">
      <p
        data-testid="game-title"
        class="text-sm font-bold truncate"
      >
        {{ session.quizTitle }}
      </p>
      <p class="text-xs text-cp-text-muted mt-1">
        {{ formatLocaleDate(session.playedAtUtc, locale, 'medium') }}
      </p>
      <div class="flex items-center gap-3 mt-2 text-xs text-cp-text-secondary">
        <span>{{ t('history.host.card.players', { count: session.playerCount }) }}</span>
        <span>{{ t('history.host.card.questions', { count: session.questionCount }) }}</span>
      </div>
      <p
        data-testid="top-player"
        class="text-xs font-bold text-primary-500 mt-2 truncate"
      >
        {{ t('history.host.card.topPlayer', { name: session.topPlayerName, score: session.topPlayerScore }) }}
      </p>
    </div>
  </NuxtLink>
</template>

<script setup lang="ts">
import QuizCoverThumbnail from '~/components/library/QuizCoverThumbnail.vue'
import type { GameSessionSummaryDto } from '~/types/api'

interface Props {
  session: GameSessionSummaryDto
}

defineProps<Props>()

const { t, locale } = useI18n()
</script>

<script lang="ts">
export default {
  name: 'HostedGameCard'
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
</style>
