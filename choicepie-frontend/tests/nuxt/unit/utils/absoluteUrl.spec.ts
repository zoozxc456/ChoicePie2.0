import { describe, it, expect } from 'vitest'
import { toAbsoluteUrl } from '~/utils/absoluteUrl'

describe('toAbsoluteUrl', () => {
  it('絕對網址原樣回傳', () => {
    expect(toAbsoluteUrl('https://cdn.example.com/a.png', 'https://choicepie.app')).toBe('https://cdn.example.com/a.png')
  })

  it('相對路徑以 origin 補成絕對網址', () => {
    expect(toAbsoluteUrl('/covers/a.png', 'https://choicepie.app')).toBe('https://choicepie.app/covers/a.png')
  })

  it('空值回傳 undefined', () => {
    expect(toAbsoluteUrl(null, 'https://choicepie.app')).toBeUndefined()
    expect(toAbsoluteUrl(undefined, 'https://choicepie.app')).toBeUndefined()
    expect(toAbsoluteUrl('', 'https://choicepie.app')).toBeUndefined()
  })

  it('無效網址回傳 undefined', () => {
    expect(toAbsoluteUrl('http://', 'https://choicepie.app')).toBeUndefined()
  })
})
