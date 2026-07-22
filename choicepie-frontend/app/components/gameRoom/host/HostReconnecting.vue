<template>
  <div class="flex flex-col items-center justify-center py-24 gap-3">
    <UIcon
      name="i-lucide-loader-2"
      class="animate-spin text-4xl text-cp-primary"
    />
    <p class="text-sm text-cp-text-secondary">
      {{ t('host.reconnecting') }}
    </p>
  </div>
</template>

<script lang="ts" setup>
const { t } = useI18n()
</script>

<style scoped lang="scss"></style>
