// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import GameInfoCard from '~/components/history/GameInfoCard.vue'
import { buildSessionDetail } from '../../fixtures/quiz'

describe('GameInfoCard', () => {
  it('顯示題庫標題', async () => {
    const wrapper = await mountSuspended(GameInfoCard, { props: { session: buildSessionDetail() } })

    expect(wrapper.find('h1').text()).toBe('Space Trivia')
  })

  it('主持人與玩家顯示不同的身分徽章', async () => {
    const host = await mountSuspended(GameInfoCard, { props: { session: buildSessionDetail({ isHost: true }) } })
    const player = await mountSuspended(GameInfoCard, { props: { session: buildSessionDetail({ isHost: false }) } })

    const hostBadge = host.find('[data-testid="role-badge"]')
    const playerBadge = player.find('[data-testid="role-badge"]')
    expect(hostBadge.classes()).toContain('text-cp-primary')
    expect(playerBadge.classes()).toContain('text-cp-info')
    expect(hostBadge.text()).not.toBe(playerBadge.text())
  })

  it('顯示玩家數與題數', async () => {
    const wrapper = await mountSuspended(GameInfoCard, { props: { session: buildSessionDetail({ playerCount: 7, questionCount: 4 }) } })

    const meta = wrapper.find('[data-testid="game-meta"]').text()
    expect(meta).toContain('7')
    expect(meta).toContain('4')
  })
})
