interface BackendLoginResult {
  member: unknown
  accessToken: string
  refreshToken: string
}

export default defineEventHandler(async (event) => {
  const refreshToken = getMemberRefreshToken(event)
  if (!refreshToken) {
    setResponseStatus(event, 401)
    return { code: 'UNAUTHORIZED', status: false, data: null, message: 'Unauthorized' }
  }

  const response = await dedupeMemberRefresh(refreshToken, () =>
    backendFetch<{ data: BackendLoginResult | null }>(event, '/api/v1/auth/refresh', {
      method: 'POST',
      refreshToken,
      forwardSetCookie: true
    }))

  if (response._data?.data) {
    const { accessToken, refreshToken: newRefreshToken, member } = response._data.data
    setMemberSession(event, { accessToken, refreshToken: newRefreshToken })
    ;(response._data as { data: unknown }).data = member
  } else if (response.status === 401) {
    clearMemberSession(event)
  }

  return relayBackendResponse(event, response)
})
