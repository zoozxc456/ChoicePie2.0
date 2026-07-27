<template>
  <div class="rounded-2xl bg-white overflow-hidden border border-neutral-200">
    <div
      v-for="(entry, i) in gameStore.rankings"
      :key="entry.nickname"
      class="flex items-center gap-3 px-4 py-3 transition-colors"
      :class="[
        entry.nickname === gameStore.myNickname ? 'bg-primary-50' : '',
        i < gameStore.rankings.length - 1 ? 'border-b border-neutral-200' : ''
      ]"
    >
      <span
        class="w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold shrink-0 text-white"
        :class="rankBadgeClass(entry.rank)"
      >
        {{ entry.rank }}
      </span>
      <span
        class="flex-1 font-medium text-sm"
        :class="entry.nickname === gameStore.myNickname ? 'text-primary-500 font-bold' : ''"
      >
        {{ entry.nickname }}
        <span
          v-if="entry.nickname === gameStore.myNickname"
          class="text-xs ml-1"
        >{{ t('common.you') }}</span>
      </span>
      <span class="font-bold tabular-nums text-sm">{{ entry.score.toLocaleString() }}</span>
    </div>
  </div>
</template>

<script lang="ts" setup>
const { t } = useI18n()
const gameStore = useGameStore()

const rankBadgeClasses = ['bg-primary-500', 'bg-info-500', 'bg-warning-500']

const rankBadgeClass = (rank: number): string =>
  rankBadgeClasses[rank - 1] ?? 'bg-neutral-200 text-neutral-400'
</script>

<style scoped lang="scss"></style>
