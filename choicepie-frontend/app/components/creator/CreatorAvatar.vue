<template>
  <img
    v-if="avatar"
    :src="avatar"
    :alt="name"
    class="rounded-full object-cover shrink-0"
    :class="sizeClass"
  >
  <div
    v-else
    class="rounded-full flex items-center justify-center font-bold text-white bg-cp-secondary shrink-0"
    :class="sizeClass"
  >
    {{ name.charAt(0) }}
  </div>
</template>

<script setup lang="ts">
interface Props {
  name: string
  avatar?: string | null
  size?: 'md' | 'lg' | 'xl'
}

const props = withDefaults(defineProps<Props>(), { avatar: null, size: 'md' })

const sizeClass = computed(() => ({
  md: 'w-12 h-12 text-lg',
  lg: 'w-16 h-16 text-xl',
  xl: 'w-24 h-24 text-3xl'
}[props.size]))
</script>

<style scoped lang="scss">
</style>
