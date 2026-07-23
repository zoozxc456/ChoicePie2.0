export default defineNuxtRouteMiddleware(async (to) => {
  const adminAuth = useAdminAuthStore()

  if (!adminAuth.isLoggedIn) {
    await adminAuth.fetchMe()
  }

  if (!adminAuth.isLoggedIn) {
    return navigateTo(`/admin/login?redirect=${encodeURIComponent(to.fullPath)}`)
  }
})
