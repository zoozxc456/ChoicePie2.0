import { vi } from 'vitest'

export const adminQuizReportClientMock = {
  fetchReports: vi.fn(),
  resolveReport: vi.fn(),
  dismissReport: vi.fn()
}

vi.mock('~/services/admin/quizReport', () => ({
  useAdminQuizReportClientApi: () => adminQuizReportClientMock
}))
