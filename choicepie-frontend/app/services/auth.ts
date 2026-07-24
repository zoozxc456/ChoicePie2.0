import type { MemberDto } from '~/types/api'
import type { LoginSchema, RegisterSchema, ForgotPasswordSchema } from '~/types/auth'

export const useAuthClientApi = () => {
  const api = useApi()

  return {
    register: (payload: RegisterSchema) => api.post<MemberDto>('/api/v1/auth/register', payload),
    loginWithEmail: (payload: LoginSchema) => api.post<MemberDto>('/api/v1/auth/login', payload),
    logout: () => api.post('/api/v1/auth/logout'),
    refresh: () => api.post<MemberDto>('/api/v1/auth/refresh'),
    forgotPassword: (payload: ForgotPasswordSchema) => api.post('/api/v1/auth/forgot-password', payload),
    resetPassword: (payload: { token: string, password: string, confirmPassword: string }) =>
      api.post('/api/v1/auth/reset-password', payload),
    verifyEmail: (token: string) => api.post('/api/v1/auth/verify-email', { token }),
    resendVerification: () => api.post('/api/v1/auth/resend-verification')
  }
}
