export default defineEventHandler(async (event): Promise<unknown> => {
  const accessToken = getHeader(event, 'x-fresh-access-token') ?? getMemberAccessToken(event)
  const formData = await toFormData(event)

  const response = await backendFetch(event, '/api/v1/account/avatar', {
    method: 'POST',
    body: formData,
    accessToken
  })

  return relayBackendResponse(event, response)
})
