<template>
  <div
    role="status"
    aria-busy="true"
    class="flex justify-center"
    :class="SIZE_CLASSES[size].wrapper"
  >
    <UIcon
      data-testid="spinner-icon"
      name="i-lucide-loader-2"
      class="animate-spin text-cp-primary"
      :class="SIZE_CLASSES[size].icon"
    />
  </div>
</template>

<script setup lang="ts">
type SpinnerSize = 'sm' | 'md' | 'lg'

interface Props {
  /** lg：一般頁面；md：後台列表；sm：卡片內區塊 */
  size?: SpinnerSize
}

withDefaults(defineProps<Props>(), {
  size: 'lg'
})

const SIZE_CLASSES: Record<SpinnerSize, { wrapper: string, icon: string }> = {
  sm: { wrapper: 'py-8', icon: 'text-2xl' },
  md: { wrapper: 'py-16', icon: 'text-3xl' },
  lg: { wrapper: 'py-20', icon: 'text-4xl' }
}
</script>

<script lang="ts">
export default {
  name: 'LoadingSpinner'
}
</script>

<style scoped lang="scss"></style>
