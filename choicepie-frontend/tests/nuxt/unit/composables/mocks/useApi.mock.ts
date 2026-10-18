import { vi } from 'vitest'
import { mockNuxtImport } from '@nuxt/test-utils/runtime'

const fetchMock = vi.hoisted(() => vi.fn())
const useAuthStoreMock = vi.hoisted(() => vi.fn())
const useAdminAuthStoreMock = vi.hoisted(() => vi.fn())

// useApi.ts 在 client 端用 useRequestFetch() 取代全域 $fetch（SSR 才需要它自動帶 cookie，
// client 端行為與全域 $fetch 相同），這裡讓它回傳同一個 fetchMock，維持既有測試斷言方式不變。
const createNuxtAppMock = () => ({
  runWithContext: (fn: () => unknown) => fn(),
  $router: { currentRoute: { value: { fullPath: '/current' } } }
})
// SSR 時每個 incoming request 都有自己的 Nuxt app；測試可換掉它來模擬不同請求。
const nuxtAppState = vi.hoisted(() => ({ current: undefined as unknown }))
nuxtAppState.current = createNuxtAppMock()

mockNuxtImport('useRuntimeConfig', () => () => ({
  public: { apiBaseUrl: 'https://api.example.test' }
}))
mockNuxtImport('useAuthStore', () => useAuthStoreMock)
mockNuxtImport('useAdminAuthStore', () => useAdminAuthStoreMock)
mockNuxtImport('useNuxtApp', () => () => nuxtAppState.current)
mockNuxtImport('useRequestFetch', () => () => fetchMock)
mockNuxtImport('navigateTo', () => vi.fn())

vi.stubGlobal('$fetch', fetchMock)

export { fetchMock, useAuthStoreMock, useAdminAuthStoreMock, nuxtAppState, createNuxtAppMock }
