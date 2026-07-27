<template>
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
const { t } = useI18n()
const gameStore = useGameStore()

// 頒獎台名次視覺（依原始名次 0=金 1=銀 2=銅）
const PODIUM_META = [
  { medal: '🥇', place: 1, colorClass: 'bg-cp-primary', glowClass: 'rank-1', height: '80px' },
  { medal: '🥈', place: 2, colorClass: 'bg-cp-info', glowClass: 'rank-2', height: '60px' },
  { medal: '🥉', place: 3, colorClass: 'bg-cp-warning', glowClass: 'rank-3', height: '40px' }
]
// 版面呈現順序為 銀/金/銅（名次 2/1/3）
const podiumEntries = computed(() =>
  [1, 0, 2]
    .map(rank => ({
      rank,
      entry: gameStore.rankings.find(r => r.rank === rank + 1),
      meta: PODIUM_META[rank]!
    }))
    .filter(p => p.entry)
)
</script>

<style scoped lang="scss"></style>
