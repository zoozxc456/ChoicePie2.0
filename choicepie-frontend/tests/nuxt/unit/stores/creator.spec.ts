import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import type { CreatorProfileDto } from '~/types/api'
import { creatorClientMock } from './mocks/creatorClient.mock'

const { fetchCreatorProfile, followCreator, unfollowCreator, fetchCreatorQuizzes } = creatorClientMock

const { useCreatorStore } = await import('~/stores/creator')

const makeProfile = (overrides: Partial<CreatorProfileDto> = {}): CreatorProfileDto => ({
  id: 'creator-1',
  name: 'Alice',
  avatar: null,
  quizCount: 3,
  challengeCount: 10,
  isFollowing: false,
  ...overrides
})

describe('useCreatorStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    vi.spyOn(console, 'error').mockImplementation(() => {})
  })

  describe('fetchCreatorProfile', () => {
    it('成功時儲存 profile', async () => {
      fetchCreatorProfile.mockResolvedValue(makeProfile())
      const store = useCreatorStore()

      await store.fetchCreatorProfile('creator-1')

      expect(fetchCreatorProfile).toHaveBeenCalledWith('creator-1')
      expect(store.profile?.id).toBe('creator-1')
      expect(store.isLoading).toBe(false)
      expect(store.error).toBeNull()
    })

    it('失敗時設定 error 訊息並保留 profile 為 null', async () => {
      fetchCreatorProfile.mockRejectedValue(new Error('boom'))
      const store = useCreatorStore()

      await store.fetchCreatorProfile('creator-1')

      expect(store.error).toBe('無法載入創作者資料')
      expect(store.profile).toBeNull()
      expect(store.isLoading).toBe(false)
    })
  })

  describe('fetchCreatorQuizzes / loadMoreQuizzes', () => {
    const page = (pageNumber: number, ids: string[], hasNextPage: boolean) => ({
      pageNumber,
      pageSize: 12,
      totalCount: 20,
      hasNextPage,
      items: ids.map(id => ({
        id, title: id, description: null, coverEmoji: '📝', coverGradient: '', difficulty: 'beginner',
        status: 'published', questionCount: 1, challengeCount: 0, passRate: 0, creatorId: 'creator-1',
        creatorName: 'Alice', creatorAvatar: null, tags: [], createdAt: '', updatedAt: ''
      }))
    })

    it('載入第一頁並以創作者 id 呼叫 API', async () => {
      fetchCreatorQuizzes.mockResolvedValue(page(1, ['q1', 'q2'], true))
      const store = useCreatorStore()

      await store.fetchCreatorQuizzes('creator-1')

      expect(fetchCreatorQuizzes).toHaveBeenCalledWith('creator-1', 1, 12)
      expect(store.quizzes.map(q => q.id)).toEqual(['q1', 'q2'])
      expect(store.quizzesHasNext).toBe(true)
    })

    it('載入更多時累加下一頁', async () => {
      fetchCreatorQuizzes.mockResolvedValueOnce(page(1, ['q1'], true)).mockResolvedValueOnce(page(2, ['q2'], false))
      const store = useCreatorStore()
      await store.fetchCreatorQuizzes('creator-1')

      await store.loadMoreQuizzes()

      expect(fetchCreatorQuizzes).toHaveBeenLastCalledWith('creator-1', 2, 12)
      expect(store.quizzes.map(q => q.id)).toEqual(['q1', 'q2'])
      expect(store.quizzesHasNext).toBe(false)
    })

    it('切換創作者時清空舊作品', async () => {
      fetchCreatorQuizzes.mockResolvedValueOnce(page(1, ['q1'], false)).mockRejectedValueOnce(new Error('boom'))
      const store = useCreatorStore()
      await store.fetchCreatorQuizzes('creator-1')

      await store.fetchCreatorQuizzes('creator-2')

      expect(store.quizzes).toEqual([])
      expect(store.quizzesHasError).toBe(true)
    })
  })

  describe('toggleFollow', () => {
    it('profile 尚未載入時不做任何事', async () => {
      const store = useCreatorStore()

      await store.toggleFollow('creator-1')

      expect(followCreator).not.toHaveBeenCalled()
      expect(unfollowCreator).not.toHaveBeenCalled()
    })

    it('未追蹤時切換會呼叫 followCreator 並更新 isFollowing', async () => {
      fetchCreatorProfile.mockResolvedValue(makeProfile({ isFollowing: false }))
      followCreator.mockResolvedValue(undefined)
      const store = useCreatorStore()
      await store.fetchCreatorProfile('creator-1')

      await store.toggleFollow('creator-1')

      expect(followCreator).toHaveBeenCalledWith('creator-1')
      expect(store.profile?.isFollowing).toBe(true)
      expect(store.isTogglingFollow).toBe(false)
    })

    it('已追蹤時切換會呼叫 unfollowCreator 並更新 isFollowing', async () => {
      fetchCreatorProfile.mockResolvedValue(makeProfile({ isFollowing: true }))
      unfollowCreator.mockResolvedValue(undefined)
      const store = useCreatorStore()
      await store.fetchCreatorProfile('creator-1')

      await store.toggleFollow('creator-1')

      expect(unfollowCreator).toHaveBeenCalledWith('creator-1')
      expect(store.profile?.isFollowing).toBe(false)
    })

    it('失敗時維持原本 isFollowing 狀態、設定 error 並往外拋出', async () => {
      fetchCreatorProfile.mockResolvedValue(makeProfile({ isFollowing: false }))
      followCreator.mockRejectedValue(new Error('boom'))
      const store = useCreatorStore()
      await store.fetchCreatorProfile('creator-1')

      await expect(store.toggleFollow('creator-1')).rejects.toThrow('boom')

      expect(store.profile?.isFollowing).toBe(false)
      expect(store.error).toBe('操作失敗，請稍後再試')
      expect(store.isTogglingFollow).toBe(false)
    })
  })
})
