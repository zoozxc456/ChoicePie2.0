<template>
  <div
    class="relative border-l border-dashed"
    :class="borderClass"
  >
    <span class="absolute -top-2.75 -left-2.75 w-5.5 h-5.5 rounded-full bg-neutral-100" />
    <span class="absolute -bottom-2.75 -left-2.75 w-5.5 h-5.5 rounded-full bg-neutral-100" />

    <div
      class="h-full flex flex-col items-center justify-center text-center px-3 py-3"
      :class="[bgClass, size === 'lg' ? 'gap-2 px-4 py-5' : 'gap-1']"
    >
      <div class="flex items-center gap-1.5">
        <UIcon
          :name="icon"
          class="shrink-0"
          :class="[textClass, size === 'lg' ? 'text-base' : 'text-sm']"
        />
        <span
          class="font-bold"
          :class="[textClass, size === 'lg' ? 'text-base' : 'text-sm']"
        >
          {{ label }}
        </span>
      </div>

      <slot name="subtext" />

      <slot name="reason" />
    </div>
  </div>
</template>

<script setup lang="ts">
interface Props {
  icon: string
  label: string
  borderClass: string
  bgClass: string
  textClass: string
  size?: 'sm' | 'lg'
}

withDefaults(defineProps<Props>(), {
  size: 'sm'
})
</script>

<script lang="ts">
export default {
  name: 'AdminStatusStub'
}
</script>

<style scoped lang="scss"></style>
