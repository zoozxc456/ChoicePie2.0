<template>
  <UButton
    type="button"
    block
    size="lg"
    color="neutral"
    variant="outline"
    class="font-bold h-12 rounded-2xl"
    :loading="loading"
    @click="emit('click')"
  >
    <template #leading>
      <span class="font-extrabold text-[#4285F4]">G</span>
    </template>
    {{ label }}
  </UButton>
</template>

<script setup lang="ts">
interface Props {
  label: string
  loading?: boolean
}

withDefaults(defineProps<Props>(), {
  loading: false
})

const emit = defineEmits<{
  click: []
}>()
</script>

<script lang="ts">
export default {
  name: 'GoogleAuthButton'
}
</script>

<style scoped lang="scss"></style>
