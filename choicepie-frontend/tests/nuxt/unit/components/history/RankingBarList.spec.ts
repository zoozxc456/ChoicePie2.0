// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import RankingBarList from '~/components/history/RankingBarList.vue'

const rankings = [
  { rank: 1, nickname: 'Alice', score: 1000 },
  { rank: 1, nickname: 'Bob', score: 1000 },
  { rank: 3, nickname: 'Cathy', score: 500 },
  { rank: 4, nickname: 'Dan', score: 10 }
]

const rows = async (props: Record<string, unknown> = {}) => {
  const wrapper = await mountSuspended(RankingBarList, { props: { rankings, ...props } })
  return wrapper.findAll('[data-testid="rank-row"]')
}

describe('RankingBarList', () => {
  it('列出所有玩家', async () => {
    expect(await rows()).toHaveLength(4)
  })

  it('名次徽章依 entry.rank：並列第 1 名都顯示獎牌', async () => {
    const items = await rows()
    const badges = items.map(row => row.find('[data-testid="rank-badge"]'))

    expect(badges[0]!.classes()).toContain('bg-cp-primary')
    expect(badges[1]!.classes()).toContain('bg-cp-primary')
    expect(badges[2]!.classes()).toContain('bg-cp-warning')
  })

  it('前三名顯示獎牌圖示，之後顯示名次數字', async () => {
    const items = await rows()

    expect(items[2]!.find('[data-testid="rank-badge"]').text()).toBe('')
    expect(items[3]!.find('[data-testid="rank-badge"]').text()).toBe('4')
  })

  it('分數條寬度以最高分為 100%，且至少 6%', async () => {
    const items = await rows({ rankings: [...rankings].reverse() })
    const widthOf = (nickname: string) =>
      items.find(row => row.text().includes(nickname))!.find('[data-testid="rank-bar"]').attributes('style')

    expect(widthOf('Alice')).toContain('width: 100%')
    expect(widthOf('Cathy')).toContain('width: 50%')
    expect(widthOf('Dan')).toContain('width: 6%')
  })

  it('highlightNickname 的列加上強調背景與後綴', async () => {
    const items = await rows({ highlightNickname: 'Cathy', highlightSuffix: ' (You)' })

    expect(items[2]!.classes()).toContain('bg-cp-primary-light')
    expect(items[2]!.text()).toContain('Cathy (You)')
    expect(items[0]!.classes()).not.toContain('bg-cp-primary-light')
  })
})
