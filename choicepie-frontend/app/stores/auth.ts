import type { User } from '~/types/user'

export const useAuthStore = defineStore('auth', () => {
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

  const loginWithEmail = async (email: string, password: string) => {
    isLoading.value = true
    try {
      const data = await $fetch<{ user: User, token: string }>('/api/auth/login', {
        method: 'POST',
        body: { email, password }
      })
      user.value = data.user
    } finally {
      isLoading.value = false
    }
  }

  const logout = async () => {
    try {
      await $fetch('/api/auth/logout', { method: 'POST' })
    } catch {
      // 即使 API 失敗也要清除本地狀態
    } finally {
      user.value = null
      await navigateTo('/login')
    }
  }

  const fetchMe = async () => {
    try {
      const data = await $fetch<User>('/api/auth/me')
      user.value = data
    } catch {
      user.value = null
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
