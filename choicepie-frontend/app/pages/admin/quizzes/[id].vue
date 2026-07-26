<template>
  <div>
    <NuxtLink
      to="/admin/quizzes"
      class="inline-flex items-center gap-1 text-sm text-neutral-500 hover:text-neutral-800 mb-4"
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
      class="text-sm text-neutral-400 text-center py-16"
    >
      {{ t('adminQuizDetail.notFound') }}
    </p>

    <div
      v-else
      class="flex flex-col gap-4"
    >
      <div class="rounded-2xl bg-white border border-neutral-200 p-6">
        <div class="flex items-start justify-between gap-3">
          <div class="min-w-0 flex-1">
            <p class="text-base font-bold">
              {{ quiz.title }}
            </p>
            <p class="text-sm text-neutral-400 mt-0.5">
              {{ t('adminQuizzes.creatorLine', { name: quiz.creatorName }) }}
            </p>
            <p
              v-if="quiz.description"
              class="text-sm text-neutral-500 mt-2"
            >
              {{ quiz.description }}
            </p>
          </div>
          <span
            class="text-[11px] px-2 py-1 rounded-full font-semibold whitespace-nowrap shrink-0"
            :class="statusBadgeClass(quiz.status)"
          >
            {{ statusLabel(quiz.status) }}
          </span>
        </div>

        <div class="grid grid-cols-2 sm:grid-cols-4 gap-3 mt-6">
          <div class="rounded-xl bg-primary-50 p-4">
            <p class="text-xs text-primary-600 font-medium">
              {{ t('adminQuizDetail.questionCount') }}
            </p>
            <p class="text-xl font-bold text-primary-700 mt-1">
              {{ quiz.questionCount }}
            </p>
          </div>
          <div class="rounded-xl bg-warning-50 p-4">
            <p class="text-xs text-warning-600 font-medium">
              {{ t('adminQuizDetail.challengeCount') }}
            </p>
            <p class="text-xl font-bold text-warning-700 mt-1">
              {{ quiz.challengeCount }}
            </p>
          </div>
          <div class="rounded-xl bg-success-50 p-4">
            <p class="text-xs text-success-600 font-medium">
              {{ t('adminQuizDetail.passRate') }}
            </p>
            <p class="text-xl font-bold text-success-700 mt-1">
              {{ quiz.passRate }}%
            </p>
          </div>
          <div class="rounded-xl bg-error-50 p-4">
            <p class="text-xs text-error-600 font-medium">
              {{ t('adminQuizDetail.favoriteCount') }}
            </p>
            <p class="text-xl font-bold text-error-700 mt-1">
              {{ quiz.favoriteCount }}
            </p>
          </div>
        </div>

        <dl
          v-if="quiz.status === 'takendown'"
          class="mt-6 text-sm"
        >
          <div>
            <dt class="text-neutral-400">
              {{ t('adminQuizzes.takedownReasonLabel') }}
            </dt>
            <dd class="mt-0.5 font-medium text-error-500">
              {{ quiz.takedownReason }}
            </dd>
          </div>
        </dl>

        <div class="flex justify-end mt-6">
          <UButton
            v-if="quiz.status !== 'takendown'"
            size="sm"
            color="error"
            variant="soft"
            @click="isModalOpen = true"
          >
            {{ t('adminQuizzes.takeDownAction') }}
          </UButton>
          <UButton
            v-else
            size="sm"
            color="primary"
            variant="soft"
            :loading="adminQuizStore.isRestoring"
            @click="handleRestore"
          >
            {{ t('adminQuizzes.restoreAction') }}
          </UButton>
        </div>
      </div>

      <UTabs
        v-model="activeTab"
        :items="tabItems"
        :ui="{ list: 'bg-neutral-200/60' }"
      >
        <template #question>
          <div class="flex flex-col gap-4 mt-4">
            <div
              v-for="(question, index) in quiz.questions"
              :key="question.id"
              class="rounded-2xl bg-white border border-neutral-200 p-4"
            >
              <div class="flex items-start gap-3">
                <span class="w-6 h-6 rounded-full bg-primary-500 text-white text-xs font-bold flex items-center justify-center shrink-0">
                  {{ index + 1 }}
                </span>
                <div class="min-w-0 flex-1">
                  <p class="text-sm font-bold">
                    {{ question.text }}
                  </p>
                  <p
                    v-if="question.explanation"
                    class="text-xs text-neutral-400 mt-1"
                  >
                    {{ question.explanation }}
                  </p>
                </div>
              </div>
              <ul class="flex flex-col gap-1.5 mt-3">
                <li
                  v-for="(option, optionIndex) in question.options"
                  :key="optionIndex"
                  class="text-sm px-3 py-1.5 rounded-lg"
                  :class="optionIndex === question.answerIndex
                    ? 'bg-success-100 text-success-800 font-semibold'
                    : 'bg-neutral-100 text-neutral-600'"
                >
                  {{ option }}
                </li>
              </ul>
            </div>
          </div>
        </template>

        <template #comment>
          <div class="rounded-2xl bg-white border border-neutral-200 p-4 mt-4">
            <div
              v-if="adminQuizStore.isLoadingComments"
              class="flex justify-center py-8"
            >
              <UIcon
                name="i-lucide-loader-2"
                class="animate-spin text-2xl text-primary-500"
              />
            </div>

            <p
              v-else-if="!comments.length"
              class="text-sm text-neutral-400 text-center py-8"
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
                    <p class="text-sm font-semibold truncate">
                      {{ comment.userName }}
                    </p>
                    <p class="text-xs text-neutral-400 shrink-0">
                      {{ formatDate(comment.createdAt) }}
                    </p>
                  </div>
                  <p class="text-sm text-neutral-600 mt-0.5 whitespace-pre-wrap">
                    {{ comment.text }}
                  </p>
                </div>
              </li>
            </ul>
          </div>
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

const { t, locale } = useI18n()
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

const formatDate = (iso: string) => new Date(iso).toLocaleDateString(locale.value)

const statusLabel = (status: string) => ({
  draft: t('adminQuizzes.status.draft'),
  published: t('adminQuizzes.status.published'),
  archived: t('adminQuizzes.status.archived'),
  deleted: t('adminQuizzes.status.deleted'),
  takendown: t('adminQuizzes.status.takendown')
}[status] ?? status)

const statusBadgeClass = (status: string) => ({
  draft: 'bg-neutral-100 text-neutral-600',
  published: 'bg-success-100 text-success-800',
  archived: 'bg-warning-100 text-warning-800',
  deleted: 'bg-neutral-100 text-neutral-600',
  takendown: 'bg-error-100 text-error-800'
}[status] ?? 'bg-neutral-100 text-neutral-600')

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
