export default defineEventHandler(async (event): Promise<unknown> => {
  const path = getRouterParam(event, 'path') ?? ''
  const isAdminPath = path.startsWith('admin/')
  // SSR 時 refresh 之後的 retry 也是讀 cookie：useApi.ts 會把 refresh 的 Set-Cookie 寫回這次 SSR
  // 最外層 request 的 Cookie header，sub-request 複製到的就是新 token。
  const accessToken = isAdminPath ? getAdminAccessToken(event) : getMemberAccessToken(event)

  const method = event.method
  const body = method === 'GET' || method === 'HEAD' ? undefined : await readBody(event)

  const response = await backendFetch(event, `/api/v1/${path}`, {
    method,
    query: getQuery(event),
    body,
    accessToken
  })

  return relayBackendResponse(event, response)
})
