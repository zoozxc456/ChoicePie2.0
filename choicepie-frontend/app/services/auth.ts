import type { MemberDto } from '~/types/api'
import type { LoginSchema, RegisterSchema } from '~/types/auth'

export const useAuthClientApi = () => {
  const api = useApi()

  return {
    register: (payload: RegisterSchema) => api.post<MemberDto>('/api/v1/auth/register', payload),
    loginWithEmail: (payload: LoginSchema) => api.post<MemberDto>('/api/v1/auth/login', payload),
    logout: () => api.post('/api/v1/auth/logout'),
    refresh: () => api.post<MemberDto>('/api/v1/auth/refresh')
  }
}
