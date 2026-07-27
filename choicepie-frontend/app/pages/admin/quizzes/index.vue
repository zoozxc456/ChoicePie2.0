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
        class="relative grid grid-cols-[1fr_112px] rounded-2xl bg-white border border-neutral-200 shadow-sm overflow-hidden transition-colors hover:border-primary-300"
      >
        <NuxtLink
          :to="`/admin/quizzes/${quiz.id}`"
          class="absolute inset-0 z-10"
          :aria-label="quiz.title"
        />

        <div class="p-4 min-w-0 flex items-center justify-between gap-3 pointer-events-none">
          <div class="min-w-0 flex-1">
            <p class="text-lg font-bold truncate">
              {{ quiz.title }}
            </p>
            <p class="text-base text-neutral-400 mt-0.5 truncate">
              {{ quiz.description || t('adminQuizzes.noDescription') }}
            </p>
          </div>

          <div class="relative shrink-0">
            <UButton
              v-if="quiz.status !== 'takendown'"
              size="sm"
              color="error"
              variant="soft"
              class="pointer-events-auto"
              @click="openTakeDownModal(quiz.id)"
            >
              {{ t('adminQuizzes.takeDownAction') }}
            </UButton>
            <UButton
              v-else
              size="sm"
              color="primary"
              variant="soft"
              class="pointer-events-auto"
              :loading="adminQuizStore.isRestoring"
              @click="handleRestore(quiz.id)"
            >
              {{ t('adminQuizzes.restoreAction') }}
            </UButton>
          </div>
        </div>

        <div
          class="relative border-l border-dashed"
          :class="statusBorderClass(quiz.status)"
        >
          <span class="absolute -top-2.75 -left-2.75 w-5.5 h-5.5 rounded-full bg-neutral-100" />
          <span class="absolute -bottom-2.75 -left-2.75 w-5.5 h-5.5 rounded-full bg-neutral-100" />

          <div
            class="h-full flex flex-col items-center justify-center text-center gap-1 px-3 py-3"
            :class="statusStubBgClass(quiz.status)"
          >
            <div class="flex items-center gap-1.5">
              <UIcon
                :name="statusIcon(quiz.status)"
                class="text-sm shrink-0"
                :class="statusTextClass(quiz.status)"
              />
              <span
                class="text-sm font-bold"
                :class="statusTextClass(quiz.status)"
              >
                {{ statusLabel(quiz.status) }}
              </span>
            </div>
          </div>
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

const statusIcon = (status: string) => ({
  draft: 'i-lucide-file-edit',
  published: 'i-lucide-check-circle-2',
  archived: 'i-lucide-archive',
  deleted: 'i-lucide-trash-2',
  takendown: 'i-lucide-shield-off'
}[status] ?? 'i-lucide-circle')

const statusBorderClass = (status: string) => ({
  draft: 'border-neutral-200',
  published: 'border-success-200',
  archived: 'border-warning-200',
  deleted: 'border-neutral-200',
  takendown: 'border-error-200'
}[status] ?? 'border-neutral-200')

const statusStubBgClass = (status: string) => ({
  draft: 'bg-neutral-100',
  published: 'bg-success-50',
  archived: 'bg-warning-50',
  deleted: 'bg-neutral-100',
  takendown: 'bg-error-50'
}[status] ?? 'bg-neutral-100')

const statusTextClass = (status: string) => ({
  draft: 'text-neutral-600',
  published: 'text-success-700',
  archived: 'text-warning-700',
  deleted: 'text-neutral-600',
  takendown: 'text-error-700'
}[status] ?? 'text-neutral-600')

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
