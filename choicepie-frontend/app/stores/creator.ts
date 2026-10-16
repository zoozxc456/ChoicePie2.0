import { defineStore } from 'pinia'
import { useCreatorClientApi } from '~/services/creator/client'
import { toQuizFromSummary } from '~/stores/quiz'
import type { CreatorProfileDto } from '~/types/api'

export const useCreatorStore = defineStore('creator', () => {
  const creatorApi = useCreatorClientApi()

  const profile = ref<CreatorProfileDto | null>(null)
  const isLoading = ref(false)
  const isTogglingFollow = ref(false)
  const error = ref<string | null>(null)

  // 創作者公開作品（載入更多）
  const currentCreatorId = ref('')
  const quizList = createPagedList(
    (page, pageSize) => creatorApi.fetchCreatorQuizzes(currentCreatorId.value, page, pageSize),
    toQuizFromSummary
  )

  const fetchCreatorQuizzes = async (id: string) => {
    if (currentCreatorId.value !== id) quizList.reset()
    currentCreatorId.value = id
    await quizList.load()
  }

  const fetchCreatorProfile = async (id: string) => {
    isLoading.value = true
    error.value = null
    try {
      profile.value = await creatorApi.fetchCreatorProfile(id)
    } catch (e) {
      error.value = '無法載入創作者資料'
      console.error(e)
    } finally {
      isLoading.value = false
    }
  }

  const toggleFollow = async (id: string) => {
    if (!profile.value) return
    isTogglingFollow.value = true
    const next = !profile.value.isFollowing
    try {
      if (next) {
        await creatorApi.followCreator(id)
      } else {
        await creatorApi.unfollowCreator(id)
      }
      profile.value = { ...profile.value, isFollowing: next }
    } catch (e) {
      error.value = '操作失敗，請稍後再試'
      console.error(e)
      throw e
    } finally {
      isTogglingFollow.value = false
    }
  }

  return {
    profile, isLoading, isTogglingFollow, error,
    fetchCreatorProfile, toggleFollow,
    quizzes: quizList.items,
    quizzesHasNext: quizList.hasNext,
    isLoadingQuizzes: quizList.isLoading,
    isLoadingMoreQuizzes: quizList.isLoadingMore,
    quizzesHasError: quizList.hasError,
    fetchCreatorQuizzes,
    loadMoreQuizzes: quizList.loadMore
  }
})
