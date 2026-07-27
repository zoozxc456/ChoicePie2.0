<template>
  <UInput
    :model-value="modelValue"
    type="text"
    icon="i-lucide-search"
    size="lg"
    class="w-full mb-4"
    :placeholder="placeholder"
    :ui="{ base: 'rounded-xl bg-white text-base' }"
    @update:model-value="handleInput"
  >
    <template
      v-if="modelValue"
      #trailing
    >
      <UButton
        icon="i-lucide-x"
        size="xs"
        color="neutral"
        variant="link"
        :padded="false"
        :aria-label="t('common.search.clear')"
        @click="handleClear"
      />
    </template>
  </UInput>
</template>

<script setup lang="ts">
interface Props {
  modelValue: string
  placeholder?: string
}

defineProps<Props>()

const emit = defineEmits<{
  'update:modelValue': [value: string]
}>()

const { t } = useI18n()

const handleInput = (value: string | number) => {
  emit('update:modelValue', String(value))
}

const handleClear = () => {
  emit('update:modelValue', '')
}
</script>

<script lang="ts">
export default {
  name: 'AdminSearchInput'
}
</script>

<style scoped lang="scss"></style>
