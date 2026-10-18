// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import PlayedGameCard from '~/components/history/PlayedGameCard.vue'
import { buildSessionSummary } from '../../fixtures/quiz'

const badgeOf = async (myRank: number | null) => {
  const wrapper = await mountSuspended(PlayedGameCard, { props: { session: buildSessionSummary({ myRank }) } })
  return wrapper.find('[data-testid="my-rank-badge"]')
}

describe('PlayedGameCard', () => {
  it('連到該場遊戲的詳情頁', async () => {
    const wrapper = await mountSuspended(PlayedGameCard, { props: { session: buildSessionSummary({ id: 'xyz' }) } })

    expect(wrapper.find('a').attributes('href')).toBe('/history/xyz')
  })

  it('前三名顯示獎牌與對應顏色、光暈', async () => {
    const badge = await badgeOf(2)

    expect(badge.classes()).toEqual(expect.arrayContaining(['bg-cp-info', 'rank-2']))
    expect(badge.text()).toBe('')
  })

  it('第四名之後顯示 #名次', async () => {
    const badge = await badgeOf(5)

    expect(badge.classes()).toContain('bg-neutral-400')
    expect(badge.text()).toBe('#5')
  })

  it('沒有名次資料時顯示 #0', async () => {
    expect((await badgeOf(null)).text()).toBe('#0')
  })
})
