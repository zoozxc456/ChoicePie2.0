import { vi } from 'vitest'

export const adminMemberClientMock = {
  fetchMembers: vi.fn(),
  suspendMember: vi.fn(),
  unsuspendMember: vi.fn()
}

vi.mock('~/services/admin/member', () => ({
  useAdminMemberClientApi: () => adminMemberClientMock
}))
