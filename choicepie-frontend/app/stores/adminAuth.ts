import { useAdminAuthClientApi } from '~/services/admin/auth'
import type { AdminUser } from '~/types/adminUser'
import type { AdminUserDto } from '~/types/api'
import type { AdminLoginSchema } from '~/types/adminAuth'

const toAdminUser = (dto: AdminUserDto): AdminUser => ({
  id: dto.id,
  email: dto.email,
  name: dto.name,
  role: dto.role,
  isVerified: dto.isVerified,
  createdAt: dto.createdAt
})

export const useAdminAuthStore = defineStore('adminAuth', () => {
  const adminAuthApi = useAdminAuthClientApi()

  const adminUser = ref<AdminUser | null>(null)
  const isLoggedIn = computed(() => !!adminUser.value)
  const isLoading = ref(false)

  const loginWithEmail = async (payload: AdminLoginSchema) => {
    isLoading.value = true
    try {
      const dto = await adminAuthApi.loginWithEmail(payload)
      adminUser.value = toAdminUser(dto)
    } finally {
      isLoading.value = false
    }
  }

  const logout = async (redirectTo = '/admin/login') => {
    try {
      await adminAuthApi.logout()
    } catch {
      // 即使 API 失敗也要清除本地狀態
    } finally {
      adminUser.value = null
      await navigateTo(redirectTo)
    }
  }

  const fetchMe = async () => {
    try {
      const dto = await adminAuthApi.refresh()
      adminUser.value = toAdminUser(dto)
      return true
    } catch {
      adminUser.value = null
      return false
    }
  }

  return {
    adminUser,
    isLoggedIn,
    isLoading,
    loginWithEmail,
    logout,
    fetchMe
  }
}, {
  persist: {
    pick: ['adminUser']
  }
})
