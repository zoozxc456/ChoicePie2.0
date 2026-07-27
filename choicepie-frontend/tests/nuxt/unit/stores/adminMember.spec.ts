import { describe, it, expect, beforeEach, vi } from 'vitest'
import { setActivePinia, createPinia } from 'pinia'
import type { AdminMemberSummaryDto, PagedResult } from '~/types/api'
import { adminMemberClientMock } from './mocks/adminMemberClient.mock'

const { fetchMembers, suspendMember, unsuspendMember } = adminMemberClientMock

const { useAdminMemberStore } = await import('~/stores/adminMember')

const makeMember = (overrides: Partial<AdminMemberSummaryDto> = {}): AdminMemberSummaryDto => ({
  id: 'member-1',
  name: 'Member Name',
  email: 'member@example.com',
  isSuspended: false,
  suspendedReason: null,
  suspendedUntil: null,
  createdAt: '2026-01-01T00:00:00Z',
  ...overrides
})

const pagedResult: PagedResult<AdminMemberSummaryDto> = {
  pageNumber: 1,
  pageSize: 20,
  totalCount: 1,
  items: [makeMember()]
}

describe('useAdminMemberStore', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
    vi.spyOn(console, 'error').mockImplementation(() => {})
  })

  describe('fetchMembers', () => {
    it('成功時儲存 members 並回傳資料', async () => {
      fetchMembers.mockResolvedValue(pagedResult)
      const store = useAdminMemberStore()

      const result = await store.fetchMembers({ search: 'foo' })

      expect(fetchMembers).toHaveBeenCalledWith({ search: 'foo' })
      expect(result).toEqual(pagedResult)
      expect(store.members).toEqual(pagedResult)
    })

    it('失敗時設定 error 並往外拋出', async () => {
      fetchMembers.mockRejectedValue(new Error('boom'))
      const store = useAdminMemberStore()

      await expect(store.fetchMembers()).rejects.toThrow('boom')

      expect(store.error).toBe('無法載入會員列表')
    })
  })

  describe('suspendMember', () => {
    it('成功時更新該會員的停權狀態', async () => {
      fetchMembers.mockResolvedValue(pagedResult)
      suspendMember.mockResolvedValue(undefined)
      const store = useAdminMemberStore()
      await store.fetchMembers()

      await store.suspendMember('member-1', 'spamming', '2026-08-01T00:00:00Z')

      expect(suspendMember).toHaveBeenCalledWith('member-1', 'spamming', '2026-08-01T00:00:00Z')
      expect(store.members?.items[0]?.isSuspended).toBe(true)
      expect(store.members?.items[0]?.suspendedReason).toBe('spamming')
      expect(store.members?.items[0]?.suspendedUntil).toBe('2026-08-01T00:00:00Z')
    })

    it('失敗時設定 error 並往外拋出', async () => {
      suspendMember.mockRejectedValue(new Error('boom'))
      const store = useAdminMemberStore()

      await expect(store.suspendMember('member-1', 'reason', null)).rejects.toThrow('boom')

      expect(store.error).toBe('停權會員失敗，請稍後再試')
    })
  })

  describe('unsuspendMember', () => {
    it('成功時清除該會員的停權狀態', async () => {
      fetchMembers.mockResolvedValue({
        ...pagedResult,
        items: [makeMember({ isSuspended: true, suspendedReason: 'spamming', suspendedUntil: null })]
      })
      unsuspendMember.mockResolvedValue(undefined)
      const store = useAdminMemberStore()
      await store.fetchMembers()

      await store.unsuspendMember('member-1')

      expect(unsuspendMember).toHaveBeenCalledWith('member-1')
      expect(store.members?.items[0]?.isSuspended).toBe(false)
      expect(store.members?.items[0]?.suspendedReason).toBeNull()
    })

    it('失敗時設定 error 並往外拋出', async () => {
      unsuspendMember.mockRejectedValue(new Error('boom'))
      const store = useAdminMemberStore()

      await expect(store.unsuspendMember('member-1')).rejects.toThrow('boom')

      expect(store.error).toBe('解除停權失敗，請稍後再試')
    })
  })
})
