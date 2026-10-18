<template>
  <div>
    <h1 class="text-lg font-extrabold mb-4">
      {{ t('adminQuizzes.title') }}
    </h1>

    <AdminSearchInput
      v-model="search"
      :placeholder="t('adminQuizzes.searchPlaceholder')"
    />

    <LoadingSpinner
      v-if="adminQuizStore.isLoading"
      size="md"
    />

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
      <AdminListRow
        v-for="quiz in quizzes"
        :key="quiz.id"
        :to="`/admin/quizzes/${quiz.id}`"
        :aria-label="quiz.title"
      >
        <p class="text-lg font-bold truncate">
          {{ quiz.title }}
        </p>
        <p class="text-base text-neutral-400 mt-0.5 truncate">
          {{ quiz.description || t('adminQuizzes.noDescription') }}
        </p>
        <template #action>
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
        </template>
        <template #stub>
          <AdminStatusStub
            v-bind="adminQuizStatusStyle(quiz.status)"
            :label="statusLabel(quiz.status)"
          />
        </template>
      </AdminListRow>
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
import AdminListRow from '~/components/admin/AdminListRow.vue'
import LoadingSpinner from '~/components/common/LoadingSpinner.vue'
import { useDebouncedSearch } from '~/composables/useDebouncedSearch'
import { useModalTarget } from '~/composables/useModalTarget'
import { adminQuizStatusLabelKey, adminQuizStatusStyle } from '~/utils/adminQuizStatus'
import { totalPagesOf } from '~/utils/pagination'

definePageMeta({ layout: 'admin', middleware: ['admin-auth'] })

const { t } = useI18n()
const adminQuizStore = useAdminQuizStore()

const {
  isOpen: isModalOpen,
  target: targetQuizId,
  open: openTakeDownModal,
  close: closeTakeDownModal
} = useModalTarget<string>()

const quizzes = computed(() => adminQuizStore.quizzes?.items ?? [])
const pageNumber = computed(() => adminQuizStore.quizzes?.pageNumber ?? 1)
const totalPages = computed(() => totalPagesOf(adminQuizStore.quizzes))

await adminQuizStore.fetchQuizzes()

const { search } = useDebouncedSearch((term) => {
  adminQuizStore.fetchQuizzes({ search: term, pageNumber: 1 })
})

const handlePageChange = (page: number) => {
  adminQuizStore.fetchQuizzes({ search: search.value || undefined, pageNumber: page })
}

const statusLabel = (status: string) => {
  const key = adminQuizStatusLabelKey(status)
  return key ? t(key) : status
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
