export type PasswordStrength = 'tooShort' | 'weak' | 'medium' | 'strong'

const MIN_LENGTH = 8

/**
 * 註冊頁的密碼強度提示：未滿 8 碼為 tooShort；
 * 其餘依「大寫 / 數字 / 符號」出現種類數判斷，0 種 weak、1 種 medium、2 種以上 strong。
 * 空字串回傳 null（不顯示強度條）。
 */
export const evaluatePasswordStrength = (password: string): PasswordStrength | null => {
  if (password.length === 0) return null
  if (password.length < MIN_LENGTH) return 'tooShort'
  const score = [/[A-Z]/, /[0-9]/, /[^A-Za-z0-9]/].filter(pattern => pattern.test(password)).length
  if (score === 0) return 'weak'
  if (score === 1) return 'medium'
  return 'strong'
}
