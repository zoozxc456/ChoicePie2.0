<template>
  <div
    class="inline-flex p-1 gap-1 bg-cp-surface-muted"
    :class="shape === 'pill' ? 'rounded-full' : 'rounded-xl border border-cp-border'"
  >
    <button
      v-for="option in options"
      :key="option.value"
      type="button"
      :aria-pressed="option.value === modelValue"
      class="text-sm font-bold transition-all"
      :class="[
        shape === 'pill' ? 'px-5 py-2.5 rounded-full' : 'px-6 py-2 rounded-lg',
        option.value === modelValue
          ? 'bg-cp-surface text-cp-primary shadow-cp-sm'
          : 'text-cp-text-secondary'
      ]"
      @click="emit('update:modelValue', option.value)"
    >
      {{ option.label }}
    </button>
  </div>
</template>

<script setup lang="ts" generic="T extends string">
interface SegmentedOption {
  value: T
  label: string
}

interface Props {
  options: SegmentedOption[]
  modelValue: T
  /** rounded：角色切換（圓角方框）；pill：分頁（全圓角） */
  shape?: 'rounded' | 'pill'
}

withDefaults(defineProps<Props>(), {
  shape: 'rounded'
})

const emit = defineEmits<{
  'update:modelValue': [value: T]
}>()
</script>

<script lang="ts">
export default {
  name: 'SegmentedControl'
}
</script>

<style scoped lang="scss"></style>
