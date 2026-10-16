<template>
  <button
    type="button"
    class="w-full h-12 rounded-2xl border-[1.5px] border-dashed border-cp-border hover:bg-cp-surface-muted text-cp-text-secondary font-semibold text-sm disabled:opacity-60"
    :disabled="disabled"
    @click="emit('add')"
  >
    ＋ {{ label }}
  </button>
</template>

<script setup lang="ts">
interface Props {
  label: string
  disabled?: boolean
}

withDefaults(defineProps<Props>(), {
  disabled: false
})

const emit = defineEmits<{
  add: []
}>()
</script>

<script lang="ts">
export default {
  name: 'AddQuestionButton'
}
</script>

<style scoped lang="scss"></style>
