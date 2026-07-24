<template>
  <div class="min-h-[calc(100vh-56px)] bg-neutral-50 flex flex-col items-center px-6 py-16">
    <NuxtLink
      to="/"
      class="text-6xl mb-6"
    >
      🥧
    </NuxtLink>

    <div class="w-full max-w-md rounded-2xl bg-white shadow-cp-lg p-7 flex flex-col gap-3.5 text-center">
      <h1 class="text-xl font-extrabold mb-1">
        {{ t('emailVerification.pageTitle') }}
      </h1>

      <UIcon
        v-if="isVerifying"
        name="i-lucide-loader-2"
        class="animate-spin text-3xl text-primary-500 mx-auto"
      />
      <p
        v-else-if="isSuccess"
        class="text-sm text-neutral-600"
      >
        {{ t('emailVerification.success') }}
      </p>
      <p
        v-else
        class="text-sm text-error-500"
      >
        {{ t('emailVerification.failed') }}
      </p>

      <hr class="border-neutral-200">

      <NuxtLink
        to="/library"
        class="text-center text-sm text-neutral-600"
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
