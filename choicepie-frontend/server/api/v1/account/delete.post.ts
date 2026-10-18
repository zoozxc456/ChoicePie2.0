// 刪除帳號：轉成後端 DELETE /account（需要 request body），成功後清除 BFF 的 session cookie，
// 並轉發後端的 Set-Cookie（清掉瀏覽器直連 SignalR 用的後端 cookie），行為與 logout 一致。
export default defineEventHandler(async (event): Promise<unknown> => {
  const accessToken = getMemberAccessToken(event)
  const body = await readBody(event)

  const response = await backendFetch(event, '/api/v1/account', {
    method: 'DELETE',
    body,
    accessToken,
    forwardSetCookie: true
  })

  // 只有真的刪除成功才清 session；密碼錯誤等失敗要保留登入狀態讓使用者重試。
  if (response.status >= 200 && response.status < 300) {
    clearMemberSession(event)
  }

  return relayBackendResponse(event, response)
})
