import { useAuthClientApi } from '~/services/auth'
import type { User } from '~/types/user'
import type { MemberDto } from '~/types/api'
import type { LoginSchema, RegisterSchema } from '~/types/auth'

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
      // TODO: redirect to /api/auth/google
      await navigateTo('/api/auth/google', { external: true })
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

  return {
    user,
    isLoggedIn,
    isLoading,
    loginWithGoogle,
    register,
    loginWithEmail,
    logout,
    fetchMe,
    setUser
  }
}, {
  persist: {
    pick: ['user']
  }
})
