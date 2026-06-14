// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  modules: ['@nuxt/eslint', '@nuxt/ui', '@nuxt/image', '@vueuse/motion/nuxt', '@vueuse/nuxt', '@pinia/nuxt', '@nuxt/fonts', '@nuxtjs/sitemap', 'pinia-plugin-persistedstate/nuxt', '@nuxtjs/i18n'],
  devtools: {
    enabled: true
  },
  app: {
    head: {
      link: [
        {
          rel: 'preconnect',
          href: 'https://fonts.googleapis.com'
        },
        {
          rel: 'preconnect',
          href: 'https://fonts.gstatic.com',
          crossorigin: ''
        },
        {
          rel: 'stylesheet',
          href: 'https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&family=Noto+Sans+TC:wght@400;500;600;700;800&display=swap'
        }
      ]
    }
  },
  css: [
    '~/assets/css/main.css'
  ],
  compatibilityDate: '2025-07-15',
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
