import { vi } from 'vitest'

// useGoogleIdentity 與 auth store 的相依由 authClient.mock 一併處理，使用方請同時 import 它。
const accountClientMock = vi.hoisted(() => ({
  fetchAccount: vi.fn(),
  updateProfile: vi.fn(),
  uploadAvatar: vi.fn(),
  changePassword: vi.fn(),
  linkGoogle: vi.fn(),
  unlinkGoogle: vi.fn(),
  deleteAccount: vi.fn()
}))

vi.mock('~/services/account/client', () => ({
  useAccountClientApi: () => accountClientMock
}))

export { accountClientMock }
