<template>
  <NuxtLink
    :to="`/history/${session.id}`"
    class="history-card relative bg-white rounded-2xl border border-cp-border overflow-hidden flex gap-4 p-4"
  >
    <span
      data-testid="my-rank-badge"
      class="absolute top-3 right-3 w-9 h-9 rounded-full flex items-center justify-center text-xs font-black text-white shrink-0"
      :class="[rankColorClass(myRank, 'bg-neutral-400'), rankGlowClass(myRank)]"
    >
      <UIcon
        v-if="myRank >= 1 && myRank <= 3"
        name="i-lucide-medal"
        class="text-base"
      />
      <template v-else>#{{ myRank }}</template>
    </span>
    <QuizCoverThumbnail
      :cover-image-url="session.coverImageUrl"
      :cover-emoji="session.coverEmoji"
      :cover-gradient="session.coverGradient"
      size="md"
    />
    <div class="min-w-0 flex-1 pr-8">
      <p class="text-sm font-bold truncate">
        {{ session.quizTitle }}
      </p>
      <p class="text-xs text-cp-text-muted mt-1">
        {{ formatLocaleDate(session.playedAtUtc, locale, 'medium') }}
      </p>
      <div class="flex items-center gap-3 mt-2 text-xs text-cp-text-secondary">
        <span>{{ t('history.player.card.players', { count: session.playerCount }) }}</span>
        <span>{{ t('history.player.card.questions', { count: session.questionCount }) }}</span>
      </div>
      <p class="text-xs font-bold text-primary-500 mt-2">
        {{ t('history.player.card.myRank', { rank: myRank }) }} · {{ t('history.player.card.myScore', { score: session.myScore ?? 0 }) }}
      </p>
    </div>
  </NuxtLink>
</template>

<script setup lang="ts">
import QuizCoverThumbnail from '~/components/library/QuizCoverThumbnail.vue'
import type { GameSessionSummaryDto } from '~/types/api'
import { rankColorClass, rankGlowClass } from '~/utils/podium'

interface Props {
  session: GameSessionSummaryDto
}

const props = defineProps<Props>()

const { t, locale } = useI18n()

const myRank = computed(() => props.session.myRank ?? 0)
</script>

<script lang="ts">
export default {
  name: 'PlayedGameCard'
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
