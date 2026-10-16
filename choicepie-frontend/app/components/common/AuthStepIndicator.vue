<template>
  <div class="flex items-center gap-2 mb-1">
    <template
      v-for="(label, i) in steps"
      :key="label"
    >
      <div class="flex items-center gap-1.5">
        <span
          class="w-5 h-5 rounded-full text-[11px] font-bold flex items-center justify-center shrink-0"
          :class="i <= current
            ? 'bg-primary-500 text-white'
            : 'bg-neutral-100 text-neutral-400'"
        >{{ i + 1 }}</span>
        <span
          class="text-xs font-medium"
          :class="i <= current ? 'text-neutral-900' : 'text-neutral-400'"
        >{{ label }}</span>
      </div>
      <div
        v-if="i < steps.length - 1"
        class="flex-1 h-px"
        :class="i < current ? 'bg-primary-500' : 'bg-neutral-200'"
      />
    </template>
  </div>
</template>

<script setup lang="ts">
interface Props {
  steps: string[]
  current: number
}

defineProps<Props>()
</script>

<style scoped lang="scss"></style>
