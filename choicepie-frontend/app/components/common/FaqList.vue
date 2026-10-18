<template>
  <div class="flex flex-col gap-3">
    <details
      v-for="item in items"
      :key="item.q"
      class="rounded-xl bg-white overflow-hidden group border border-cp-border transition-colors open:border-cp-primary/40"
    >
      <summary class="flex items-center justify-between px-5 py-4 cursor-pointer font-semibold text-sm select-none list-none">
        {{ item.q }}
        <UIcon
          name="i-lucide-chevron-down"
          class="transition-transform group-open:rotate-180 text-cp-text-muted"
        />
      </summary>
      <div class="px-5 pb-4 text-sm leading-relaxed text-cp-text-secondary border-t border-cp-border">
        <div class="pt-3">
          {{ item.a }}
        </div>
      </div>
    </details>
  </div>
</template>

<script setup lang="ts">
interface FaqItem {
  q: string
  a: string
}

interface Props {
  items: FaqItem[]
}

defineProps<Props>()
</script>

<script lang="ts">
export default {
  name: 'FaqList'
}
</script>

<style scoped lang="scss"></style>
