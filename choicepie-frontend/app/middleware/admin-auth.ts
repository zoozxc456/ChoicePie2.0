export default defineNuxtRouteMiddleware(async (to) => {
  const adminAuth = useAdminAuthStore()

  // persisted 的 adminUser 只代表「曾經登入過」，session cookie 可能早已過期，
  // 所以每次都要用 fetchMe() 向後端實際驗證，不能只看 isLoggedIn 就放行。
  const { success: isLoggedIn } = await adminAuth.fetchMe()

  if (!isLoggedIn) {
    return navigateTo(`/admin/login?redirect=${encodeURIComponent(to.fullPath)}`)
  }
})
