<template>
  <div class="rounded-2xl bg-white border border-neutral-200 p-4">
    <p
      data-testid="stat-value"
      class="text-2xl font-extrabold"
    >
      {{ value }}
    </p>
    <p
      data-testid="stat-label"
      class="text-xs text-neutral-500 mt-1"
    >
      {{ label }}
    </p>
  </div>
</template>

<script setup lang="ts">
interface Props {
  value: number
  label: string
}

defineProps<Props>()
</script>

<script lang="ts">
export default {
  name: 'AdminStatCard'
}
</script>

<style scoped lang="scss"></style>
