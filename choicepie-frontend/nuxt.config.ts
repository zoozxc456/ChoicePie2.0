// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  modules: [
    '@nuxt/eslint',
    '@nuxt/ui',
    '@nuxt/image',
    '@vueuse/motion/nuxt',
    '@vueuse/nuxt',
    '@pinia/nuxt',
    '@nuxt/fonts',
    '@nuxtjs/sitemap',
    'pinia-plugin-persistedstate/nuxt',
    '@nuxtjs/i18n'
  ],
  devtools: {
    enabled: true
  },
  css: ['~/assets/css/main.css'],
  runtimeConfig: {
    public: {
      apiBaseUrl: 'https://choicepie-dev-api.minjie.demo'
    }
  },
  devServer: {
    https: {
      key: './cert/key.pem',
      cert: './cert/cert.pem'
    },
    host: '0.0.0.0',
    port: 3000
  },
  compatibilityDate: '2025-07-15',
  vite: {
    server: {
      warmup: {
        clientFiles: ['./app/layouts/**/*.vue', './app/components/common/**/*.vue']
      }
    }
  },
  fonts: {
    families: [
      { name: 'Outfit', provider: 'google', weights: [400, 500, 600, 700, 800] },
      { name: 'Noto Sans TC', provider: 'google', weights: [400, 500, 600, 700, 800] }
    ]
  },
  i18n: {
    locales: [
      { code: 'zh-TW', language: 'zh-TW', name: '繁體中文', file: 'zh-TW.json' },
      { code: 'en', language: 'en-US', name: 'English', file: 'en.json' }
    ],
    defaultLocale: 'zh-TW',
    langDir: 'locales/',
    strategy: 'no_prefix'
  }
})
