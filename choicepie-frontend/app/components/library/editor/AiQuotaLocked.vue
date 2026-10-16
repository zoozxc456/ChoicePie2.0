<template>
  <div class="bg-white border border-neutral-200 rounded-2xl px-7 py-10 flex flex-col items-center text-center gap-3">
    <UIcon
      name="i-lucide-hourglass"
      class="text-4xl text-neutral-400"
    />
    <p class="text-lg font-bold">
      {{ t('hostNew.ai.locked.title') }}
    </p>
    <p class="text-[13px] text-neutral-600 leading-relaxed whitespace-pre-line">
      {{ t('hostNew.ai.locked.desc') }}
    </p>
    <NuxtLink
      to="/library/new/manual"
      class="mt-2 h-12 px-6 rounded-2xl bg-primary-500 text-white font-bold text-sm flex items-center whitespace-nowrap"
    >
      {{ t('hostNew.ai.locked.switchToManual') }}
    </NuxtLink>
  </div>
</template>

<script setup lang="ts">
const { t } = useI18n()
</script>

<script lang="ts">
export default {
  name: 'AiQuotaLocked'
}
</script>

<style scoped lang="scss"></style>
