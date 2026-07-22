<template>
  <div class="min-h-screen bg-cp-bg">
    <ChoicePieHeader />

    <main class="pt-16">
      <slot />
    </main>
  </div>
</template>

<script setup lang="ts">
import ChoicePieHeader from '~/components/common/ChoicePieHeader.vue'
</script>

<script lang="ts">
export default {
  name: 'ContentLayout'
}
</script>

<style scoped lang="scss">
</style>
