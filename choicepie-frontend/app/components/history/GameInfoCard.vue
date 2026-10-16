<template>
  <div class="rounded-2xl bg-white border border-cp-border p-6 text-center">
    <QuizCoverThumbnail
      :cover-image-url="session.coverImageUrl"
      :cover-emoji="session.coverEmoji"
      :cover-gradient="session.coverGradient"
      size="lg"
      rounded="2xl"
      class="mx-auto mb-4"
    />
    <span
      data-testid="role-badge"
      class="inline-block text-xs font-bold px-3 py-1 rounded-full mb-3"
      :class="session.isHost ? 'bg-cp-primary-light text-cp-primary' : 'bg-cp-info-bg text-cp-info'"
    >
      {{ session.isHost ? t('history.detail.hostBadge') : t('history.detail.playerBadge') }}
    </span>
    <h1 class="text-lg font-black mb-3">
      {{ session.quizTitle }}
    </h1>
    <div
      data-testid="game-meta"
      class="flex items-center justify-center gap-3 text-sm text-cp-text-secondary"
    >
      <span>{{ t('history.detail.meta.players', { count: session.playerCount }) }}</span>
      <span>·</span>
      <span>{{ t('history.detail.meta.questions', { count: session.questionCount }) }}</span>
    </div>
    <p class="text-xs text-cp-text-muted mt-1.5">
      {{ t('history.detail.meta.playedAt', { date: formatLocaleDate(session.playedAtUtc, locale, 'medium') }) }}
    </p>
  </div>
</template>

<script setup lang="ts">
import QuizCoverThumbnail from '~/components/library/QuizCoverThumbnail.vue'
import type { GameSessionDetailDto } from '~/types/api'

interface Props {
  session: GameSessionDetailDto
}

defineProps<Props>()

const { t, locale } = useI18n()
</script>

<script lang="ts">
export default {
  name: 'GameInfoCard'
}
</script>

<style scoped lang="scss"></style>
