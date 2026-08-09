<template>
  <div class="min-h-[calc(100vh-56px)] bg-neutral-50 flex flex-col items-center justify-center px-6 py-16">
    <div class="w-full max-w-sm flex flex-col items-center text-center gap-4">
      <div
        class="w-20 h-20 rounded-full flex items-center justify-center"
        :class="{
          'bg-primary-100': isVerifying,
          'bg-cp-success-bg': !isVerifying && isSuccess,
          'bg-cp-danger-bg': !isVerifying && !isSuccess
        }"
      >
        <UIcon
          v-if="isVerifying"
          name="i-lucide-loader-2"
          class="animate-spin text-4xl text-primary-500"
        />
        <UIcon
          v-else-if="isSuccess"
          name="i-lucide-check"
          class="text-4xl text-cp-success"
        />
        <UIcon
          v-else
          name="i-lucide-x"
          class="text-4xl text-cp-danger"
        />
      </div>

      <h1 class="text-xl font-extrabold">
        {{ t('emailVerification.pageTitle') }}
      </h1>
      <p
        v-if="isSuccess"
        class="text-sm text-neutral-600"
      >
        {{ t('emailVerification.success') }}
      </p>
      <p
        v-else-if="!isVerifying"
        class="text-sm text-error-500"
      >
        {{ t('emailVerification.failed') }}
      </p>

      <NuxtLink
        to="/library"
        class="mt-2 text-center text-sm font-semibold text-primary-500"
      >
        {{ t('emailVerification.backToLibrary') }}
      </NuxtLink>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'default' })

const { t } = useI18n()
const auth = useAuthStore()
const route = useRoute()

const isVerifying = ref(true)
const isSuccess = ref(false)

const token = computed(() => (route.query.token as string) || '')

onMounted(async () => {
  if (!token.value) {
    isVerifying.value = false
    isSuccess.value = false
    return
  }

  try {
    await auth.verifyEmail(token.value)
    isSuccess.value = true
  } catch {
    isSuccess.value = false
  } finally {
    isVerifying.value = false
  }
})
</script>

<script lang="ts">
export default {
  name: 'VerifyEmailPage'
}
</script>

<style scoped lang="scss"></style>
