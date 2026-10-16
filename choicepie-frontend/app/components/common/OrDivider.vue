<template>
  <div class="flex items-center gap-3 text-xs text-neutral-400">
    <hr class="flex-1 border-neutral-200">
    {{ label }}
    <hr class="flex-1 border-neutral-200">
  </div>
</template>

<script setup lang="ts">
interface Props {
  label: string
}

defineProps<Props>()
</script>

<script lang="ts">
export default {
  name: 'OrDivider'
}
</script>

<style scoped lang="scss"></style>
