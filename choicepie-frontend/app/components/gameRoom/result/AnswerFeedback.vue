<template>
  <div
    class="rounded-2xl p-5 mb-4 text-center border-2"
    :class="gameStore.isCorrect ? 'bg-success-50 border-success-500' : 'bg-error-50 border-error-500'"
  >
    <div class="text-4xl mb-2">
      {{ gameStore.isCorrect ? '🎉' : '😅' }}
    </div>
    <p
      class="text-xl font-black mb-1"
      :class="gameStore.isCorrect ? 'text-success-500' : 'text-error-500'"
    >
      {{ gameStore.isCorrect ? t('room.result.correct') : t('room.result.wrong') }}
    </p>
    <p
      v-if="gameStore.isCorrect && gameStore.pointsEarned > 0"
      class="font-bold text-lg text-primary-500"
    >
      {{ t('room.result.points', { points: gameStore.pointsEarned.toLocaleString() }) }}
    </p>
  </div>
</template>

<script lang="ts" setup>
const { t } = useI18n()
const gameStore = useGameStore()
</script>

<style scoped lang="scss"></style>
