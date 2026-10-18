<template>
  <UInput
    v-model="model"
    :type="isVisible ? 'text' : 'password'"
    :placeholder="placeholder"
    size="lg"
    class="w-full"
    :ui="{ base: inputClass }"
  >
    <template
      v-if="showToggle"
      #trailing
    >
      <UButton
        data-testid="password-visibility-toggle"
        color="neutral"
        variant="link"
        size="sm"
        :padded="false"
        :icon="isVisible ? 'i-lucide-eye-off' : 'i-lucide-eye'"
        @click="toggleVisible"
      />
    </template>
  </UInput>
</template>

<script setup lang="ts">
interface Props {
  placeholder?: string
  showToggle?: boolean
  inputClass?: string
}

withDefaults(defineProps<Props>(), {
  placeholder: undefined,
  showToggle: true,
  inputClass: 'bg-neutral-100 h-12 text-sm px-4'
})

const model = defineModel<string>({ required: true })
// 由外部傳入 v-model:visible 時可讓多個密碼欄位共用同一個顯示狀態；未傳入時自行管理
const visible = defineModel<boolean>('visible', { default: undefined })
const localVisible = ref(false)

const isVisible = computed(() => visible.value ?? localVisible.value)

const toggleVisible = () => {
  const next = !isVisible.value
  localVisible.value = next
  visible.value = next
}
</script>

<script lang="ts">
export default {
  name: 'PasswordInput'
}
</script>

<style scoped lang="scss"></style>
