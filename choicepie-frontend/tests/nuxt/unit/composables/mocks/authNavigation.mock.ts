import { vi } from 'vitest'
import { mockNuxtImport } from '@nuxt/test-utils/runtime'

const useAuthStoreMock = vi.hoisted(() => vi.fn())
const useRouteMock = vi.hoisted(() => vi.fn())
const navigateToMock = vi.hoisted(() => vi.fn())

mockNuxtImport('useAuthStore', () => useAuthStoreMock)
mockNuxtImport('useRoute', () => useRouteMock)
mockNuxtImport('navigateTo', () => navigateToMock)

export { useAuthStoreMock, useRouteMock, navigateToMock }
