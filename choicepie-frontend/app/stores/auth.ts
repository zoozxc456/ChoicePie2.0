import { useAuthClientApi } from '~/services/auth'
import type { User } from '~/types/user'
import type { MemberDto } from '~/types/api'
import type { LoginSchema, RegisterSchema, ForgotPasswordSchema } from '~/types/auth'

const toUser = (member: MemberDto): User => ({
  id: member.id,
  email: member.email,
  name: member.name,
  avatar: member.avatar ?? undefined,
  isVerified: member.isVerified,
  createdAt: member.createdAt
})

export const useAuthStore = defineStore('auth', () => {
  const authApi = useAuthClientApi()

  const user = ref<User | null>(null)
  const isLoggedIn = computed(() => !!user.value)
  const isLoading = ref(false)

  const loginWithGoogle = async () => {
    isLoading.value = true
    try {
      const { requestIdToken } = useGoogleIdentity()
      const idToken = await requestIdToken()
      const member = await authApi.loginWithGoogle(idToken)
      user.value = toUser(member)
    } finally {
      isLoading.value = false
    }
  }

  const register = async (payload: RegisterSchema) => {
    isLoading.value = true
    try {
      const member = await authApi.register(payload)
      user.value = toUser(member)
    } finally {
      isLoading.value = false
    }
  }

  const loginWithEmail = async (payload: LoginSchema) => {
    isLoading.value = true
    try {
      const member = await authApi.loginWithEmail(payload)
      user.value = toUser(member)
    } finally {
      isLoading.value = false
    }
  }

  const logout = async (redirectTo = '/login') => {
    try {
      await authApi.logout()
    } catch {
      // 即使 API 失敗也要清除本地狀態
    } finally {
      user.value = null
      await navigateTo(redirectTo)
    }
  }

  const fetchMe = async () => {
    try {
      const member = await authApi.refresh()
      user.value = toUser(member)
      return true
    } catch {
      user.value = null
      return false
    }
  }

  const setUser = (u: User) => {
    user.value = u
  }

  const isForgotPasswordLoading = ref(false)
  const isResetPasswordLoading = ref(false)
  const isVerifyEmailLoading = ref(false)
  const isResendVerificationLoading = ref(false)
  const error = ref<string | null>(null)

  const forgotPassword = async (payload: ForgotPasswordSchema) => {
    isForgotPasswordLoading.value = true
    error.value = null
    try {
      await authApi.forgotPassword(payload)
    } catch (e) {
      error.value = '發送失敗，請稍後再試'
      console.error(e)
      throw e
    } finally {
      isForgotPasswordLoading.value = false
    }
  }

  const resetPassword = async (payload: { token: string, password: string, confirmPassword: string }) => {
    isResetPasswordLoading.value = true
    error.value = null
    try {
      await authApi.resetPassword(payload)
    } catch (e) {
      error.value = '重設密碼失敗，連結可能已失效'
      console.error(e)
      throw e
    } finally {
      isResetPasswordLoading.value = false
    }
  }

  const verifyEmail = async (token: string) => {
    isVerifyEmailLoading.value = true
    error.value = null
    try {
      await authApi.verifyEmail(token)
      if (user.value) {
        user.value = { ...user.value, isVerified: true }
      }
    } catch (e) {
      error.value = '驗證失敗，連結可能已失效'
      console.error(e)
      throw e
    } finally {
      isVerifyEmailLoading.value = false
    }
  }

  const resendVerification = async () => {
    isResendVerificationLoading.value = true
    error.value = null
    try {
      await authApi.resendVerification()
    } catch (e) {
      error.value = '發送失敗，請稍後再試'
      console.error(e)
      throw e
    } finally {
      isResendVerificationLoading.value = false
    }
  }

  return {
    user,
    isLoggedIn,
    isLoading,
    error,
    loginWithGoogle,
    register,
    loginWithEmail,
    logout,
    fetchMe,
    setUser,
    isForgotPasswordLoading, isResetPasswordLoading, isVerifyEmailLoading, isResendVerificationLoading,
    forgotPassword, resetPassword, verifyEmail, resendVerification
  }
}, {
  persist: {
    pick: ['user']
  }
})
