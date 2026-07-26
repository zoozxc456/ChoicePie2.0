<template>
  <div
    v-if="totalPages > 1"
    class="flex items-center justify-between mt-4"
  >
    <p class="text-xs text-neutral-400">
      {{ t('common.pagination.summary', { current: pageNumber, total: totalPages }) }}
    </p>
    <div class="flex items-center gap-2">
      <UButton
        size="sm"
        color="neutral"
        variant="soft"
        icon="i-lucide-chevron-left"
        :disabled="pageNumber <= 1"
        :aria-label="t('common.pagination.previous')"
        @click="emit('update:pageNumber', pageNumber - 1)"
      />
      <UButton
        size="sm"
        color="neutral"
        variant="soft"
        icon="i-lucide-chevron-right"
        :disabled="pageNumber >= totalPages"
        :aria-label="t('common.pagination.next')"
        @click="emit('update:pageNumber', pageNumber + 1)"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
interface Props {
  pageNumber: number
  totalPages: number
}

defineProps<Props>()

const emit = defineEmits<{
  'update:pageNumber': [page: number]
}>()

const { t } = useI18n()
</script>

<script lang="ts">
export default {
  name: 'AdminPagination'
}
</script>

<style scoped lang="scss"></style>
