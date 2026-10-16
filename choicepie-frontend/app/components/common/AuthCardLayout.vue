<template>
  <div
    class="flex flex-col items-center px-6 py-16"
    :class="[fullHeight ? 'min-h-screen' : 'min-h-[calc(100vh-56px)]', withBg ? 'bg-neutral-50' : '']"
  >
    <NuxtLink
      :to="logoTo"
      class="text-6xl mb-6"
    >
      🥧
    </NuxtLink>

    <div
      class="w-full max-w-md rounded-2xl bg-white shadow-cp-lg p-7 flex flex-col gap-3.5"
      :class="{ 'text-center': centered }"
    >
      <slot />
    </div>

    <slot name="below" />
  </div>
</template>

<script setup lang="ts">
interface Props {
  logoTo?: string
  centered?: boolean
  fullHeight?: boolean
  withBg?: boolean
}

withDefaults(defineProps<Props>(), {
  logoTo: '/',
  centered: false,
  fullHeight: false,
  withBg: true
})
</script>

<style scoped lang="scss"></style>
