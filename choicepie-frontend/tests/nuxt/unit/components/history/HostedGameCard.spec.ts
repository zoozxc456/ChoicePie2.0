// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import HostedGameCard from '~/components/history/HostedGameCard.vue'
import { buildSessionSummary } from '../../fixtures/quiz'

describe('HostedGameCard', () => {
  it('連到該場遊戲的詳情頁', async () => {
    const wrapper = await mountSuspended(HostedGameCard, { props: { session: buildSessionSummary({ id: 'abc' }) } })

    expect(wrapper.find('a').attributes('href')).toBe('/history/abc')
  })

  it('顯示題庫標題與第一名玩家', async () => {
    const wrapper = await mountSuspended(HostedGameCard, { props: { session: buildSessionSummary({ topPlayerName: 'Zoe' }) } })

    expect(wrapper.find('[data-testid="game-title"]').text()).toBe('Space Trivia')
    expect(wrapper.find('[data-testid="top-player"]').text()).toContain('Zoe')
  })
})
