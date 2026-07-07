<template>
  <button
    class="flex items-center gap-3 p-4 rounded-2xl text-left transition-all w-full option-card"
    :class="cardClass"
    :disabled="disabled"
    @click="emit('click')"
  >
    <span class="w-8 h-8 rounded-full flex items-center justify-center text-sm font-bold shrink-0 bg-neutral-100">
      {{ letter }}
    </span>
    <span
      class="text-sm font-medium"
      :class="textClass"
    >{{ text }}</span>
  </button>
</template>

<script lang="ts" setup>
export type QuizOptionState = 'default' | 'selected' | 'correct' | 'wrong' | 'disabled'

interface Props {
  letter: string
  text: string
  state?: QuizOptionState
  disabled?: boolean
}

const props = withDefaults(defineProps<Props>(), {
  state: 'default',
  disabled: false
})

const emit = defineEmits<{ click: [] }>()

const cardClass = computed(() => {
  switch (props.state) {
    case 'selected': return 'option-card--selected'
    case 'correct': return 'option-card--correct'
    case 'wrong': return 'option-card--wrong'
    case 'disabled': return 'option-card--disabled'
    default: return ''
  }
})

const textClass = computed(() => {
  switch (props.state) {
    case 'correct': return 'option-text--correct'
    case 'wrong': return 'option-text--wrong'
    default: return 'option-text--default'
  }
})
</script>

<style scoped lang="scss">
.option-card--selected {
  border-color: var(--cp-primary);
  background: var(--cp-primary-light);
}
.option-card--correct {
  border-color: var(--cp-success);
  background: var(--cp-success-bg);
}
.option-card--wrong {
  border-color: var(--cp-danger);
  background: var(--cp-danger-bg);
}
.option-card--disabled {
  opacity: 0.5;
  border-color: var(--cp-border);
}

.option-text--correct {
  color: #2e7d32;
  font-weight: 700;
}
.option-text--wrong {
  color: var(--cp-danger);
}
.option-text--default {
  color: var(--cp-text-primary);
}
</style>
