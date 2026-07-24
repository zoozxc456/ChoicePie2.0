<template>
  <div
    v-if="quiz"
    class="max-w-5xl mx-auto px-6 pt-6 pb-20"
  >
    <NuxtLink
      to="/library"
      class="text-[13px] text-neutral-600 mb-4 inline-block"
    >
      ← {{ t('libraryDetail.backToLibrary') }}
    </NuxtLink>

    <!-- ── Hero ── -->
    <div class="rounded-t-2xl px-10 pt-10 pb-8 flex gap-8 items-end flex-wrap bg-[linear-gradient(180deg,#1a1a2e_0%,#16213e_60%,#2d3748_100%)]">
      <div
        class="w-48 h-48 rounded-xl flex items-center justify-center text-7xl shrink-0 shadow-[0_16px_48px_rgba(0,0,0,0.4)]"
        :style="quiz.coverGradient"
      >
        {{ quiz.coverEmoji }}
      </div>

      <div class="flex-1 min-w-60 flex flex-col gap-2.5 pb-1">
        <p class="text-xs font-semibold text-white">
          {{ DIFFICULTY_LABEL[quiz.difficulty] }} · {{ quiz.tags[0] }}
        </p>
        <h1 class="text-4xl font-extrabold text-white leading-tight">
          {{ quiz.title }}
        </h1>
        <p class="text-[13px] text-white/60">
          {{ t('libraryDetail.creatorLine', { name: quiz.creatorName }) }} · {{ t('libraryDetail.questions', { count: quiz.questionCount }) }} · {{ t('libraryDetail.challenges', { count: quiz.challengeCount.toLocaleString() }) }}
        </p>
      </div>
    </div>

    <!-- ── Action bar ── -->
    <div class="flex items-center gap-3 px-8 py-5 bg-white rounded-b-2xl mb-6 flex-wrap shadow-cp-md">
      <button
        class="w-14 h-14 rounded-full flex items-center justify-center text-2xl text-white shrink-0 bg-primary-500 transition-transform hover:scale-105"
        @click="isStartModalOpen = true"
      >
        ▶
      </button>

      <button
        class="h-10 px-4 rounded-full text-[13px] font-semibold text-white bg-secondary-800 whitespace-nowrap"
        @click="isStartModalOpen = true"
      >
        {{ t('libraryDetail.useForGame') }}
      </button>
      <button
        class="h-10 px-4 rounded-full text-[13px] font-semibold border border-neutral-200 bg-white whitespace-nowrap"
        :disabled="isStartingAttempt"
        @click="handleSoloPractice"
      >
        {{ t('libraryDetail.soloPractice') }}
      </button>

      <button
        class="h-10 px-4 rounded-full text-[13px] font-semibold border whitespace-nowrap disabled:opacity-60"
        :class="quizStore.isFavorited
          ? 'border-error-200 bg-error-100 text-error-800'
          : 'border-neutral-200 bg-white'"
        :disabled="quizStore.isTogglingFavorite"
        @click="handleToggleFavorite"
      >
        {{ quizStore.isFavorited ? `♥ ${t('libraryDetail.favorite.remove')}` : `♡ ${t('libraryDetail.favorite.add')}` }}
      </button>

      <ShareMenu
        :quiz-id="quiz.id"
        :quiz-title="quiz.title"
      />

      <div class="ml-auto flex gap-2 items-center">
        <button
          v-if="isOwner && quiz.status !== 'Published'"
          class="h-8 px-3.5 rounded-full text-[13px] font-semibold text-white bg-primary-500 whitespace-nowrap disabled:opacity-60"
          :disabled="isTogglingStatus"
          @click="handlePublish"
        >
          {{ t('libraryDetail.status.publishAction') }}
        </button>
        <button
          v-if="isOwner && quiz.status === 'Published'"
          class="h-8 px-3.5 rounded-full text-[13px] font-semibold border border-neutral-200 bg-white whitespace-nowrap disabled:opacity-60"
          :disabled="isTogglingStatus"
          @click="handleUnpublish"
        >
          {{ t('libraryDetail.status.unpublishAction') }}
        </button>
        <span
          v-if="isOwner"
          class="text-[11px] px-2.5 py-1 rounded-full font-semibold whitespace-nowrap"
          :class="statusBadgeClass"
        >
          {{ statusLabel }}
        </span>
        <span class="text-[11px] px-2.5 py-1 rounded-full font-semibold bg-success-100 text-success-800 whitespace-nowrap">
          {{ t('libraryDetail.passRateBadge', { rate: quiz.passRate }) }}
        </span>
        <span
          class="text-[11px] px-2.5 py-1 rounded-full font-semibold whitespace-nowrap"
          :class="difficultyClass"
        >
          {{ DIFFICULTY_LABEL[quiz.difficulty] }}
        </span>
      </div>
    </div>

    <!-- ── Content ── -->
    <div class="grid grid-cols-1 lg:grid-cols-[1fr_320px] gap-6">
      <!-- Left: Question list + Comments -->
      <div class="flex flex-col gap-6">
        <div class="bg-white border border-neutral-200 rounded-2xl p-5">
          <h2 class="text-base font-bold mb-3">
            {{ t('libraryDetail.questionList') }}
          </h2>
          <div class="flex flex-col">
            <div
              v-for="(q, i) in quiz.questions"
              :key="q.id"
              class="flex items-center gap-4 px-3 py-2.5 rounded-xl hover:bg-neutral-100 transition-colors"
            >
              <div class="w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold shrink-0 bg-primary-100 text-primary-500">
                {{ i + 1 }}
              </div>
              <div class="flex-1 text-sm font-medium">
                {{ q.text }}
              </div>
            </div>
          </div>
        </div>

        <!-- Attempt history -->
        <AttemptHistoryList :quiz-id="quizId" />

        <!-- Comments -->
        <CommentList :quiz-id="quizId" />

        <!-- Related quizzes -->
        <div
          v-if="quizStore.relatedQuizzes.length > 0"
          class="bg-white border border-neutral-200 rounded-2xl p-5"
        >
          <h2 class="text-base font-bold mb-3">
            {{ t('libraryDetail.related.title') }}
          </h2>
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <NuxtLink
              v-for="related in quizStore.relatedQuizzes"
              :key="related.id"
              :to="`/library/${related.id}`"
              class="flex items-center gap-3 p-3 rounded-xl border border-neutral-200 hover:bg-neutral-100 transition-colors"
            >
              <div
                class="w-10 h-10 rounded-lg flex items-center justify-center text-xl shrink-0"
                :style="related.coverGradient"
              >
                {{ related.coverEmoji }}
              </div>
              <div class="flex-1 min-w-0">
                <p class="text-sm font-semibold truncate">
                  {{ related.title }}
                </p>
                <p class="text-xs text-neutral-400">
                  {{ t('libraryDetail.questions', { count: related.questionCount }) }} · {{ t('libraryDetail.challenges', { count: related.challengeCount.toLocaleString() }) }}
                </p>
              </div>
            </NuxtLink>
          </div>
        </div>
      </div>

      <!-- Right: Creator -->
      <div class="flex flex-col gap-6">
        <div class="bg-white border border-neutral-200 rounded-2xl p-5 flex flex-col items-center gap-2.5 text-center">
          <div class="w-12 h-12 rounded-full flex items-center justify-center text-lg font-bold text-white bg-secondary-800 shrink-0">
            {{ quiz.creatorName[0] }}
          </div>
          <p class="text-sm font-bold flex items-center gap-1">
            {{ quiz.creatorName }}
          </p>
          <p
            v-if="creatorStore.profile"
            class="text-xs text-neutral-400"
          >
            {{ t('libraryDetail.creator.quizCount', { count: creatorStore.profile.quizCount }) }} ·
            {{ t('libraryDetail.creator.challengeCount', { count: creatorStore.profile.challengeCount }) }}
          </p>
          <button
            v-if="!isOwner && creatorStore.profile"
            class="h-8 px-4 rounded-full text-[13px] font-semibold border whitespace-nowrap disabled:opacity-60"
            :class="creatorStore.profile.isFollowing
              ? 'border-neutral-200 bg-white'
              : 'border-transparent bg-primary-500 text-white'"
            :disabled="creatorStore.isTogglingFollow"
            @click="handleToggleFollow"
          >
            {{ creatorStore.profile.isFollowing ? t('libraryDetail.creator.following') : t('libraryDetail.creator.follow') }}
          </button>
        </div>
      </div>
    </div>

    <!-- ── Start Game Modal ── -->
    <Transition name="fade">
      <div
        v-if="isStartModalOpen"
        class="fixed inset-0 z-50 flex items-center justify-center bg-black/50"
        @click.self="isStartModalOpen = false"
      >
        <div class="rounded-2xl bg-white w-full max-w-md mx-4 overflow-hidden shadow-cp-xl">
          <div class="px-6 pt-6 pb-4">
            <h2 class="text-xl font-bold mb-1">
              {{ t('libraryDetail.modal.title') }}
            </h2>
            <p class="text-sm text-neutral-600">
              {{ t('libraryDetail.modal.subtitle') }}
            </p>
          </div>
          <div class="px-6 pb-6">
            <div class="flex gap-4 items-center p-4 rounded-xl mb-5 bg-neutral-100">
              <div
                class="w-14 h-14 rounded-xl flex items-center justify-center text-2xl shrink-0"
                :style="quiz.coverGradient"
              >
                {{ quiz.coverEmoji }}
              </div>
              <div>
                <p class="font-semibold text-sm mb-1">
                  {{ quiz.title }}
                </p>
                <p class="text-xs text-neutral-600">
                  {{ t('libraryDetail.questions', { count: quiz.questionCount }) }} · {{ DIFFICULTY_LABEL[quiz.difficulty] }} · {{ t('libraryDetail.passRateBadge', { rate: quiz.passRate }) }}
                </p>
              </div>
            </div>

            <p class="text-xs font-semibold mb-3 text-neutral-400 tracking-wide">
              {{ t('libraryDetail.modal.settings') }}
            </p>
            <div class="flex flex-col gap-2 mb-6">
              <div
                v-for="setting in gameSettings"
                :key="setting.label"
                class="flex justify-between items-center p-3 rounded-xl border border-neutral-200"
              >
                <div>
                  <p class="text-sm font-medium">
                    {{ setting.label }}
                  </p>
                  <p class="text-xs text-neutral-400">
                    {{ setting.value }}
                  </p>
                </div>
                <span class="text-success-500 font-bold">✓</span>
              </div>

              <div class="flex justify-between items-center p-3 rounded-xl border border-neutral-200">
                <p class="text-sm font-medium">
                  {{ t('libraryDetail.modal.timeLimit') }}
                </p>
                <div class="flex gap-1.5">
                  <button
                    v-for="option in timeLimitOptions"
                    :key="option"
                    type="button"
                    class="h-8 px-3 rounded-full text-xs font-semibold transition-colors"
                    :class="option === timeLimit
                      ? 'bg-primary-500 text-white'
                      : 'bg-neutral-100 text-neutral-600'"
                    @click="timeLimit = option"
                  >
                    {{ t('libraryDetail.modal.timeLimitSeconds', { seconds: option }) }}
                  </button>
                </div>
              </div>
            </div>

            <UButton
              block
              size="lg"
              color="primary"
              :loading="isCreatingRoom"
              @click="handleCreateRoom"
            >
              {{ t('libraryDetail.modal.createRoom') }}
            </UButton>
            <UButton
              block
              size="md"
              color="neutral"
              variant="ghost"
              class="mt-2"
              @click="isStartModalOpen = false"
            >
              {{ t('libraryDetail.modal.cancel') }}
            </UButton>
          </div>
        </div>
      </div>
    </Transition>
  </div>

  <!-- Loading -->
  <div
    v-else
    class="flex justify-center py-20"
  >
    <UIcon
      name="i-lucide-loader-2"
      class="animate-spin text-4xl text-primary-500"
    />
  </div>
</template>

<script setup lang="ts">
import { DIFFICULTY_LABEL } from '~/types/quiz'

definePageMeta({ layout: 'content' })

const { t } = useI18n()
const route = useRoute()
const quizStore = useQuizStore()
const quizAttemptStore = useQuizAttemptStore()
const creatorStore = useCreatorStore()
const gameRoom = useGameRoom()
const auth = useAuthStore()

const quiz = computed(() => quizStore.currentQuiz)
const isStartModalOpen = ref(false)
const isCreatingRoom = ref(false)
const isStartingAttempt = ref(false)
const isTogglingStatus = ref(false)

const timeLimitOptions = [10, 20, 30, 60] as const
const timeLimit = ref<typeof timeLimitOptions[number]>(20)

const quizId = route.params.id as string
await quizStore.fetchQuizById(quizId)
await quizStore.fetchComments(quizId)
await quizStore.fetchRelatedQuizzes(quizId)
if (quiz.value) {
  await creatorStore.fetchCreatorProfile(quiz.value.creatorId)
}
if (auth.isLoggedIn) {
  await quizStore.fetchFavoriteStatus(quizId)
}

useSeoMeta({
  title: () => quiz.value?.title,
  ogTitle: () => quiz.value?.title,
  description: () => quiz.value?.description ?? undefined,
  ogDescription: () => quiz.value?.description ?? undefined,
  ogType: 'website'
})

const difficultyClass = computed(() => ({
  beginner: 'bg-success-100 text-success-800',
  intermediate: 'bg-warning-100 text-warning-800',
  expert: 'bg-error-100 text-error-800'
}[quiz.value!.difficulty]))

const isOwner = computed(() => !!auth.user && auth.user.id === quiz.value?.creatorId)

const statusLabel = computed(() => ({
  Published: t('libraryDetail.status.published'),
  Draft: t('libraryDetail.status.draft'),
  Archived: t('libraryDetail.status.archived')
}[quiz.value?.status ?? ''] ?? quiz.value?.status))

const statusBadgeClass = computed(() => ({
  Published: 'bg-success-100 text-success-800',
  Draft: 'bg-neutral-100 text-neutral-600',
  Archived: 'bg-warning-100 text-warning-800'
}[quiz.value?.status ?? ''] ?? 'bg-neutral-100 text-neutral-600'))

const handlePublish = async () => {
  if (!quiz.value) return
  isTogglingStatus.value = true
  try {
    await quizStore.publishQuiz(quiz.value.id)
  } finally {
    isTogglingStatus.value = false
  }
}

const handleUnpublish = async () => {
  if (!quiz.value) return
  isTogglingStatus.value = true
  try {
    await quizStore.unpublishQuiz(quiz.value.id)
  } finally {
    isTogglingStatus.value = false
  }
}

const handleCreateRoom = async () => {
  if (!quiz.value) return
  isCreatingRoom.value = true
  try {
    await gameRoom.createRoom({
      quizId: quiz.value.id,
      questionIds: quiz.value.questions.map(q => q.id),
      timeLimit: timeLimit.value
    })
  } catch {
    isCreatingRoom.value = false
  }
}

const handleSoloPractice = async () => {
  if (!quiz.value) return
  const auth = useAuthStore()
  if (!auth.isLoggedIn) {
    await navigateTo(`/login?redirect=${encodeURIComponent(route.fullPath)}`)
    return
  }
  isStartingAttempt.value = true
  try {
    const result = await quizAttemptStore.startAttempt(quiz.value.id)
    await navigateTo(`/attempt/${result.attemptId}`)
  } catch {
    isStartingAttempt.value = false
  }
}

const gameSettings = computed(() => [
  { label: t('libraryDetail.modal.allQuestions'), value: t('libraryDetail.modal.allQuestionsValue') },
  { label: t('libraryDetail.modal.joinMethod'), value: t('libraryDetail.modal.joinMethodValue') }
])

const handleToggleFavorite = async () => {
  if (!quiz.value) return
  if (!auth.isLoggedIn) {
    await navigateTo(`/login?redirect=${encodeURIComponent(route.fullPath)}`)
    return
  }
  try {
    await quizStore.toggleFavorite(quiz.value.id)
  } catch {
    // 錯誤已寫入 quizStore.error，這裡不需要額外處理
  }
}

const handleToggleFollow = async () => {
  if (!quiz.value) return
  if (!auth.isLoggedIn) {
    await navigateTo(`/login?redirect=${encodeURIComponent(route.fullPath)}`)
    return
  }
  try {
    await creatorStore.toggleFollow(quiz.value.creatorId)
  } catch {
    // 錯誤已寫入 creatorStore.error，這裡不需要額外處理
  }
}
</script>

<script lang="ts">
export default {
  name: 'LibraryDetailPage'
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
