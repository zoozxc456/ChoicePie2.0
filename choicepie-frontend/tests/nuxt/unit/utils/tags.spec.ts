import { describe, it, expect } from 'vitest'
import { formatTags, parseTags } from '~/utils/tags'

describe('parseTags', () => {
  it('以逗號切分並去除前後空白', () => {
    expect(parseTags(' 數學 , 國中,  代數 ')).toEqual(['數學', '國中', '代數'])
  })

  it('略過空白項目', () => {
    expect(parseTags('a,, ,b,')).toEqual(['a', 'b'])
  })

  it('空字串回傳空陣列', () => {
    expect(parseTags('')).toEqual([])
  })
})

describe('formatTags', () => {
  it('以「, 」串接成輸入框字串，可再被 parseTags 還原', () => {
    const tags = ['數學', '國中']
    expect(formatTags(tags)).toBe('數學, 國中')
    expect(parseTags(formatTags(tags))).toEqual(tags)
  })
})
