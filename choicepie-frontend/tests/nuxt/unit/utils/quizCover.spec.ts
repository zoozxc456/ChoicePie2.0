import { describe, it, expect } from 'vitest'
import { COVER_COLOR_OPTIONS, COVER_EMOJI_OPTIONS, coverColorClass, createDefaultCover } from '~/utils/quizCover'

describe('coverColorClass', () => {
  it.each([
    ['primary', 'bg-cp-primary'],
    ['secondary', 'bg-cp-secondary'],
    ['success', 'bg-cp-success'],
    ['danger', 'bg-cp-danger'],
    ['warning', 'bg-cp-warning'],
    ['info', 'bg-cp-info']
  ])('%s → %s', (color, expected) => {
    expect(coverColorClass(color)).toBe(expected)
  })

  it('未知顏色退回 secondary', () => {
    expect(coverColorClass('unknown')).toBe('bg-cp-secondary')
  })
})

describe('COVER_COLOR_OPTIONS / COVER_EMOJI_OPTIONS', () => {
  it('每個可選顏色都有對應的 class', () => {
    expect(COVER_COLOR_OPTIONS).toEqual(['primary', 'secondary', 'success', 'danger', 'warning', 'info'])
    for (const color of COVER_COLOR_OPTIONS) {
      expect(coverColorClass(color)).toBe(`bg-cp-${color}`)
    }
  })

  it('提供 8 個 emoji，第一個是預設封面 emoji', () => {
    expect(COVER_EMOJI_OPTIONS).toHaveLength(8)
    expect(COVER_EMOJI_OPTIONS[0]).toBe(createDefaultCover().coverEmoji)
  })
})

describe('createDefaultCover', () => {
  it('預設為無圖片、📝、primary', () => {
    expect(createDefaultCover()).toEqual({ coverImageUrl: null, coverEmoji: '📝', coverGradient: 'primary' })
  })

  it('每次回傳新物件，避免頁面間共用同一個 reference', () => {
    expect(createDefaultCover()).not.toBe(createDefaultCover())
  })
})
