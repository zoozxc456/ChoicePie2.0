import type { AdminUserDto } from '~/types/api'
import type { AdminLoginSchema } from '~/types/adminAuth'

export const useAdminAuthClientApi = () => {
  const api = useApi()

  return {
    loginWithEmail: (payload: AdminLoginSchema) => api.post<AdminUserDto>('/api/v1/admin/auth/login', payload),
    logout: () => api.post('/api/v1/admin/auth/logout'),
    refresh: () => api.post<AdminUserDto>('/api/v1/admin/auth/refresh'),
    fetchMe: () => api.get<AdminUserDto>('/api/v1/admin/auth/me')
  }
}
