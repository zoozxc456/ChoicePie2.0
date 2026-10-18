import { describe, it, expect } from 'vitest'
import { buildQuizShareUrl, buildSocialShareUrl } from '~/utils/shareUrl'

describe('buildQuizShareUrl', () => {
  it('以 origin + 題庫路徑 + ref 管道組出分享網址', () => {
    expect(buildQuizShareUrl('https://choicepie.app', 'quiz-1', 'copy'))
      .toBe('https://choicepie.app/library/quiz-1?ref=copy')
  })
})

describe('buildSocialShareUrl', () => {
  const target = 'https://choicepie.app/library/quiz-1?ref=line'

  it('LINE 分享網址會 encode 目標網址', () => {
    expect(buildSocialShareUrl('line', target, '標題'))
      .toBe(`https://social-plugins.line.me/lineit/share?url=${encodeURIComponent(target)}`)
  })

  it('Facebook 分享網址會 encode 目標網址', () => {
    expect(buildSocialShareUrl('facebook', target, '標題'))
      .toBe(`https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(target)}`)
  })

  it('X 分享網址同時帶 encode 過的標題與網址', () => {
    expect(buildSocialShareUrl('x', target, 'A & B'))
      .toBe(`https://twitter.com/intent/tweet?text=${encodeURIComponent('A & B')}&url=${encodeURIComponent(target)}`)
  })
})
