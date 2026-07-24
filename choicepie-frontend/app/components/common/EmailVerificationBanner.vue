<template>
  <div
    v-if="showBanner"
    class="bg-warning-100 text-warning-800 text-sm px-4 py-2 flex items-center justify-center gap-3 flex-wrap"
  >
    <span>{{ t('emailVerification.bannerText') }}</span>
    <button
      class="font-semibold underline disabled:opacity-60"
      :disabled="auth.isResendVerificationLoading || hasResent"
      @click="handleResend"
    >
      {{ hasResent ? t('emailVerification.resent') : t('emailVerification.resendAction') }}
    </button>
    <button
      class="text-warning-600"
      @click="isDismissed = true"
    >
      {{ t('emailVerification.dismiss') }}
    </button>
  </div>
</template>

<script setup lang="ts">
const { t } = useI18n()
const auth = useAuthStore()

const isDismissed = ref(false)
const hasResent = ref(false)

const showBanner = computed(() =>
  auth.isLoggedIn && auth.user && !auth.user.isVerified && !isDismissed.value)

const handleResend = async () => {
  try {
    await auth.resendVerification()
    hasResent.value = true
  } catch {
    // 錯誤已寫入 auth.error，這裡不需要額外處理
  }
}
</script>

<script lang="ts">
export default {
  name: 'EmailVerificationBanner'
}
</script>

<style scoped lang="scss"></style>
