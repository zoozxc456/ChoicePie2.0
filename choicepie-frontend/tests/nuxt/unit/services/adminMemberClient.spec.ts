import { describe, it, expect, beforeEach, vi } from 'vitest'
import { apiMock } from './mocks/useApi.mock'
import { useAdminMemberClientApi } from '~/services/admin/member'

describe('useAdminMemberClientApi', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  it('fetchMembers 帶入查詢參數', () => {
    const client = useAdminMemberClientApi()

    client.fetchMembers({ search: 'foo' })

    expect(apiMock.get).toHaveBeenCalledWith('/api/v1/admin/members', { search: 'foo' })
  })

  it('suspendMember 呼叫正確路徑與 body', () => {
    const client = useAdminMemberClientApi()

    client.suspendMember('member1', 'spamming', '2026-08-01T00:00:00Z')

    expect(apiMock.post).toHaveBeenCalledWith('/api/v1/admin/members/member1/suspend', {
      reason: 'spamming',
      until: '2026-08-01T00:00:00Z'
    })
  })

  it('unsuspendMember 呼叫正確路徑', () => {
    const client = useAdminMemberClientApi()

    client.unsuspendMember('member1')

    expect(apiMock.post).toHaveBeenCalledWith('/api/v1/admin/members/member1/unsuspend')
  })
})
