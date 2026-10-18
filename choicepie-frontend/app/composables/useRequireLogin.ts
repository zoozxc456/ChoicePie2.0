/**
 * 需要登入的動作統一在這裡攔截：未登入者導向登入頁，登入後導回目前頁面。
 * 回傳 false 時呼叫端應中止動作。
 */
export const useRequireLogin = () => {
  const auth = useAuthStore()
  const route = useRoute()

  const ensureLoggedIn = async () => {
    if (auth.isLoggedIn) return true
    await navigateTo(`/login?redirect=${encodeURIComponent(route.fullPath)}`)
    return false
  }

  return { ensureLoggedIn }
}
