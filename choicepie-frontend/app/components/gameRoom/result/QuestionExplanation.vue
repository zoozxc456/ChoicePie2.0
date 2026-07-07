<template>
  <div
    v-if="gameStore.currentExplanation"
    class="rounded-2xl p-4 mb-4 explanation-text bg-neutral-100 border border-neutral-200 text-sm leading-relaxed"
  >
    <p class="text-xs font-semibold mb-2 text-neutral-400">
      {{ t('room.result.explanation') }}
    </p>
    {{ gameStore.currentExplanation }}
  </div>
</template>

<script lang="ts" setup>
const { t } = useI18n()
const gameStore = useGameStore()
</script>

<style scoped lang="scss"></style>
