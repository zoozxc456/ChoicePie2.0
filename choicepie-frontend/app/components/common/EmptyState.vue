<template>
  <div class="text-center py-20">
    <UIcon
      v-if="icon"
      data-testid="empty-icon"
      :name="icon"
      class="text-5xl mb-4 text-cp-text-muted"
    />
    <div
      v-else-if="emoji"
      data-testid="empty-emoji"
      class="wobble text-6xl mb-4"
    >
      {{ emoji }}
    </div>
    <p
      data-testid="empty-title"
      class="font-bold text-base mb-1.5 text-cp-text-primary"
    >
      {{ title }}
    </p>
    <p
      v-if="description"
      data-testid="empty-description"
      class="text-sm text-cp-text-secondary mb-6"
    >
      {{ description }}
    </p>
    <div
      v-if="$slots.default"
      data-testid="empty-actions"
    >
      <slot />
    </div>
  </div>
</template>

<script setup lang="ts">
interface Props {
  title: string
  description?: string
  /** Iconify 圖示名稱；與 emoji 擇一 */
  icon?: string
  emoji?: string
}

defineProps<Props>()
</script>

<script lang="ts">
export default {
  name: 'EmptyState'
}
</script>

<style scoped lang="scss">
@keyframes wobble {
  0%, 100% { transform: rotate(0deg); }
  25% { transform: rotate(-8deg); }
  75% { transform: rotate(8deg); }
}
.wobble {
  display: inline-block;
  animation: wobble 1.8s ease-in-out infinite;
}
@media (prefers-reduced-motion: reduce) {
  .wobble {
    animation: none;
  }
}
</style>
