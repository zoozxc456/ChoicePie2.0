<template>
  <div class="flex gap-2">
    <button
      v-for="option in options"
      :key="option.value"
      type="button"
      :aria-pressed="option.value === modelValue"
      class="flex-1 text-center py-2.5 rounded-full text-sm font-bold"
      :class="option.value === modelValue ? ACTIVE_CLASSES[tone] : 'bg-neutral-100 text-neutral-600'"
      @click="emit('update:modelValue', option.value)"
    >
      {{ option.label }}
    </button>
  </div>
</template>

<script setup lang="ts" generic="T extends string | number">
interface ChipOption {
  value: T
  label: string
}

interface Props {
  options: ChipOption[]
  modelValue: T
  tone?: 'primary' | 'secondary'
}

withDefaults(defineProps<Props>(), {
  tone: 'primary'
})

const emit = defineEmits<{
  'update:modelValue': [value: T]
}>()

const ACTIVE_CLASSES = {
  primary: 'bg-primary-500 text-white',
  secondary: 'bg-secondary-800 text-white'
} as const
</script>

<script lang="ts">
export default {
  name: 'ChipPicker'
}
</script>

<style scoped lang="scss"></style>
