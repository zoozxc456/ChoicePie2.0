<template>
  <div class="flex flex-col items-center justify-center py-24 gap-3 text-center">
    <p class="text-2xl font-bold">
      {{ t('host.rejoinFailed.title') }}
    </p>
    <p class="text-sm text-cp-text-secondary">
      {{ t('host.rejoinFailed.desc') }}
    </p>
    <UButton
      size="lg"
      color="primary"
      class="mt-2"
      @click="$router.push('/library')"
    >
      {{ t('host.rejoinFailed.backToLibrary') }}
    </UButton>
  </div>
</template>

<script lang="ts" setup>
const { t } = useI18n()
</script>

<style scoped lang="scss"></style>
