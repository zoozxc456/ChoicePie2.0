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
    '@nuxtjs/i18n',
    '@sentry/nuxt/module'
  ],
  devtools: {
    enabled: true
  },
  css: ['~/assets/css/main.css'],
  runtimeConfig: {
    // Nitro server routes 用來呼叫真正後端的位址（server-only，不會被打包進前端 bundle）。
    backendApiUrl: 'https://choicepie-dev-api.minjie.demo',
    // 只有 Nuxt 本身位於可信任的負載平衡器後方時才設為 true（NUXT_TRUST_PROXY_HEADERS），
    // 此時才採用進來的 X-Forwarded-For 當使用者 IP；預設只用 socket 位址，避免被偽造。
    trustProxyHeaders: false,
    // 開啟後在非 dev 環境也印出（已遮蔽憑證的）backendFetch 日誌（NUXT_DEBUG_BACKEND_FETCH）。
    debugBackendFetch: false,
    public: {
      apiBaseUrl: 'https://choicepie-dev-api.minjie.demo',
      googleClientId: '',
      sentry: {
        // 空字串 = 不啟用 Sentry。由 NUXT_PUBLIC_SENTRY_DSN 覆寫。
        dsn: '',
        environment: process.env.NODE_ENV || 'production',
        tracesSampleRate: 0
      }
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
  },
  // 不上傳 source map（需要 SENTRY_AUTH_TOKEN）；如需啟用請見 .env.example。
  sentry: {
    sourcemaps: { disable: true },
    telemetry: false
  }
})
