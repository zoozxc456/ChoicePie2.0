<template>
  <div class="grid grid-cols-2 gap-4">
    <div
      v-for="(option, i) in options"
      :key="i"
      data-testid="bigscreen-option"
      class="rounded-2xl p-5 flex items-center gap-4 border-2 transition-all duration-500"
      :class="[borderClass(i), backgroundClass(i), correctAnswerIndex === i ? 'scale-[1.02]' : '']"
    >
      <span
        data-testid="option-letter"
        class="w-12 h-12 rounded-xl flex items-center justify-center text-xl font-black shrink-0 text-white"
        :class="OPTION_COLOR_CLASSES[i]"
      >
        {{ OPTION_LETTERS[i] }}
      </span>
      <span class="text-xl font-semibold text-white">{{ option }}</span>
      <UIcon
        v-if="correctAnswerIndex === i"
        data-testid="correct-mark"
        name="i-lucide-check"
        class="ml-auto text-2xl"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
interface Props {
  options: readonly string[]
  /** null 表示尚未揭曉答案 */
  correctAnswerIndex: number | null
}

const props = defineProps<Props>()

const OPTION_LETTERS = ['A', 'B', 'C', 'D']
const OPTION_COLOR_CLASSES = ['bg-cp-danger', 'bg-cp-info', 'bg-cp-primary', 'bg-cp-success']
const OPTION_BORDER_CLASSES = ['border-cp-danger/40', 'border-cp-info/40', 'border-cp-primary/40', 'border-cp-success/40']
const OPTION_BG_CLASSES = ['bg-cp-danger/15', 'bg-cp-info/15', 'bg-cp-primary/15', 'bg-cp-success/15']

const borderClass = (index: number): string => {
  if (props.correctAnswerIndex === null) return OPTION_BORDER_CLASSES[index] ?? 'border-white/20'
  return index === props.correctAnswerIndex ? 'border-cp-success' : 'border-white/10'
}

const backgroundClass = (index: number): string => {
  if (props.correctAnswerIndex === null) return OPTION_BG_CLASSES[index] ?? 'bg-white/10'
  return index === props.correctAnswerIndex ? 'bg-cp-success/25' : 'bg-white/[0.03]'
}
</script>

<script lang="ts">
export default {
  name: 'BigscreenOptionGrid'
}
</script>

<style scoped lang="scss"></style>
