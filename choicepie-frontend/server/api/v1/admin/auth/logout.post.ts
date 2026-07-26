export default defineEventHandler(async (event) => {
  const refreshToken = getAdminRefreshToken(event)

  const response = await backendFetch(event, '/api/v1/admin/auth/logout', {
    method: 'POST',
    refreshToken,
    forwardSetCookie: true
  })

  clearAdminSession(event)

  return relayBackendResponse(event, response)
})
