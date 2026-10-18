const isDeletion = (attributes: string[]) => attributes.some((attribute) => {
  const [key = '', value = ''] = attribute.split('=', 2).map(part => part.trim())
  if (key.toLowerCase() === 'max-age') return Number(value) <= 0
  if (key.toLowerCase() === 'expires') return Date.parse(value) <= Date.now()
  return false
})

/**
 * 把一組 Set-Cookie 套用到 request 的 Cookie header 上，得到瀏覽器收到這些 Set-Cookie 後下一次會送出的 Cookie header：
 * 同名覆寫、Max-Age<=0 或 Expires 已過期視為刪除，其餘 cookie 保持原順序。
 */
export const applySetCookiesToCookieHeader = (cookieHeader: string | undefined, setCookies: string[]): string => {
  const cookies = new Map<string, string>()
  for (const pair of (cookieHeader ?? '').split(';')) {
    const separator = pair.indexOf('=')
    if (separator <= 0) continue
    cookies.set(pair.slice(0, separator).trim(), pair.slice(separator + 1).trim())
  }

  for (const setCookie of setCookies) {
    const [pair = '', ...attributes] = setCookie.split(';')
    const separator = pair.indexOf('=')
    if (separator <= 0) continue
    const name = pair.slice(0, separator).trim()

    if (isDeletion(attributes)) {
      cookies.delete(name)
    } else {
      cookies.set(name, pair.slice(separator + 1).trim())
    }
  }

  return [...cookies].map(([name, value]) => `${name}=${value}`).join('; ')
}
