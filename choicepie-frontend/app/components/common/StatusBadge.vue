<template>
  <span
    class="inline-block text-[11px] px-2.5 py-1 rounded-full font-semibold whitespace-nowrap"
    :class="TONE_CLASSES[tone]"
  >
    <slot />
  </span>
</template>

<script setup lang="ts">
import type { StatusBadgeTone } from '~/utils/statusTone'

interface Props {
  tone?: StatusBadgeTone
}

withDefaults(defineProps<Props>(), {
  tone: 'neutral'
})

const TONE_CLASSES: Record<StatusBadgeTone, string> = {
  neutral: 'bg-neutral-100 text-neutral-600',
  success: 'bg-success-100 text-success-800',
  warning: 'bg-warning-100 text-warning-800',
  error: 'bg-error-100 text-error-800',
  primary: 'bg-cp-primary-light text-cp-primary'
}
</script>

<script lang="ts">
export default {
  name: 'StatusBadge'
}
</script>

<style scoped lang="scss"></style>
