<template>
  <div
    v-if="quiz"
    class="max-w-5xl mx-auto px-6 pt-6 pb-20"
  >
    <NuxtLink
      to="/library"
      class="text-[13px] text-cp-text-secondary mb-4 inline-block"
    >
      ← {{ t('libraryDetail.backToLibrary') }}
    </NuxtLink>

    <!-- ── Hero ── -->
    <div class="rounded-t-2xl px-10 pt-10 pb-8 flex gap-8 items-end flex-wrap bg-cp-secondary">
      <QuizCoverThumbnail
        :cover-image-url="quiz.coverImageUrl"
        :cover-emoji="quiz.coverEmoji"
        :cover-gradient="quiz.coverGradient"
        size="xl"
      />

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
    <div class="flex items-center gap-3 px-8 py-5 bg-cp-surface rounded-b-2xl mb-6 flex-wrap border border-t-0 border-cp-border">
      <button
        class="w-11 h-11 rounded-full flex items-center justify-center text-lg text-white shrink-0 bg-cp-primary cursor-pointer transition-transform hover:scale-110"
        @click="isStartModalOpen = true"
      >
        <UIcon name="i-lucide-play" />
      </button>

      <button
        v-if="isOwner"
        class="h-10 px-4 rounded-full text-[13px] font-semibold bg-cp-surface-muted whitespace-nowrap cursor-pointer"
        :disabled="isStartingAttempt"
        @click="handleSoloPractice"
      >
        {{ t('libraryDetail.soloPractice') }}
      </button>

      <button
        class="h-10 px-4 rounded-full text-[13px] font-semibold whitespace-nowrap cursor-pointer disabled:opacity-60 inline-flex items-center gap-1.5"
        :class="quizStore.isFavorited
          ? 'bg-error-100 text-error-800'
          : 'bg-cp-surface-muted'"
        :disabled="quizStore.isTogglingFavorite"
        @click="handleToggleFavorite"
      >
        <UIcon
          name="i-lucide-heart"
          :class="{ 'fill-current': quizStore.isFavorited }"
        />
        {{ quizStore.isFavorited ? t('libraryDetail.favorite.remove') : t('libraryDetail.favorite.add') }}
      </button>

      <button
        v-if="isOwner && quiz.status !== 'published'"
        class="h-10 px-4 rounded-full text-[13px] font-semibold text-white bg-cp-primary whitespace-nowrap cursor-pointer disabled:opacity-60 inline-flex items-center gap-1.5"
        :disabled="isTogglingStatus"
        @click="handlePublish"
      >
        <UIcon name="i-lucide-upload" />
        {{ t('libraryDetail.status.publishAction') }}
      </button>
      <button
        v-if="isOwner && quiz.status === 'published'"
        class="h-10 px-4 rounded-full text-[13px] font-semibold bg-cp-surface-muted whitespace-nowrap cursor-pointer disabled:opacity-60 inline-flex items-center gap-1.5"
        :disabled="isTogglingStatus"
        @click="handleUnpublish"
      >
        <UIcon name="i-lucide-eye-off" />
        {{ t('libraryDetail.status.unpublishAction') }}
      </button>

      <ShareMenu
        :quiz-id="quiz.id"
        :quiz-title="quiz.title"
      />

      <button
        v-if="!isOwner && auth.isLoggedIn"
        class="h-10 px-4 rounded-full text-[13px] font-semibold bg-cp-surface-muted whitespace-nowrap cursor-pointer disabled:opacity-60"
        :disabled="quizStore.hasReported"
        @click="isReportModalOpen = true"
      >
        {{ quizStore.hasReported ? t('libraryDetail.report.reported') : t('libraryDetail.report.action') }}
      </button>

      <div class="ml-auto flex gap-2 items-center">
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
        <div
          v-if="isOwner"
          class="bg-cp-surface rounded-2xl border border-cp-border p-5"
        >
          <h2 class="text-base font-bold mb-3 text-cp-text-primary">
            {{ t('libraryDetail.questionList') }}
          </h2>
          <div class="flex flex-col">
            <div
              v-for="(q, i) in quiz.questions"
              :key="q.id"
              class="flex items-center gap-4 px-3 py-2.5 rounded-xl hover:bg-cp-surface-muted transition-colors"
            >
              <div class="w-7 h-7 rounded-full flex items-center justify-center text-xs font-bold shrink-0 bg-cp-primary-light text-cp-primary">
                {{ i + 1 }}
              </div>
              <div class="flex-1 text-sm font-medium text-cp-text-primary">
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
          class="bg-cp-surface rounded-2xl border border-cp-border p-5"
        >
          <h2 class="text-base font-bold mb-3 text-cp-text-primary">
            {{ t('libraryDetail.related.title') }}
          </h2>
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-3">
            <NuxtLink
              v-for="related in quizStore.relatedQuizzes"
              :key="related.id"
              :to="`/library/${related.id}`"
              class="flex items-center gap-3 p-3 rounded-xl hover:bg-cp-surface-muted transition-colors"
            >
              <QuizCoverThumbnail
                :cover-image-url="related.coverImageUrl"
                :cover-emoji="related.coverEmoji"
                :cover-gradient="related.coverGradient"
                size="sm"
                rounded="lg"
              />
              <div class="flex-1 min-w-0">
                <p class="text-sm font-semibold truncate text-cp-text-primary">
                  {{ related.title }}
                </p>
                <p class="text-xs text-cp-text-muted">
                  {{ t('libraryDetail.questions', { count: related.questionCount }) }} · {{ t('libraryDetail.challenges', { count: related.challengeCount.toLocaleString() }) }}
                </p>
              </div>
            </NuxtLink>
          </div>
        </div>
      </div>

      <!-- Right: Creator -->
      <div class="flex flex-col gap-6">
        <div class="bg-cp-surface rounded-2xl border border-cp-border p-5 flex flex-col items-center gap-2.5 text-center">
          <div class="w-12 h-12 rounded-full flex items-center justify-center text-lg font-bold text-white bg-cp-secondary shrink-0">
            {{ quiz.creatorName[0] }}
          </div>
          <p class="text-sm font-bold flex items-center gap-1 text-cp-text-primary">
            {{ quiz.creatorName }}
          </p>
          <p
            v-if="creatorStore.profile"
            class="text-xs text-cp-text-muted"
          >
            {{ t('libraryDetail.creator.quizCount', { count: creatorStore.profile.quizCount }) }} ·
            {{ t('libraryDetail.creator.challengeCount', { count: creatorStore.profile.challengeCount }) }}
          </p>
          <button
            v-if="!isOwner && creatorStore.profile"
            class="h-8 px-4 rounded-full text-[13px] font-semibold whitespace-nowrap cursor-pointer disabled:opacity-60"
            :class="creatorStore.profile.isFollowing
              ? 'bg-cp-surface-muted'
              : 'bg-cp-primary text-white'"
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
        <div class="rounded-2xl bg-cp-surface w-full max-w-md mx-4 overflow-hidden shadow-cp-xl">
          <div class="px-6 pt-6 pb-4">
            <h2 class="text-xl font-bold mb-1 text-cp-text-primary">
              {{ t('libraryDetail.modal.title') }}
            </h2>
            <p class="text-sm text-cp-text-secondary">
              {{ t('libraryDetail.modal.subtitle') }}
            </p>
          </div>
          <div class="px-6 pb-6">
            <div class="flex gap-4 items-center p-4 rounded-xl mb-5 bg-cp-surface-muted">
              <QuizCoverThumbnail
                :cover-image-url="quiz.coverImageUrl"
                :cover-emoji="quiz.coverEmoji"
                :cover-gradient="quiz.coverGradient"
                size="sm"
              />
              <div>
                <p class="font-semibold text-sm mb-1 text-cp-text-primary">
                  {{ quiz.title }}
                </p>
                <p class="text-xs text-cp-text-secondary">
                  {{ t('libraryDetail.questions', { count: quiz.questionCount }) }} · {{ DIFFICULTY_LABEL[quiz.difficulty] }} · {{ t('libraryDetail.passRateBadge', { rate: quiz.passRate }) }}
                </p>
              </div>
            </div>

            <p class="text-xs font-semibold mb-3 text-cp-text-muted tracking-wide">
              {{ t('libraryDetail.modal.settings') }}
            </p>
            <div class="flex flex-col gap-2 mb-6">
              <div
                v-for="setting in gameSettings"
                :key="setting.label"
                class="flex justify-between items-center p-3 rounded-xl bg-cp-surface-muted"
              >
                <div>
                  <p class="text-sm font-medium text-cp-text-primary">
                    {{ setting.label }}
                  </p>
                  <p class="text-xs text-cp-text-muted">
                    {{ setting.value }}
                  </p>
                </div>
                <UIcon
                  name="i-lucide-check"
                  class="text-cp-success text-lg"
                />
              </div>

              <div class="flex justify-between items-center p-3 rounded-xl bg-cp-surface-muted">
                <p class="text-sm font-medium text-cp-text-primary">
                  {{ t('libraryDetail.modal.timeLimit') }}
                </p>
                <div class="flex gap-1.5">
                  <button
                    v-for="option in timeLimitOptions"
                    :key="option"
                    type="button"
                    class="h-8 px-3 rounded-full text-xs font-semibold cursor-pointer transition-colors"
                    :class="option === timeLimit
                      ? 'bg-cp-primary text-white'
                      : 'bg-cp-surface text-cp-text-secondary'"
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

    <ReportQuizModal
      :open="isReportModalOpen"
      :is-submitting="quizStore.isReporting"
      @confirm="handleReport"
      @cancel="isReportModalOpen = false"
    />
  </div>

  <!-- Loading -->
  <div
    v-else
    class="flex justify-center py-20"
  >
    <UIcon
      name="i-lucide-loader-2"
      class="animate-spin text-4xl text-cp-primary"
    />
  </div>
</template>

<script setup lang="ts">
import QuizCoverThumbnail from '~/components/library/QuizCoverThumbnail.vue'
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
  published: t('libraryDetail.status.published'),
  draft: t('libraryDetail.status.draft'),
  archived: t('libraryDetail.status.archived')
}[quiz.value?.status ?? ''] ?? quiz.value?.status))

const statusBadgeClass = computed(() => ({
  published: 'bg-success-100 text-success-800',
  draft: 'bg-neutral-100 text-neutral-600',
  archived: 'bg-warning-100 text-warning-800'
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
      questionIds: quiz.value.questionIds,
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

const isReportModalOpen = ref(false)

const handleReport = async (reason: string, description?: string) => {
  if (!quiz.value) return
  try {
    await quizStore.reportQuiz(quiz.value.id, reason, description)
    isReportModalOpen.value = false
  } catch {
    // 錯誤已寫入 quizStore.error，這裡不需要額外處理
  }
}

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
