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

  // 只清除本地狀態，不做導頁——導頁需要 Nuxt context，
  // 由呼叫端（如 useApi.ts）在自己持有 context 的地方處理，避免跨 await 邊界後 context 遺失。
  const clearSession = () => {
    adminUser.value = null
  }

  const logout = async (redirectTo = '/admin/login') => {
    try {
      await adminAuthApi.logout()
    } catch {
      // 即使 API 失敗也要清除本地狀態
    } finally {
      clearSession()
      await navigateTo(redirectTo)
    }
  }

  // 先用 /me（純驗證 access token，不會動 refresh token）確認登入狀態；
  // 只有 access token 已過期（/me 失敗）時才 fallback 打 /refresh 換發新 token。
  // 避免每次進頁面都 rotate refresh token，導致同一次 SSR render 內其他請求
  // 用到已被替換的舊 cookie 而失敗。
  const fetchMe = async () => {
    try {
      const dto = await adminAuthApi.me()
      adminUser.value = toAdminUser(dto)
      return true
    } catch {
      // 忽略，繼續嘗試 refresh
    }

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
    clearSession,
    fetchMe
  }
}, {
  persist: {
    pick: ['adminUser']
  }
})
