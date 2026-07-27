export default defineEventHandler(async (event): Promise<unknown> => {
  const refreshToken = getMemberRefreshToken(event)

  const response = await backendFetch(event, '/api/v1/auth/logout', {
    method: 'POST',
    refreshToken,
    forwardSetCookie: true
  })

  clearMemberSession(event)

  return relayBackendResponse(event, response)
})
