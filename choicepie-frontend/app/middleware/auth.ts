export default defineNuxtRouteMiddleware((to) => {
  const auth = useAuthStore()

  if (to.path.startsWith('/host') && !auth.isLoggedIn) {
    return navigateTo(`/login?redirect=${encodeURIComponent(to.fullPath)}`)
  }
})
