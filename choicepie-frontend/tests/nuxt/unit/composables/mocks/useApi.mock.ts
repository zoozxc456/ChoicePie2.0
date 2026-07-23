import { vi } from 'vitest'
import { mockNuxtImport } from '@nuxt/test-utils/runtime'

const fetchMock = vi.hoisted(() => vi.fn())
const useAuthStoreMock = vi.hoisted(() => vi.fn())
const useAdminAuthStoreMock = vi.hoisted(() => vi.fn())

mockNuxtImport('useRuntimeConfig', () => () => ({
  public: { apiBaseUrl: 'https://api.example.test' }
}))
mockNuxtImport('useAuthStore', () => useAuthStoreMock)
mockNuxtImport('useAdminAuthStore', () => useAdminAuthStoreMock)

vi.stubGlobal('$fetch', fetchMock)

export { fetchMock, useAuthStoreMock, useAdminAuthStoreMock }
