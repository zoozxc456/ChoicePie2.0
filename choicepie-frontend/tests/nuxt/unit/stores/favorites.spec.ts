import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import { quizClientMock } from './mocks/quizClient.mock'

const { fetchMyFavorites, removeFavorite } = quizClientMock

const { useFavoritesStore } = await import('~/stores/favorites')

const makeSummary = (id: string) => ({
  id, title: id, description: null, coverEmoji: '📝', coverGradient: '', difficulty: 'beginner',
  status: 'published', questionCount: 1, challengeCount: 0, passRate: 0, creatorId: 'c1',
  creatorName: 'Alice', creatorAvatar: null, tags: [], createdAt: '', updatedAt: ''
})

const makePage = (ids: string[], pageNumber = 1, totalCount = ids.length, pageSize = 12) => ({
  pageNumber, pageSize, totalCount, items: ids.map(makeSummary)
})

describe('useFavoritesStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    vi.spyOn(console, 'error').mockImplementation(() => {})
  })

  it('fetchFavorites 載入第一頁收藏', async () => {
    fetchMyFavorites.mockResolvedValue(makePage(['q1', 'q2']))
    const store = useFavoritesStore()

    await store.fetchFavorites()

    expect(fetchMyFavorites).toHaveBeenCalledWith(1, 12)
    expect(store.quizzes.map(q => q.id)).toEqual(['q1', 'q2'])
    expect(store.hasNext).toBe(false)
  })

  it('沒有 hasNextPage 欄位時以總數推算是否還有下一頁', async () => {
    fetchMyFavorites.mockResolvedValue(makePage(['q1'], 1, 30))
    const store = useFavoritesStore()

    await store.fetchFavorites()

    expect(store.hasNext).toBe(true)
  })

  it('loadMore 累加下一頁', async () => {
    fetchMyFavorites
      .mockResolvedValueOnce(makePage(['q1'], 1, 2, 1))
      .mockResolvedValueOnce(makePage(['q2'], 2, 2, 1))
    const store = useFavoritesStore()
    await store.fetchFavorites()

    await store.loadMore()

    expect(fetchMyFavorites).toHaveBeenLastCalledWith(2, 12)
    expect(store.quizzes.map(q => q.id)).toEqual(['q1', 'q2'])
  })

  it('載入失敗時標記 hasError', async () => {
    fetchMyFavorites.mockRejectedValue(new Error('boom'))
    const store = useFavoritesStore()

    await store.fetchFavorites()

    expect(store.hasError).toBe(true)
    expect(store.isLoading).toBe(false)
  })

  it('unfavorite 成功後從清單移除並減少總數', async () => {
    fetchMyFavorites.mockResolvedValue(makePage(['q1', 'q2']))
    removeFavorite.mockResolvedValue(undefined)
    const store = useFavoritesStore()
    await store.fetchFavorites()

    await store.unfavorite('q1')

    expect(removeFavorite).toHaveBeenCalledWith('q1')
    expect(store.quizzes.map(q => q.id)).toEqual(['q2'])
    expect(store.totalCount).toBe(1)
    expect(store.removingId).toBeNull()
  })

  it('unfavorite 失敗時保留項目並標記錯誤', async () => {
    fetchMyFavorites.mockResolvedValue(makePage(['q1']))
    removeFavorite.mockRejectedValue(new Error('boom'))
    const store = useFavoritesStore()
    await store.fetchFavorites()

    await store.unfavorite('q1')

    expect(store.quizzes).toHaveLength(1)
    expect(store.removeError).toBe(true)
  })
})
