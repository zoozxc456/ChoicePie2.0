<template>
  <div class="min-h-screen bg-cp-bg flex flex-col items-center justify-center px-6 py-16 text-center">
    <div class="text-7xl mb-6">
      🥧
    </div>
    <h1 class="text-3xl font-black mb-3">
      {{ statusCode === 404 ? t('error.notFound.title') : t('error.generic.title') }}
    </h1>
    <p class="text-sm text-cp-text-secondary max-w-96 mb-8 leading-relaxed">
      {{ statusCode === 404 ? t('error.notFound.desc') : t('error.generic.desc') }}
    </p>
    <UButton
      size="lg"
      color="primary"
      class="rounded-full px-6 font-bold"
      @click="handleGoHome"
    >
      {{ t('error.backHome') }}
    </UButton>
  </div>
</template>

<script setup lang="ts">
import type { NuxtError } from '#app'

const props = defineProps<{ error: NuxtError }>()

const { t } = useI18n()

const statusCode = computed(() => props.error.statusCode)

const handleGoHome = () => clearError({ redirect: '/' })
</script>

<style scoped lang="scss">
</style>
