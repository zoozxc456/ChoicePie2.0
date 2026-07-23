import type { ApiEnvelope } from '~/types/api'

export class ApiError extends Error {
  code: string
  status: number

  constructor(message: string, code: string, status: number) {
    super(message)
    this.name = 'ApiError'
    this.code = code
    this.status = status
  }
}

const AUTH_REFRESH_PATH = '/api/v1/auth/refresh'
const AUTH_LOGOUT_PATH = '/api/v1/auth/logout'
const ADMIN_AUTH_REFRESH_PATH = '/api/v1/admin/auth/refresh'
const ADMIN_AUTH_LOGOUT_PATH = '/api/v1/admin/auth/logout'
const ADMIN_PATH_PREFIX = '/api/v1/admin/'

// 401 時先嘗試 refresh 一次，多個請求同時 401 只會觸發一次 refresh。
// admin 與會員各自獨立的 in-flight refresh，避免兩邊 session 互相干擾。
let refreshPromise: Promise<boolean> | null = null
let adminRefreshPromise: Promise<boolean> | null = null

/** 呼叫後端 ChoicePie.Backend.WebApi，統一帶入 baseURL 與 cookie 憑證，並解開 ApiResponse 信封 */
export const useApi = () => {
  const config = useRuntimeConfig()

  const toApiError = (e: unknown) => {
    const fetchError = e as { data?: ApiEnvelope<unknown>, status?: number, message?: string }
    return new ApiError(
      fetchError.data?.message ?? fetchError.message ?? '發生未知錯誤',
      fetchError.data?.code ?? 'UNKNOWN',
      fetchError.status ?? 0
    )
  }

  const fetchEnvelope = <T>(path: string, options: Parameters<typeof $fetch>[1] = {}) =>
    $fetch<ApiEnvelope<T>>(path, {
      baseURL: config.public.apiBaseUrl,
      credentials: 'include',
      ...options
    })

  const request = async <T>(path: string, options: Parameters<typeof $fetch>[1] = {}, isRetry = false): Promise<T> => {
    try {
      const envelope = await fetchEnvelope<T>(path, options)
      return envelope.data as T
    } catch (e) {
      const apiError = toApiError(e)

      const isAdminPath = path.startsWith(ADMIN_PATH_PREFIX)
      const isAuthEndpoint = isAdminPath
        ? path === ADMIN_AUTH_REFRESH_PATH || path === ADMIN_AUTH_LOGOUT_PATH
        : path === AUTH_REFRESH_PATH || path === AUTH_LOGOUT_PATH

      if (apiError.status === 401 && !isRetry && !isAuthEndpoint) {
        if (isAdminPath) {
          adminRefreshPromise ??= useAdminAuthStore().fetchMe().finally(() => {
            adminRefreshPromise = null
          })
          const refreshed = await adminRefreshPromise

          if (refreshed) {
            return request<T>(path, options, true)
          }

          await useAdminAuthStore().logout('/admin/login')
        } else {
          refreshPromise ??= useAuthStore().fetchMe().finally(() => {
            refreshPromise = null
          })
          const refreshed = await refreshPromise

          if (refreshed) {
            return request<T>(path, options, true)
          }

          await useAuthStore().logout('/')
        }
      }

      throw apiError
    }
  }

  return {
    get: <T>(path: string, query?: Record<string, unknown>) =>
      request<T>(path, { method: 'GET', query }),
    post: <T>(path: string, body?: object) =>
      request<T>(path, { method: 'POST', body }),
    put: <T>(path: string, body?: object) =>
      request<T>(path, { method: 'PUT', body }),
    del: <T>(path: string) =>
      request<T>(path, { method: 'DELETE' })
  }
}
