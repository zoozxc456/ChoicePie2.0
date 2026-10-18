// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import BigscreenEnded from '~/components/bigscreen/BigscreenEnded.vue'

const rankings = [
  { rank: 1, nickname: 'Alice', score: 1000 },
  { rank: 1, nickname: 'Bob', score: 1000 },
  { rank: 3, nickname: 'Cathy', score: 500 },
  { rank: 4, nickname: 'Dan', score: 100 }
]

describe('BigscreenEnded', () => {
  it('頒獎台依 entry.rank 取人，柱子數字等於名次', async () => {
    const wrapper = await mountSuspended(BigscreenEnded, { props: { rankings } })
    const slots = wrapper.findAll('[data-testid="podium-slot"]')

    expect(slots.map(s => s.attributes('data-place'))).toEqual(['1', '3'])
    expect(slots.map(s => s.find('[data-testid="podium-bar"]').text())).toEqual(['1', '3'])
    expect(slots[0]!.text()).toContain('Alice')
  })

  it('未上頒獎台的玩家（含並列者）列在下方，名次取自 entry.rank', async () => {
    const wrapper = await mountSuspended(BigscreenEnded, { props: { rankings } })
    const others = wrapper.findAll('[data-testid="other-player"]')

    expect(others.map(o => o.find('[data-testid="other-nickname"]').text())).toEqual(['Bob', 'Dan'])
    expect(others[0]!.find('[data-testid="other-rank"]').text()).toContain('1')
    expect(others[1]!.find('[data-testid="other-rank"]').text()).toContain('4')
  })
})
