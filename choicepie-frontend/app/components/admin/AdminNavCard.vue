<template>
  <NuxtLink
    :to="to"
    class="relative rounded-2xl bg-white border border-neutral-200 p-5 flex flex-col gap-3 hover:border-primary-300 hover:shadow-cp-md transition"
  >
    <span
      v-if="count !== null"
      data-testid="nav-count"
      class="absolute top-3 right-3 text-[11px] px-2 py-0.5 rounded-full font-bold"
      :class="highlight ? 'bg-warning-100 text-warning-800' : 'bg-neutral-100 text-neutral-600'"
    >
      {{ count }}
    </span>
    <UIcon
      :name="icon"
      class="text-2xl text-primary-500"
    />
    <span class="font-bold text-base">{{ label }}</span>
  </NuxtLink>
</template>

<script setup lang="ts">
interface Props {
  to: string
  icon: string
  label: string
  /** null 表示資料尚未載入，不顯示徽章 */
  count: number | null
  highlight?: boolean
}

withDefaults(defineProps<Props>(), {
  highlight: false
})
</script>

<script lang="ts">
export default {
  name: 'AdminNavCard'
}
</script>

<style scoped lang="scss"></style>
