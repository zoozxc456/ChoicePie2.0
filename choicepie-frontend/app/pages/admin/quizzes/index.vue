<template>
  <div>
    <h1 class="text-lg font-extrabold mb-4">
      {{ t('adminQuizzes.title') }}
    </h1>

    <AdminSearchInput
      v-model="search"
      :placeholder="t('adminQuizzes.searchPlaceholder')"
    />

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
      class="text-base text-neutral-400 text-center py-16"
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
        class="relative rounded-2xl bg-white border border-neutral-200 p-4 transition-colors hover:border-primary-300 hover:bg-primary-50/40"
      >
        <NuxtLink
          :to="`/admin/quizzes/${quiz.id}`"
          class="absolute inset-0"
          :aria-label="quiz.title"
        />

        <div class="flex items-start justify-between gap-3 pointer-events-none">
          <div class="min-w-0 flex-1">
            <p class="text-lg font-bold truncate">
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

        <div class="relative flex justify-end mt-3">
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

    <AdminPagination
      :page-number="pageNumber"
      :total-pages="totalPages"
      @update:page-number="handlePageChange"
    />

    <AdminTakeDownQuizModal
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
const pageNumber = computed(() => adminQuizStore.quizzes?.pageNumber ?? 1)
const totalPages = computed(() => adminQuizStore.quizzes?.totalPages
  ?? Math.ceil((adminQuizStore.quizzes?.totalCount ?? 0) / (adminQuizStore.quizzes?.pageSize ?? 1)))

await adminQuizStore.fetchQuizzes()

watch(search, () => {
  if (searchDebounce) clearTimeout(searchDebounce)
  searchDebounce = setTimeout(() => {
    adminQuizStore.fetchQuizzes({ search: search.value || undefined, pageNumber: 1 })
  }, 300)
})

const handlePageChange = (page: number) => {
  adminQuizStore.fetchQuizzes({ search: search.value || undefined, pageNumber: page })
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
