interface BackendAdminLoginResult {
  adminUser: unknown
  accessToken: string
  refreshToken: string
}

export default defineEventHandler(async (event): Promise<unknown> => {
  const refreshToken = getAdminRefreshToken(event)
  if (!refreshToken) {
    setResponseStatus(event, 401)
    return { code: 'UNAUTHORIZED', status: false, data: null, message: 'Unauthorized' }
  }

  const response = await dedupeAdminRefresh(refreshToken, () =>
    backendFetch<{ data: BackendAdminLoginResult | null }>(event, '/api/v1/admin/auth/refresh', {
      method: 'POST',
      refreshToken,
      forwardSetCookie: true
    }))

  if (response._data?.data) {
    const { accessToken, refreshToken: newRefreshToken, adminUser } = response._data.data
    setAdminSession(event, { accessToken, refreshToken: newRefreshToken })
    // token 只放在 httpOnly cookie，不回傳給瀏覽器端 JS（與會員的 refresh 一致）。
    ;(response._data as { data: unknown }).data = adminUser
  } else if (response.status === 401) {
    clearAdminSession(event)
  }

  return relayBackendResponse(event, response)
})
