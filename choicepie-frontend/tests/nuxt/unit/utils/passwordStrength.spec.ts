import { describe, it, expect } from 'vitest'
import { evaluatePasswordStrength } from '~/utils/passwordStrength'

describe('evaluatePasswordStrength', () => {
  it('空字串時回傳 null（不顯示強度條）', () => {
    expect(evaluatePasswordStrength('')).toBeNull()
  })

  it('少於 8 碼為 tooShort，即使包含大寫數字符號', () => {
    expect(evaluatePasswordStrength('Ab1!')).toBe('tooShort')
  })

  it('只有小寫字母為 weak', () => {
    expect(evaluatePasswordStrength('abcdefgh')).toBe('weak')
  })

  it.each([
    ['大寫', 'abcdefgH'],
    ['數字', 'abcdefg1'],
    ['符號', 'abcdefg!']
  ])('只多一種（%s）為 medium', (_, password) => {
    expect(evaluatePasswordStrength(password)).toBe('medium')
  })

  it.each([
    ['大寫 + 數字', 'abcdefG1'],
    ['數字 + 符號', 'abcdef1!'],
    ['三種都有', 'abcdeF1!']
  ])('兩種以上（%s）為 strong', (_, password) => {
    expect(evaluatePasswordStrength(password)).toBe('strong')
  })
})
