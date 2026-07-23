<template>
  <div class="bg-white border border-neutral-200 rounded-2xl p-5">
    <h2 class="text-base font-bold mb-3">
      {{ t('libraryDetail.comments.title') }}
    </h2>

    <div
      v-if="auth.isLoggedIn"
      class="flex flex-col gap-2 mb-5"
    >
      <UTextarea
        v-model="commentText"
        :placeholder="t('libraryDetail.comments.placeholder')"
        :rows="2"
        autoresize
      />
      <UButton
        class="self-end rounded-xl"
        size="sm"
        color="primary"
        :loading="quizStore.isPostingComment"
        :disabled="!commentText.trim()"
        @click="handleAddComment"
      >
        {{ t('libraryDetail.comments.submit') }}
      </UButton>
    </div>
    <p
      v-else
      class="text-[13px] text-neutral-400 mb-5"
    >
      {{ t('libraryDetail.comments.loginToComment') }}
    </p>

    <p
      v-if="!quizStore.isLoadingComments && quizStore.comments.length === 0"
      class="text-[13px] text-neutral-400"
    >
      {{ t('libraryDetail.comments.empty') }}
    </p>

    <div class="flex flex-col gap-4">
      <CommentItem
        v-for="comment in quizStore.comments"
        :key="comment.id"
        :comment="comment"
        :is-owner="!!auth.user && auth.user.id === comment.userId"
        :is-updating="quizStore.isUpdatingComment"
        :is-deleting="quizStore.isDeletingComment"
        @update="handleUpdateComment"
        @delete="handleDeleteComment"
      />
    </div>
  </div>
</template>

<script lang="ts" setup>
interface Props {
  quizId: string
}

const props = defineProps<Props>()

const { t } = useI18n()
const auth = useAuthStore()
const quizStore = useQuizStore()

const commentText = ref('')

const handleAddComment = async () => {
  if (!commentText.value.trim()) return
  try {
    await quizStore.addComment(props.quizId, commentText.value.trim())
    commentText.value = ''
  } catch {
    // 錯誤已寫入 quizStore.error，這裡不需要額外處理
  }
}

const handleUpdateComment = async (commentId: string, text: string) => {
  try {
    await quizStore.updateComment(props.quizId, commentId, text)
  } catch {
    // 錯誤已寫入 quizStore.error，這裡不需要額外處理
  }
}

const handleDeleteComment = async (commentId: string) => {
  try {
    await quizStore.deleteComment(props.quizId, commentId)
  } catch {
    // 錯誤已寫入 quizStore.error，這裡不需要額外處理
  }
}
</script>

<style scoped lang="scss"></style>
