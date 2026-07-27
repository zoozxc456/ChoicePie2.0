<template>
  <div class="min-h-screen bg-cp-bg">
    <ChoicePieHeader />
    <EmailVerificationBanner class="fixed top-16 inset-x-0 z-40" />

    <main class="pt-16">
      <slot />
    </main>
  </div>
</template>

<script setup lang="ts">
import ChoicePieHeader from '~/components/common/ChoicePieHeader.vue'
import EmailVerificationBanner from '~/components/common/EmailVerificationBanner.vue'
</script>

<script lang="ts">
export default {
  name: 'ContentLayout'
}
</script>

<style scoped lang="scss">
</style>
