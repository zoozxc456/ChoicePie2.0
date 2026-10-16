<template>
  <div class="rounded-2xl bg-white border border-neutral-200 p-5">
    <p class="text-lg font-bold mb-3">
      {{ t('adminMemberDetail.commentsTitle') }}
    </p>
    <LoadingSpinner
      v-if="isLoading"
      size="sm"
    />
    <p
      v-else-if="!comments.length"
      class="text-base text-neutral-400 text-center py-8"
    >
      {{ t('adminMemberDetail.commentsEmpty') }}
    </p>
    <ul
      v-else
      class="flex flex-col divide-y divide-neutral-100"
    >
      <li
        v-for="comment in comments"
        :key="comment.id"
        class="py-3"
      >
        <div class="flex items-baseline gap-2">
          <p class="text-base font-medium truncate">
            {{ comment.quizTitle }}
          </p>
          <p class="text-xs text-neutral-400 shrink-0">
            {{ formatDate(comment.createdAt) }}
          </p>
        </div>
        <p class="text-base text-neutral-600 mt-0.5 whitespace-pre-wrap">
          {{ comment.text }}
        </p>
      </li>
    </ul>
  </div>
</template>

<script setup lang="ts">
import LoadingSpinner from '~/components/common/LoadingSpinner.vue'
import type { AdminMemberCommentDto } from '~/types/api'

interface Props {
  comments: AdminMemberCommentDto[]
  isLoading: boolean
}

defineProps<Props>()

const { t, locale } = useI18n()

const formatDate = (iso: string) => formatLocaleDate(iso, locale.value)
</script>

<script lang="ts">
export default {
  name: 'AdminMemberCommentList'
}
</script>

<style scoped lang="scss"></style>
