export default defineEventHandler(async (event): Promise<unknown> => {
  const path = getRouterParam(event, 'path') ?? ''
  const isAdminPath = path.startsWith('admin/')
  // retry 請求會透過 x-fresh-access-token 顯式帶上剛 refresh 出來的新 token（見 useApi.ts）——
  // 這種情況下不能 fallback 讀 cookie：retry 屬於全新的 sub-request（Nitro 對 useRequestFetch()
  // 打自己的 server route 一律建立新的 H3Event），cookie header 仍是這次 SSR 最外層 request 一開始
  // 帶進來的舊值，讀了只會拿到 refresh 前的過期 token，導致 retry 必定又 401。
  const freshAccessToken = getHeader(event, 'x-fresh-access-token')
  const accessToken = freshAccessToken ?? (isAdminPath ? getAdminAccessToken(event) : getMemberAccessToken(event))

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
