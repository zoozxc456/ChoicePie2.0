// 變更密碼需要帶上目前 session 的 refresh token（通用 catch-all 不會轉發它），
// 後端才能在撤銷其他裝置 session 時保留這一個。
export default defineEventHandler(async (event): Promise<unknown> => {
  const accessToken = getMemberAccessToken(event)
  const body = await readBody(event)

  const response = await backendFetch(event, '/api/v1/account/password', {
    method: 'PUT',
    body,
    accessToken,
    refreshToken: getMemberRefreshToken(event)
  })

  return relayBackendResponse(event, response)
})
