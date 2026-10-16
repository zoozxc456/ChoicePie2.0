import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { creatorClientMock } from './mocks/creatorClient.mock'

const { fetchFollowing, fetchFollowingQuizzes } = creatorClientMock

const { useFollowingStore } = await import('~/stores/following')

const makeCreator = (id: string) => ({ id, name: id, avatar: null, quizCount: 2, followedAt: '' })
const makeSummary = (id: string) => ({
  id, title: id, description: null, coverEmoji: '📝', coverGradient: '', difficulty: 'beginner',
  status: 'published', questionCount: 1, challengeCount: 0, passRate: 0, creatorId: 'c1',
  creatorName: 'Alice', creatorAvatar: null, tags: [], createdAt: '', updatedAt: ''
})

describe('useFollowingStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    vi.spyOn(console, 'error').mockImplementation(() => {})
  })

  it('fetchAll 同時載入追蹤中創作者與動態', async () => {
    fetchFollowing.mockResolvedValue({ pageNumber: 1, pageSize: 20, totalCount: 1, items: [makeCreator('c1')] })
    fetchFollowingQuizzes.mockResolvedValue({
      pageNumber: 1, pageSize: 12, totalCount: 30, items: [makeSummary('q1')]
    })
    const store = useFollowingStore()

    await store.fetchAll()

    expect(fetchFollowing).toHaveBeenCalledWith(1, 20)
    expect(fetchFollowingQuizzes).toHaveBeenCalledWith(1, 12)
    expect(store.creators.map(c => c.id)).toEqual(['c1'])
    expect(store.feed.map(q => q.id)).toEqual(['q1'])
    expect(store.hasMoreFeed).toBe(true)
    expect(store.hasMoreCreators).toBe(false)
  })

  it('loadMoreFeed 累加動態下一頁', async () => {
    fetchFollowing.mockResolvedValue({ pageNumber: 1, pageSize: 20, totalCount: 1, items: [makeCreator('c1')] })
    fetchFollowingQuizzes
      .mockResolvedValueOnce({ pageNumber: 1, pageSize: 1, totalCount: 2, items: [makeSummary('q1')] })
      .mockResolvedValueOnce({ pageNumber: 2, pageSize: 1, totalCount: 2, items: [makeSummary('q2')] })
    const store = useFollowingStore()
    await store.fetchAll()

    await store.loadMoreFeed()

    expect(fetchFollowingQuizzes).toHaveBeenLastCalledWith(2, 12)
    expect(store.feed.map(q => q.id)).toEqual(['q1', 'q2'])
  })

  it('動態載入失敗時標記 feedHasError', async () => {
    fetchFollowing.mockResolvedValue({ pageNumber: 1, pageSize: 20, totalCount: 0, items: [] })
    fetchFollowingQuizzes.mockRejectedValue(new Error('boom'))
    const store = useFollowingStore()

    await store.fetchAll()

    expect(store.feedHasError).toBe(true)
  })
})
