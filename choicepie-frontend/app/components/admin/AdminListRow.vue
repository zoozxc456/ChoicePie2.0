<template>
  <div class="relative grid grid-cols-[1fr_112px] rounded-2xl bg-white border border-neutral-200 overflow-hidden transition-all hover:border-primary-300 hover:shadow-cp-md">
    <!-- 覆蓋整列的連結，讓整列可點進詳情頁 -->
    <NuxtLink
      data-testid="row-link"
      :to="to"
      class="absolute inset-0 z-10"
      :aria-label="ariaLabel"
    />
    <div class="p-4 min-w-0 flex items-center justify-between gap-3 pointer-events-none">
      <div
        data-testid="row-content"
        class="min-w-0 flex-1"
      >
        <slot />
      </div>
      <!-- 操作按鈕需疊在覆蓋連結之上，否則點擊會被連結攔截而換頁 -->
      <div
        v-if="$slots.action"
        data-testid="row-action"
        class="relative z-20 shrink-0 pointer-events-auto"
      >
        <slot name="action" />
      </div>
    </div>
    <slot name="stub" />
  </div>
</template>

<script setup lang="ts">
interface Props {
  to: string
  ariaLabel: string
}

defineProps<Props>()
</script>

<script lang="ts">
export default {
  name: 'AdminListRow'
}
</script>

<style scoped lang="scss"></style>
