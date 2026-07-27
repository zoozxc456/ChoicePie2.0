export default defineNuxtRouteMiddleware(async (to) => {
  const auth = useAuthStore()

  // persisted 的 user 只代表「曾經登入過」，session cookie 可能早已過期，
  // 所以每次都要用 fetchMe() 向後端實際驗證，不能只看 isLoggedIn 就放行。
  await auth.fetchMe()

  if (!auth.isLoggedIn) {
    return navigateTo(`/login?redirect=${encodeURIComponent(to.fullPath)}`)
  }
})
