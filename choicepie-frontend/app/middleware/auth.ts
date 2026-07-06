export default defineNuxtRouteMiddleware(() => {
  const auth = useAuthStore()

  // 暫時用假使用者，之後接上真實 API 後移除
  if (!auth.isLoggedIn) {
    auth.setUser({
      id: 'mock-user-1',
      email: 'mingyu@example.com',
      name: 'Mingyu',
      isVerified: true,
      createdAt: '2026-01-01T00:00:00.000Z'
    })
  }
})
