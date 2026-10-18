<template>
  <div
    v-if="totalPages > 1"
    class="flex items-center justify-center mt-6"
  >
    <nav class="flex items-center gap-1">
      <UButton
        size="sm"
        color="neutral"
        variant="ghost"
        icon="i-lucide-chevron-left"
        class="rounded-lg"
        :disabled="pageNumber <= 1"
        :aria-label="t('common.pagination.previous')"
        @click="emit('update:pageNumber', pageNumber - 1)"
      />

      <template
        v-for="(item, index) in pageItems"
        :key="index"
      >
        <span
          v-if="item === 'ellipsis'"
          class="w-8 text-center text-sm text-neutral-400 select-none"
        >
          …
        </span>
        <UButton
          v-else
          size="sm"
          :color="item === pageNumber ? 'primary' : 'neutral'"
          :variant="item === pageNumber ? 'solid' : 'ghost'"
          class="rounded-lg w-8 justify-center font-semibold"
          :aria-label="t('common.pagination.page', { page: item })"
          :aria-current="item === pageNumber ? 'page' : undefined"
          @click="emit('update:pageNumber', item)"
        >
          {{ item }}
        </UButton>
      </template>

      <UButton
        size="sm"
        color="neutral"
        variant="ghost"
        icon="i-lucide-chevron-right"
        class="rounded-lg"
        :disabled="pageNumber >= totalPages"
        :aria-label="t('common.pagination.next')"
        @click="emit('update:pageNumber', pageNumber + 1)"
      />
    </nav>
  </div>
</template>

<script setup lang="ts">
import { buildPageItems } from '~/utils/pagination'

interface Props {
  pageNumber: number
  totalPages: number
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'update:pageNumber': [page: number]
}>()

const { t } = useI18n()

const pageItems = computed(() => buildPageItems(props.pageNumber, props.totalPages))
</script>

<script lang="ts">
export default {
  name: 'AdminPagination'
}
</script>

<style scoped lang="scss"></style>
