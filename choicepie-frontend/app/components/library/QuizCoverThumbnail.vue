<template>
  <div
    class="flex items-center justify-center overflow-hidden shrink-0"
    :class="[sizeClass, roundedClass, !coverImageUrl && colorClass]"
  >
    <img
      v-if="coverImageUrl"
      :src="coverImageUrl"
      class="w-full h-full object-cover"
      alt=""
    >
    <span
      v-else
      :class="emojiSizeClass"
    >{{ coverEmoji }}</span>
  </div>
</template>

<script setup lang="ts">
interface Props {
  coverImageUrl?: string | null
  coverEmoji: string
  coverGradient: string
  size?: 'xs' | 'sm' | 'md' | 'lg' | 'xl' | 'full'
  rounded?: 'lg' | 'xl' | '2xl' | 'full' | 'none'
}

const props = withDefaults(defineProps<Props>(), { coverImageUrl: null, size: 'md', rounded: 'xl' })

const COLOR_CLASS_MAP: Record<string, string> = {
  primary: 'bg-cp-primary',
  secondary: 'bg-cp-secondary',
  success: 'bg-cp-success',
  danger: 'bg-cp-danger',
  warning: 'bg-cp-warning',
  info: 'bg-cp-info'
}
const colorClass = computed(() => COLOR_CLASS_MAP[props.coverGradient] ?? 'bg-cp-secondary')

const SIZE_CLASS_MAP: Record<NonNullable<Props['size']>, string> = {
  xs: 'w-9 h-9',
  sm: 'w-12 h-12',
  md: 'w-16 h-16',
  lg: 'w-20 h-20',
  xl: 'w-48 h-48',
  full: 'w-full h-full'
}
const sizeClass = computed(() => SIZE_CLASS_MAP[props.size])

const EMOJI_SIZE_CLASS_MAP: Record<NonNullable<Props['size']>, string> = {
  xs: 'text-lg',
  sm: 'text-2xl',
  md: 'text-3xl',
  lg: 'text-4xl',
  xl: 'text-7xl',
  full: 'text-5xl'
}
const emojiSizeClass = computed(() => EMOJI_SIZE_CLASS_MAP[props.size])

const roundedClass = computed(() => props.rounded === 'none' ? '' : `rounded-${props.rounded}`)
</script>

<style scoped lang="scss"></style>
