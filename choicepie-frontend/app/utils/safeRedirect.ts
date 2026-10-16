/**
 * 登入後導回的路徑只接受站內路徑，避免 open redirect：
 * 必須以單一 `/` 開頭，不可為 `//host`（protocol-relative），也不可含反斜線
 * （部分瀏覽器會把 `/\host` 視同 `//host`）或控制字元。
 */
export const sanitizeRedirect = (value: unknown, fallback = '/library'): string => {
  const candidate = Array.isArray(value) ? value[0] : value
  if (typeof candidate !== 'string') return fallback
  if (!candidate.startsWith('/') || candidate.startsWith('//')) return fallback
  // eslint-disable-next-line no-control-regex
  if (candidate.includes('\\') || /[\u0000-\u001f]/.test(candidate)) return fallback
  return candidate
}
