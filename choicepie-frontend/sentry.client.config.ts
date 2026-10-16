import * as Sentry from '@sentry/nuxt'

const { sentry } = useRuntimeConfig().public

// DSN 為空時完全不初始化 Sentry（本機開發、CI、未設定的環境都不會送出任何事件）。
if (sentry.dsn) {
  Sentry.init({
    dsn: sentry.dsn,
    environment: sentry.environment,
    tracesSampleRate: Number(sentry.tracesSampleRate) || 0,
    sendDefaultPii: false
  })
}
