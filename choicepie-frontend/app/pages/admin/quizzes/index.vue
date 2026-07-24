<template>
  <div class="max-w-4xl mx-auto">
    <h1 class="text-lg font-extrabold mb-4">
      {{ t('adminQuizzes.title') }}
    </h1>

    <input
      v-model="search"
      type="text"
      class="w-full rounded-xl border border-neutral-200 px-4 py-2 text-sm mb-4"
      :placeholder="t('adminQuizzes.searchPlaceholder')"
      @input="handleSearchInput"
    >

    <div
      v-if="adminQuizStore.isLoading"
      class="flex justify-center py-16"
    >
      <UIcon
        name="i-lucide-loader-2"
        class="animate-spin text-3xl text-primary-500"
      />
    </div>

    <p
      v-else-if="!quizzes.length"
      class="text-sm text-neutral-400 text-center py-16"
    >
      {{ t('adminQuizzes.empty') }}
    </p>

    <div
      v-else
      class="flex flex-col gap-3"
    >
      <div
        v-for="quiz in quizzes"
        :key="quiz.id"
        class="rounded-2xl bg-white border border-neutral-200 p-4"
      >
        <div class="flex items-start justify-between gap-3">
          <div class="min-w-0 flex-1">
            <p class="text-sm font-bold truncate">
              {{ quiz.title }}
            </p>
            <p class="text-xs text-neutral-400 mt-0.5">
              {{ t('adminQuizzes.creatorLine', { name: quiz.creatorName }) }}
            </p>
          </div>
          <span
            class="text-[11px] px-2 py-1 rounded-full font-semibold whitespace-nowrap shrink-0"
            :class="statusBadgeClass(quiz.status)"
          >
            {{ statusLabel(quiz.status) }}
          </span>
        </div>

        <div class="flex justify-end mt-3">
          <UButton
            v-if="quiz.status !== 'takendown'"
            size="sm"
            color="error"
            variant="soft"
            @click="openTakeDownModal(quiz.id)"
          >
            {{ t('adminQuizzes.takeDownAction') }}
          </UButton>
          <UButton
            v-else
            size="sm"
            color="primary"
            variant="soft"
            :loading="adminQuizStore.isRestoring"
            @click="handleRestore(quiz.id)"
          >
            {{ t('adminQuizzes.restoreAction') }}
          </UButton>
        </div>
      </div>
    </div>

    <TakeDownQuizModal
      :open="isModalOpen"
      :is-submitting="adminQuizStore.isTakingDown"
      @confirm="handleTakeDown"
      @cancel="closeTakeDownModal"
    />
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['admin-auth'] })

const { t } = useI18n()
const adminQuizStore = useAdminQuizStore()

const search = ref('')
const isModalOpen = ref(false)
const targetQuizId = ref<string | null>(null)
let searchDebounce: ReturnType<typeof setTimeout> | null = null

const quizzes = computed(() => adminQuizStore.quizzes?.items ?? [])

await adminQuizStore.fetchQuizzes()

const handleSearchInput = () => {
  if (searchDebounce) clearTimeout(searchDebounce)
  searchDebounce = setTimeout(() => {
    adminQuizStore.fetchQuizzes({ search: search.value || undefined })
  }, 300)
}

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

const openTakeDownModal = (quizId: string) => {
  targetQuizId.value = quizId
  isModalOpen.value = true
}

const closeTakeDownModal = () => {
  isModalOpen.value = false
  targetQuizId.value = null
}

const handleTakeDown = async (reason: string) => {
  if (!targetQuizId.value) return
  try {
    await adminQuizStore.takeDownQuiz(targetQuizId.value, reason)
    closeTakeDownModal()
  } catch {
    // 錯誤已寫入 adminQuizStore.error，這裡不需要額外處理
  }
}

const handleRestore = async (quizId: string) => {
  try {
    await adminQuizStore.restoreQuiz(quizId)
  } catch {
    // 錯誤已寫入 adminQuizStore.error，這裡不需要額外處理
  }
}
</script>

<script lang="ts">
export default {
  name: 'AdminQuizzesPage'
}
</script>

<style scoped lang="scss"></style>
