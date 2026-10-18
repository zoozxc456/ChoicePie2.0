import { describe, it, expect } from 'vitest'
import { ALLOWED_IMAGE_TYPES, MAX_IMAGE_BYTES, validateImageFile } from '~/utils/imageFile'

const fileOf = (type: string, size: number) => ({ type, size }) as File

describe('validateImageFile', () => {
  it.each(ALLOWED_IMAGE_TYPES)('%s 在大小限制內時通過', (type) => {
    expect(validateImageFile(fileOf(type, 1024))).toBeNull()
  })

  it('不支援的格式回傳 invalidType', () => {
    expect(validateImageFile(fileOf('image/gif', 1024))).toBe('invalidType')
  })

  it('剛好 5MB 時通過，超過時回傳 tooLarge', () => {
    expect(MAX_IMAGE_BYTES).toBe(5 * 1024 * 1024)
    expect(validateImageFile(fileOf('image/png', MAX_IMAGE_BYTES))).toBeNull()
    expect(validateImageFile(fileOf('image/png', MAX_IMAGE_BYTES + 1))).toBe('tooLarge')
  })

  it('格式與大小都不符時優先回報格式錯誤', () => {
    expect(validateImageFile(fileOf('application/pdf', MAX_IMAGE_BYTES + 1))).toBe('invalidType')
  })

  it('accept 屬性字串與允許格式一致', () => {
    expect(ALLOWED_IMAGE_TYPES.join(',')).toBe('image/jpeg,image/png,image/webp')
  })
})
