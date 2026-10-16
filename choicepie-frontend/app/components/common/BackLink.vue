<template>
  <NuxtLink
    :to="to"
    class="inline-flex items-center gap-1.5 text-[13px] font-semibold text-cp-text-secondary hover:text-cp-primary"
  >
    <UIcon
      data-testid="back-link-icon"
      name="i-lucide-arrow-left"
    />
    {{ label }}
  </NuxtLink>
</template>

<script setup lang="ts">
interface Props {
  to: string
  label: string
}

defineProps<Props>()
</script>

<script lang="ts">
export default {
  name: 'BackLink'
}
</script>

<style scoped lang="scss"></style>
