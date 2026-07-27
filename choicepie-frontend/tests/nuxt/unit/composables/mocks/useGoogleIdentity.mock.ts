import { vi } from 'vitest'
import { mockNuxtImport } from '@nuxt/test-utils/runtime'

mockNuxtImport('useRuntimeConfig', () => () => ({
  public: { googleClientId: 'test-client-id.apps.googleusercontent.com' }
}))

export { vi }
