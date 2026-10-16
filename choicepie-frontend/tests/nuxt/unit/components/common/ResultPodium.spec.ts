// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import ResultPodium from '~/components/common/ResultPodium.vue'

const rankings = [
  { rank: 3, nickname: 'Bronze', score: 100 },
  { rank: 1, nickname: 'Gold', score: 1500 },
  { rank: 2, nickname: 'Silver', score: 900 },
  { rank: 4, nickname: 'Fourth', score: 50 }
]

const slots = async (props: Record<string, unknown> = {}) => {
  const wrapper = await mountSuspended(ResultPodium, { props: { rankings, ...props } })
  return { wrapper, items: wrapper.findAll('[data-testid="podium-slot"]') }
}

describe('ResultPodium', () => {
  it('只呈現前三名，順序為 銀 / 金 / 銅', async () => {
    const { items } = await slots()

    expect(items.map(item => item.attributes('data-place'))).toEqual(['2', '1', '3'])
  })

  it('每個名次顯示依 entry.rank 對應的玩家與分數', async () => {
    const { items } = await slots()
    const gold = items.find(item => item.attributes('data-place') === '1')!

    expect(gold.find('[data-testid="podium-nickname"]').text()).toBe('Gold')
    expect(gold.find('[data-testid="podium-score"]').text()).toBe((1500).toLocaleString())
  })

  it('柱子上的數字等於該名次', async () => {
    const { items } = await slots()

    expect(items.map(item => item.find('[data-testid="podium-bar"]').text())).toEqual(['2', '1', '3'])
  })

  it('冠軍才有光暈', async () => {
    const { items } = await slots()
    const glowing = items.filter(item => item.find('[data-testid="podium-medal"]').classes().includes('rank-1'))

    expect(glowing).toHaveLength(1)
    expect(glowing[0]!.attributes('data-place')).toBe('1')
  })

  it('highlightNickname 會加上後綴並標示強調色', async () => {
    const { items } = await slots({ highlightNickname: 'Silver', highlightSuffix: ' (You)' })
    const silver = items.find(item => item.attributes('data-place') === '2')!
    const gold = items.find(item => item.attributes('data-place') === '1')!

    expect(silver.find('[data-testid="podium-nickname"]').text()).toBe('Silver (You)')
    expect(silver.find('[data-testid="podium-nickname"]').classes()).toContain('text-cp-primary')
    expect(gold.find('[data-testid="podium-nickname"]').text()).toBe('Gold')
  })

  it('animated 為 false 時不套用進場動畫 class', async () => {
    const { items } = await slots()

    expect(items[0]!.classes()).not.toContain('podium-item--animated')
  })

  it('animated 為 true 時套用進場動畫 class', async () => {
    const { items } = await slots({ animated: true })

    expect(items[0]!.classes()).toContain('podium-item--animated')
  })

  it('排名為空時不渲染任何名次', async () => {
    const { items } = await slots({ rankings: [] })

    expect(items).toHaveLength(0)
  })
})
