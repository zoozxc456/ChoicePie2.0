<template>
  <div class="bg-white border border-neutral-200 rounded-2xl p-6 flex items-center justify-between gap-5 flex-wrap">
    <div>
      <p
        data-testid="ready-title"
        class="font-semibold text-sm mb-0.5"
      >
        {{ title }}
      </p>
      <p
        data-testid="ready-subtitle"
        class="text-[13px] text-neutral-600"
      >
        {{ subtitle }}
      </p>
    </div>
    <p
      data-testid="ready-count"
      class="text-4xl font-extrabold text-primary-500"
    >
      {{ countLabel }}
    </p>
  </div>
</template>

<script setup lang="ts">
interface Props {
  title: string
  subtitle: string
  countLabel: string
}

defineProps<Props>()
</script>

<script lang="ts">
export default {
  name: 'ReadySummaryCard'
}
</script>

<style scoped lang="scss"></style>
