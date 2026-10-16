import { ApiError } from '~/composables/useApi'

/** 優先顯示後端回傳的（已本地化的）錯誤訊息，沒有時才用呼叫端提供的 fallback。 */
export const getApiErrorMessage = (e: unknown, fallback: string) =>
  e instanceof ApiError && e.message ? e.message : fallback
