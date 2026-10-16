<template>
  <!-- display（hidden / flex）由使用端決定，這裡只定義排列方向 -->
  <div class="flex-col items-center">
    <div class="w-full max-w-sm rounded-2xl p-5 bg-white border border-neutral-200 shadow-cp-lg -rotate-2">
      <div class="flex items-center justify-between mb-3">
        <span class="text-xs font-semibold text-neutral-400">{{ t('join.preview.label') }}</span>
        <span class="text-sm font-bold text-cp-primary">08</span>
      </div>
      <div class="h-1.5 rounded-full bg-neutral-100 mb-4 overflow-hidden">
        <div class="timer-bar urgent w-2/3" />
      </div>
      <p class="text-base font-bold mb-3">
        {{ t('join.preview.question') }}
      </p>
      <div class="flex flex-col gap-2">
        <div
          v-for="option in options"
          :key="option.letter"
          class="option-card p-3! text-sm flex items-center gap-2"
          :class="{ correct: option.correct }"
        >
          <span
            class="w-5 h-5 rounded-full text-xs font-bold flex items-center justify-center shrink-0"
            :class="option.correct ? 'bg-cp-success text-white' : 'bg-neutral-100 text-neutral-500'"
          >{{ option.letter }}</span>
          {{ option.label }}
        </div>
      </div>
    </div>

    <div class="flex flex-wrap gap-2 justify-center mt-5 max-w-sm">
      <div
        v-for="name in PREVIEW_PLAYERS"
        :key="name"
        class="player-chip"
      >
        <div class="online-dot" />
        <span>{{ name }}</span>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
const { t } = useI18n()

const PREVIEW_PLAYERS = ['Amy', 'Ben', 'Cathy', '+15']

const options = computed(() => [
  { letter: 'A', label: t('join.preview.optionA'), correct: false },
  { letter: 'B', label: t('join.preview.optionB'), correct: true },
  { letter: 'C', label: t('join.preview.optionC'), correct: false }
])
</script>

<script lang="ts">
export default {
  name: 'JoinPreviewMock'
}
</script>

<style scoped lang="scss"></style>
