<template>
  <div class="text-center mb-6">
    <div class="text-5xl mb-3">
      🏆
    </div>
    <h1 class="text-2xl font-bold mb-1">
      {{ t('room.ended.title') }}
    </h1>
  </div>

  <!-- My result -->
  <div class="rounded-2xl p-5 mb-5 text-center bg-primary-50 border-2 border-primary-200">
    <p class="text-xs font-semibold mb-1 text-primary-500">
      {{ t('room.ended.yourResult') }}
    </p>
    <p class="text-4xl font-black mb-1 text-primary-500">
      {{ t('room.result.rank', { rank: gameStore.myRank }) }}
    </p>
    <p class="text-xl font-bold text-secondary-800">
      {{ gameStore.myScore.toLocaleString() }}
    </p>
  </div>

  <div class="mb-4">
    <RankingList />
  </div>

  <NuxtLink :to="'/'">
    <UButton
      block
      size="lg"
      variant="outline"
    >
      {{ t('room.ended.exit') }}
    </UButton>
  </NuxtLink>
</template>

<script lang="ts" setup>
import RankingList from '~/components/gameRoom/result/RankingList.vue'

const { t } = useI18n()
const gameStore = useGameStore()
</script>

<style scoped lang='scss'></style>
