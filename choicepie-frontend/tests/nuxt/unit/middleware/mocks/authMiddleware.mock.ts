import { vi } from 'vitest'
import { mockNuxtImport } from '@nuxt/test-utils/runtime'

const useAuthStoreMock = vi.hoisted(() => vi.fn())
const navigateToMock = vi.hoisted(() => vi.fn())

mockNuxtImport('useAuthStore', () => useAuthStoreMock)
mockNuxtImport('navigateTo', () => navigateToMock)

export { useAuthStoreMock, navigateToMock }
