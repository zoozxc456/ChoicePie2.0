<template>
  <div>
    <NuxtLink
      to="/admin/quizzes"
      class="inline-flex items-center gap-1 text-base text-neutral-500 hover:text-neutral-800 mb-4"
    >
      <UIcon name="i-lucide-arrow-left" />
      {{ t('adminQuizDetail.back') }}
    </NuxtLink>

    <div
      v-if="adminQuizStore.isLoadingDetail"
      class="flex justify-center py-16"
    >
      <UIcon
        name="i-lucide-loader-2"
        class="animate-spin text-3xl text-primary-500"
      />
    </div>

    <p
      v-else-if="!quiz"
      class="text-base text-neutral-400 text-center py-16"
    >
      {{ t('adminQuizDetail.notFound') }}
    </p>

    <div
      v-else
      class="flex flex-col gap-4"
    >
      <AdminQuizDetailCard
        :quiz="quiz"
        :is-restoring="adminQuizStore.isRestoring"
        @take-down="isModalOpen = true"
        @restore="handleRestore"
      />

      <UTabs
        v-model="activeTab"
        :items="tabItems"
        :ui="{ list: 'bg-neutral-200/60' }"
      >
        <template #question>
          <AdminQuizQuestionList :questions="quiz.questions" />
        </template>

        <template #comment>
          <AdminQuizCommentList
            :comments="comments"
            :is-loading="adminQuizStore.isLoadingComments"
          />
        </template>
      </UTabs>
    </div>

    <AdminTakeDownQuizModal
      :open="isModalOpen"
      :is-submitting="adminQuizStore.isTakingDown"
      @confirm="handleTakeDown"
      @cancel="isModalOpen = false"
    />
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['admin-auth'] })

const { t } = useI18n()
const route = useRoute()
const adminQuizStore = useAdminQuizStore()

const quizId = route.params.id as string
const isModalOpen = ref(false)

const quiz = computed(() => adminQuizStore.currentQuiz)
const comments = computed(() => adminQuizStore.comments?.items ?? [])

const activeTab = ref('question')
const tabItems = computed(() => [
  { label: t('adminQuizDetail.questionTab'), value: 'question', slot: 'question' as const },
  { label: t('adminQuizDetail.commentTab'), value: 'comment', slot: 'comment' as const }
])

await Promise.all([
  adminQuizStore.fetchQuizById(quizId),
  adminQuizStore.fetchQuizComments(quizId)
])

const handleTakeDown = async (reason: string) => {
  try {
    await adminQuizStore.takeDownQuiz(quizId, reason)
    isModalOpen.value = false
  } catch {
    // 錯誤已寫入 adminQuizStore.error，這裡不需要額外處理
  }
}

const handleRestore = async () => {
  try {
    await adminQuizStore.restoreQuiz(quizId)
  } catch {
    // 錯誤已寫入 adminQuizStore.error，這裡不需要額外處理
  }
}
</script>

<script lang="ts">
export default {
  name: 'AdminQuizDetailPage'
}
</script>

<style scoped lang="scss"></style>
