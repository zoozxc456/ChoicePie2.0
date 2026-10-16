<template>
  <div>
    <!-- Loading -->
    <div
      v-if="isLoading"
      class="flex justify-center py-20"
    >
      <UIcon
        name="i-lucide-loader-2"
        class="animate-spin text-4xl text-cp-primary"
      />
    </div>

    <!-- Empty -->
    <div
      v-else-if="!quizStore.quizzes.length"
      class="text-center py-20"
    >
      <div class="text-6xl mb-4">
        🥧
      </div>
      <p class="font-bold text-base mb-1.5 text-cp-text-primary">
        {{ t('myQuizzes.empty') }}
      </p>
      <p class="text-sm text-cp-text-secondary mb-6">
        {{ t('myQuizzes.emptyDesc') }}
      </p>
      <NuxtLink to="/library/new">
        <UButton
          color="primary"
          size="lg"
          class="rounded-full px-6 font-bold"
        >
          {{ t('myQuizzes.emptyCta') }}
        </UButton>
      </NuxtLink>
    </div>

    <div
      v-else
      class="grid gap-4 grid-cols-[repeat(auto-fill,minmax(280px,1fr))]"
    >
      <NuxtLink
        v-for="quiz in quizStore.quizzes"
        :key="quiz.id"
        :to="`/library/${quiz.id}`"
      >
        <MineQuizCard
          :quiz="quiz"
          :is-loading="pendingId === quiz.id"
          @publish="handlePublish"
          @unpublish="handleUnpublish"
          @archive="handleArchive"
          @unarchive="handleUnarchive"
          @delete="handleDelete"
        />
      </NuxtLink>
    </div>

    <!-- Delete confirm modal -->
    <Transition name="fade">
      <div
        v-if="confirmDeleteId"
        class="fixed inset-0 z-50 flex items-center justify-center bg-black/50"
        @click.self="confirmDeleteId = null"
      >
        <div class="rounded-2xl bg-cp-surface w-full max-w-sm mx-4 overflow-hidden shadow-cp-xl p-6">
          <h2 class="text-lg font-bold mb-1 text-cp-text-primary">
            {{ t('myQuizzes.deleteConfirm.title') }}
          </h2>
          <p class="text-sm text-cp-text-secondary mb-5">
            {{ t('myQuizzes.deleteConfirm.subtitle') }}
          </p>
          <UButton
            block
            size="lg"
            color="error"
            class="rounded-xl font-bold"
            :loading="pendingId === confirmDeleteId"
            @click="handleDelete"
          >
            {{ t('myQuizzes.deleteConfirm.confirm') }}
          </UButton>
          <UButton
            block
            size="md"
            color="neutral"
            variant="ghost"
            class="mt-2"
            @click="confirmDeleteId = null"
          >
            {{ t('myQuizzes.deleteConfirm.cancel') }}
          </UButton>
        </div>
      </div>
    </Transition>
  </div>
</template>

<script setup lang="ts">
import { useQuizStore } from '~/stores/quiz'
import MineQuizCard from '~/components/library/MineQuizCard.vue'

const { t } = useI18n()
const quizStore = useQuizStore()

const pendingId = ref<string | null>(null)
const confirmDeleteId = ref<string | null>(null)

const { pending: isLoading } = await useAsyncData('library-mine-quizzes', () => quizStore.fetchQuizzes({ mine: true }))

const handlePublish = async (id: string) => {
  pendingId.value = id
  try {
    await quizStore.publishQuiz(id)
    await quizStore.fetchQuizzes({ mine: true })
  } finally {
    pendingId.value = null
  }
}

const handleUnpublish = async (id: string) => {
  pendingId.value = id
  try {
    await quizStore.unpublishQuiz(id)
    await quizStore.fetchQuizzes({ mine: true })
  } finally {
    pendingId.value = null
  }
}

const handleArchive = async (id: string) => {
  pendingId.value = id
  try {
    await quizStore.archiveQuiz(id)
    await quizStore.fetchQuizzes({ mine: true })
  } finally {
    pendingId.value = null
  }
}

const handleUnarchive = async (id: string) => {
  pendingId.value = id
  try {
    await quizStore.unarchiveQuiz(id)
    await quizStore.fetchQuizzes({ mine: true })
  } finally {
    pendingId.value = null
  }
}

const handleDelete = async () => {
  if (!confirmDeleteId.value) return
  pendingId.value = confirmDeleteId.value
  try {
    await quizStore.deleteQuiz(confirmDeleteId.value)
    confirmDeleteId.value = null
  } finally {
    pendingId.value = null
  }
}
</script>

<script lang="ts">
export default {
  name: 'MyQuizzesTab'
}
</script>

<style scoped lang="scss">
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.2s;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
