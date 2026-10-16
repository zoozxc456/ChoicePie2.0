import * as Sentry from '@sentry/nuxt'

// Server 端在 Nitro 啟動時載入，此時還拿不到 runtimeConfig，所以直接讀同名環境變數
// （與 nuxt.config.ts 的 runtimeConfig.public.sentry 使用相同的 NUXT_PUBLIC_SENTRY_* 變數）。
const dsn = process.env.NUXT_PUBLIC_SENTRY_DSN

// DSN 為空時完全不初始化 Sentry。
if (dsn) {
  Sentry.init({
    dsn,
    environment: process.env.NUXT_PUBLIC_SENTRY_ENVIRONMENT || process.env.NODE_ENV || 'production',
    tracesSampleRate: Number(process.env.NUXT_PUBLIC_SENTRY_TRACES_SAMPLE_RATE) || 0,
    sendDefaultPii: false
  })
}
