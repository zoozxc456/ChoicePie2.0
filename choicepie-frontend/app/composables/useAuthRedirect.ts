import { computed } from 'vue'
import { sanitizeRedirect } from '~/utils/safeRedirect'

/** 會員登入 / 註冊頁共用：登入成功後導回 redirect query（僅限站內路徑） */
export const useAuthRedirect = () => {
  const auth = useAuthStore()
  const route = useRoute()

  const redirect = computed(() => sanitizeRedirect(route.query.redirect))

  const goToRedirect = () => navigateTo(redirect.value)

  // persisted 的 isLoggedIn 只代表「曾經登入過」，須用 fetchMe() 實際驗證 session 是否仍有效，
  // 避免 session 過期時在登入頁與受保護頁之間形成 redirect loop。
  const redirectIfAuthenticated = async () => {
    if (!auth.isLoggedIn) return
    if (await auth.fetchMe()) {
      await goToRedirect()
    }
  }

  return { redirect, goToRedirect, redirectIfAuthenticated }
}
