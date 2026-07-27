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
interface Props {
  pageNumber: number
  totalPages: number
}

const props = defineProps<Props>()

const emit = defineEmits<{
  'update:pageNumber': [page: number]
}>()

const { t } = useI18n()

type PageItem = number | 'ellipsis'

// 頁數多時只顯示目前頁附近 ± 1 頁、首尾頁，其餘用刪節號收合，避免頁碼過多擠爆版面。
const pageItems = computed<PageItem[]>(() => {
  const { pageNumber: current, totalPages: total } = props
  const items: PageItem[] = []
  const siblings = 1

  const rangeStart = Math.max(2, current - siblings)
  const rangeEnd = Math.min(total - 1, current + siblings)

  items.push(1)
  if (rangeStart > 2) {
    items.push('ellipsis')
  }
  for (let page = rangeStart; page <= rangeEnd; page++) {
    items.push(page)
  }
  if (rangeEnd < total - 1) {
    items.push('ellipsis')
  }
  if (total > 1) {
    items.push(total)
  }

  return items
})
</script>

<script lang="ts">
export default {
  name: 'AdminPagination'
}
</script>

<style scoped lang="scss"></style>
