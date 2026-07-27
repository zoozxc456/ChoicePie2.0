import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import type { CreatorProfileDto } from '~/types/api'
import { creatorClientMock } from './mocks/creatorClient.mock'

const { fetchCreatorProfile, followCreator, unfollowCreator } = creatorClientMock

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
