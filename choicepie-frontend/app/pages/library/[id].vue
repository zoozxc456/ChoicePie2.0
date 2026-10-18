<template>
  <div
    v-if="quiz"
    class="max-w-5xl mx-auto px-6 pt-6 pb-20"
  >
    <BackLink
      to="/library"
      :label="t('libraryDetail.backToLibrary')"
      class="mb-4"
    />

    <QuizDetailHero :quiz="quiz" />
    <QuizDetailActionBar
      class="mb-6"
      :quiz="quiz"
      :is-owner="isOwner"
      :is-favorited="quizStore.isFavorited"
      :is-toggling-favorite="quizStore.isTogglingFavorite"
      :has-reported="quizStore.hasReported"
      :is-starting-attempt="isStartingAttempt"
      :is-toggling-status="isTogglingStatus"
      @start="handleOpenStartModal"
      @solo-practice="handleSoloPractice"
      @toggle-favorite="handleToggleFavorite"
      @publish="handlePublish"
      @unpublish="handleUnpublish"
      @report="handleOpenReportModal"
    />

    <div class="grid grid-cols-1 lg:grid-cols-[1fr_320px] gap-6">
      <div class="flex flex-col gap-6">
        <QuizQuestionList
          v-if="isOwner"
          :questions="quiz.questions"
        />
        <AttemptHistoryList :quiz-id="quizId" />
        <CommentList :quiz-id="quizId" />
        <RelatedQuizList :quizzes="quizStore.relatedQuizzes" />
      </div>

      <div class="flex flex-col gap-6">
        <CreatorSideCard
          :creator-id="quiz.creatorId"
          :creator-name="quiz.creatorName"
          :profile="creatorStore.profile"
          :can-follow="!isOwner"
          :is-toggling-follow="creatorStore.isTogglingFollow"
          @toggle-follow="handleToggleFollow"
        />
      </div>
    </div>

    <StartGameModal
      :open="isStartModalOpen"
      :quiz="quiz"
      :loading="isCreatingRoom"
      @create="handleCreateRoom"
      @cancel="isStartModalOpen = false"
    />

    <ReportQuizModal
      :open="isReportModalOpen"
      :is-submitting="quizStore.isReporting"
      @confirm="handleReport"
      @cancel="isReportModalOpen = false"
    />
  </div>

  <LoadingSpinner v-else />
</template>

<script setup lang="ts">
import BackLink from '~/components/common/BackLink.vue'
import LoadingSpinner from '~/components/common/LoadingSpinner.vue'
import AttemptHistoryList from '~/components/attempt/AttemptHistoryList.vue'
import CommentList from '~/components/library/CommentList.vue'
import ReportQuizModal from '~/components/library/ReportQuizModal.vue'
import CreatorSideCard from '~/components/library/detail/CreatorSideCard.vue'
import QuizDetailActionBar from '~/components/library/detail/QuizDetailActionBar.vue'
import QuizDetailHero from '~/components/library/detail/QuizDetailHero.vue'
import QuizQuestionList from '~/components/library/detail/QuizQuestionList.vue'
import RelatedQuizList from '~/components/library/detail/RelatedQuizList.vue'
import StartGameModal from '~/components/library/detail/StartGameModal.vue'
import { useRequireLogin } from '~/composables/useRequireLogin'

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

// 封面圖（R2 上傳）存在時才帶 OG 圖與大圖卡片；以絕對網址輸出，社群爬蟲才抓得到。
const requestOrigin = useRequestURL().origin
const ogImageUrl = computed(() =>
  toAbsoluteUrl(quiz.value?.coverImageUrl, requestOrigin))

useSeoMeta({
  title: () => quiz.value?.title,
  ogTitle: () => quiz.value?.title,
  description: () => quiz.value?.description ?? undefined,
  ogDescription: () => quiz.value?.description ?? undefined,
  ogType: 'website',
  ogImage: () => ogImageUrl.value,
  twitterCard: () => ogImageUrl.value ? 'summary_large_image' : undefined,
  twitterImage: () => ogImageUrl.value
})

// 需要登入的動作（開房、單人練習、檢舉、收藏、追蹤）：未登入者導向登入頁，登入後導回本題庫。
const { ensureLoggedIn } = useRequireLogin()

const handleOpenStartModal = async () => {
  if (!await ensureLoggedIn()) return
  isStartModalOpen.value = true
}

const isOwner = computed(() => !!auth.user && auth.user.id === quiz.value?.creatorId)

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

const handleCreateRoom = async (timeLimit: number) => {
  if (!quiz.value) return
  isCreatingRoom.value = true
  try {
    await gameRoom.createRoom({
      quizId: quiz.value.id,
      questionIds: quiz.value.questionIds,
      timeLimit
    })
  } catch {
    isCreatingRoom.value = false
  }
}

const handleSoloPractice = async () => {
  if (!quiz.value) return
  if (!await ensureLoggedIn()) return
  isStartingAttempt.value = true
  try {
    const result = await quizAttemptStore.startAttempt(quiz.value.id)
    await navigateTo(`/attempt/${result.attemptId}`)
  } catch {
    isStartingAttempt.value = false
  }
}

const isReportModalOpen = ref(false)

const handleOpenReportModal = async () => {
  if (!await ensureLoggedIn()) return
  isReportModalOpen.value = true
}

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
  if (!await ensureLoggedIn()) return
  try {
    await quizStore.toggleFavorite(quiz.value.id)
  } catch {
    // 錯誤已寫入 quizStore.error，這裡不需要額外處理
  }
}

const handleToggleFollow = async () => {
  if (!quiz.value) return
  if (!await ensureLoggedIn()) return
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

<style scoped lang="scss"></style>
