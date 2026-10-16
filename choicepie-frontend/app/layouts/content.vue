<template>
  <div class="min-h-screen bg-cp-bg">
    <ChoicePieHeader />

    <div class="pt-16">
      <!-- 放在一般流內並 sticky，避免 fixed 時蓋住頁面頂端內容 -->
      <EmailVerificationBanner class="sticky top-16 z-40" />

      <main>
        <slot />
      </main>
    </div>
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
