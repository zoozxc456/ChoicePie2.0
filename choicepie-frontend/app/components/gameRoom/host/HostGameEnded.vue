<template>
  <div class="text-center mb-8">
    <h1 class="text-3xl font-bold mb-2">
      {{ t('host.ended.title') }}
    </h1>
    <p class="text-cp-text-secondary">
      {{ t('host.ended.finalRank') }}
    </p>
  </div>

  <ResultPodium
    :rankings="gameStore.rankings"
    class="mb-8"
  />

  <!-- Full list -->
  <div class="rounded-2xl bg-white overflow-hidden mb-6 border border-cp-border">
    <div
      v-for="(entry, i) in gameStore.rankings"
      :key="entry.nickname"
      class="flex items-center gap-4 px-5 py-3"
      :class="i < gameStore.rankings.length - 1 ? 'border-b border-cp-border' : ''"
    >
      <span class="w-8 text-center font-bold text-cp-text-muted">{{ entry.rank }}</span>
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

<script lang="ts" setup>
import ResultPodium from '~/components/common/ResultPodium.vue'

const { t } = useI18n()
const gameStore = useGameStore()
</script>

<style scoped lang="scss"></style>
