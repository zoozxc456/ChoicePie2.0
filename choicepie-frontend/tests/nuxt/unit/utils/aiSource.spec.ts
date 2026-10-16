import { describe, it, expect } from 'vitest'
import { AI_SOURCE_MIN_CHARS, isUrlSource, hasEnoughSource, isSourceTooShort } from '~/utils/aiSource'

describe('isUrlSource', () => {
  it.each([
    ['https://example.com/article', true],
    ['  http://example.com  ', true],
    ['HTTPS://EXAMPLE.COM', true],
    ['https://example.com and more text', false],
    ['example.com', false],
    ['', false]
  ])('%j → %s', (text, expected) => {
    expect(isUrlSource(text)).toBe(expected)
  })
})

describe('hasEnoughSource', () => {
  it(`文字至少 ${AI_SOURCE_MIN_CHARS} 字（不含前後空白）`, () => {
    expect(hasEnoughSource('a'.repeat(AI_SOURCE_MIN_CHARS))).toBe(true)
    expect(hasEnoughSource(` ${'a'.repeat(AI_SOURCE_MIN_CHARS - 1)} `)).toBe(false)
  })

  it('網址不受字數限制', () => {
    expect(hasEnoughSource('https://a.io')).toBe(true)
  })
})

describe('isSourceTooShort', () => {
  it('有輸入但不足字數時提示', () => {
    expect(isSourceTooShort('short')).toBe(true)
  })

  it('空白或網址不提示', () => {
    expect(isSourceTooShort('')).toBe(false)
    expect(isSourceTooShort('https://a.io')).toBe(false)
  })

  it('達到字數不提示', () => {
    expect(isSourceTooShort('a'.repeat(AI_SOURCE_MIN_CHARS))).toBe(false)
  })
})
