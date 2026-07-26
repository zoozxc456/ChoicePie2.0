export default defineEventHandler(async (event) => {
  const body = await readBody(event)
  const response = await backendFetch(event, '/api/v1/auth/register', { method: 'POST', body })
  return relayBackendResponse(event, response)
})
