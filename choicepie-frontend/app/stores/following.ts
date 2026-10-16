import { defineStore } from 'pinia'
import { useCreatorClientApi } from '~/services/creator/client'
import { toQuizFromSummary } from '~/stores/quiz'

export const useFollowingStore = defineStore('following', () => {
  const creatorApi = useCreatorClientApi()

  const creatorList = createPagedList(
    (page, pageSize) => creatorApi.fetchFollowing(page, pageSize),
    dto => dto,
    20
  )
  const feedList = createPagedList(
    (page, pageSize) => creatorApi.fetchFollowingQuizzes(page, pageSize),
    toQuizFromSummary
  )

  const fetchAll = () => Promise.all([creatorList.load(), feedList.load()])

  return {
    creators: creatorList.items,
    hasMoreCreators: creatorList.hasNext,
    isLoadingCreators: creatorList.isLoading,
    isLoadingMoreCreators: creatorList.isLoadingMore,
    creatorsHasError: creatorList.hasError,
    feed: feedList.items,
    hasMoreFeed: feedList.hasNext,
    isLoadingFeed: feedList.isLoading,
    isLoadingMoreFeed: feedList.isLoadingMore,
    feedHasError: feedList.hasError,
    fetchAll,
    loadMoreCreators: creatorList.loadMore,
    loadMoreFeed: feedList.loadMore
  }
})
