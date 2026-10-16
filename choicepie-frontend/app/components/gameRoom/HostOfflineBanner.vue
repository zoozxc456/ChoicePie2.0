<template>
  <div
    v-if="!gameStore.isHostOnline"
    class="fixed inset-x-0 top-0 z-[60] flex items-center justify-center gap-3 px-4 py-3 bg-cp-warning text-white font-semibold text-center"
    :class="large ? 'text-2xl py-5' : 'text-sm'"
    role="status"
    aria-live="polite"
  >
    <UIcon
      name="i-lucide-wifi-off"
      class="shrink-0"
    />
    <span>{{ t('common.hostOffline') }}</span>
  </div>
</template>

<script setup lang="ts">
defineProps<{ large?: boolean }>()

const { t } = useI18n()
const gameStore = useGameStore()
</script>
