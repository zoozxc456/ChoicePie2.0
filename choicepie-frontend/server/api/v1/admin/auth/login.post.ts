interface BackendAdminLoginResult {
  adminUser: unknown
  accessToken: string
  refreshToken: string
}

export default defineEventHandler(async (event): Promise<unknown> => {
  const body = await readBody(event)
  const response = await backendFetch<{ data: BackendAdminLoginResult | null }>(event, '/api/v1/admin/auth/login', {
    method: 'POST',
    body,
    forwardSetCookie: true
  })

  if (response._data?.data) {
    const { accessToken, refreshToken, adminUser } = response._data.data
    setAdminSession(event, { accessToken, refreshToken })
    ;(response._data as { data: unknown }).data = adminUser
  }

  return relayBackendResponse(event, response)
})
