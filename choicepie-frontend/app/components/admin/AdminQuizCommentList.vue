<template>
  <div class="rounded-2xl bg-white border border-neutral-200 p-4 mt-4">
    <div
      v-if="isLoading"
      class="flex justify-center py-8"
    >
      <UIcon
        name="i-lucide-loader-2"
        class="animate-spin text-2xl text-primary-500"
      />
    </div>

    <p
      v-else-if="!comments.length"
      class="text-base text-neutral-400 text-center py-8"
    >
      {{ t('adminQuizDetail.commentsEmpty') }}
    </p>

    <ul
      v-else
      class="flex flex-col divide-y divide-neutral-100"
    >
      <li
        v-for="comment in comments"
        :key="comment.id"
        class="py-3 flex items-start gap-3"
      >
        <img
          v-if="comment.userAvatar"
          :src="comment.userAvatar"
          class="w-8 h-8 rounded-full object-cover shrink-0"
          alt=""
        >
        <div
          v-else
          class="w-8 h-8 rounded-full bg-neutral-100 flex items-center justify-center text-xs font-bold text-neutral-400 shrink-0"
        >
          {{ comment.userName.charAt(0) }}
        </div>
        <div class="min-w-0 flex-1">
          <div class="flex items-baseline gap-2">
            <p class="text-base font-semibold truncate">
              {{ comment.userName }}
            </p>
            <p class="text-xs text-neutral-400 shrink-0">
              {{ formatDate(comment.createdAt) }}
            </p>
          </div>
          <p class="text-base text-neutral-600 mt-0.5 whitespace-pre-wrap">
            {{ comment.text }}
          </p>
        </div>
      </li>
    </ul>
  </div>
</template>

<script setup lang="ts">
import type { CommentDto } from '~/types/api'

interface Props {
  comments: CommentDto[]
  isLoading: boolean
}

defineProps<Props>()

const { t, locale } = useI18n()

const formatDate = (iso: string) => new Date(iso).toLocaleDateString(locale.value)
</script>

<script lang="ts">
export default {
  name: 'AdminQuizCommentList'
}
</script>

<style scoped lang="scss"></style>
