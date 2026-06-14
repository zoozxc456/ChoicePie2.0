// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  modules: ['@nuxt/eslint', '@nuxt/ui', '@nuxt/image', '@vueuse/motion/nuxt', '@vueuse/nuxt', '@pinia/nuxt', '@nuxt/fonts', '@nuxtjs/sitemap'],
  devtools: {
    enabled: true
  },
  css: [
    '~/assets/css/design-tokens.css',
  ],
  app: {
    head: {
      link: [
        {
          rel: 'preconnect',
          href: 'https://fonts.googleapis.com',
        },
        {
          rel: 'preconnect',
          href: 'https://fonts.gstatic.com',
          crossorigin: '',
        },
        {
          rel: 'stylesheet',
          href: 'https://fonts.googleapis.com/css2?family=Inter:wght@400;500;600;700;800&family=Noto+Sans+TC:wght@400;500;600;700;800&display=swap',
        },
      ],
    },
  },
});
