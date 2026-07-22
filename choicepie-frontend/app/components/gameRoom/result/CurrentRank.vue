<template>
  <div class="rounded-2xl p-4 bg-white text-center border border-neutral-200 mb-4">
    <p class="text-xs font-semibold mb-1 text-neutral-400">
      {{ t('room.result.currentRank') }}
    </p>
    <p class="text-3xl font-black text-primary-500">
      {{ t('room.result.rank', { rank: gameStore.myRank }) }}
    </p>
    <p class="font-bold text-neutral-600">
      {{ gameStore.myScore.toLocaleString() }}
    </p>
  </div>

  <RankingList />
</template>

<script lang="ts" setup>
import RankingList from '~/components/gameRoom/result/RankingList.vue'

const { t } = useI18n()
const gameStore = useGameStore()
</script>

<style scoped lang="scss"></style>
