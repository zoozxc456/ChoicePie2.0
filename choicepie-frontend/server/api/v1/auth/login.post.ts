interface BackendLoginResult {
  member: unknown
  accessToken: string
  refreshToken: string
}

export default defineEventHandler(async (event): Promise<unknown> => {
  const body = await readBody(event)
  const response = await backendFetch<{ data: BackendLoginResult | null }>(event, '/api/v1/auth/login', {
    method: 'POST',
    body,
    forwardSetCookie: true
  })

  if (response._data?.data) {
    const { accessToken, refreshToken, member } = response._data.data
    setMemberSession(event, { accessToken, refreshToken })
    ;(response._data as { data: unknown }).data = member
  }

  return relayBackendResponse(event, response)
})
