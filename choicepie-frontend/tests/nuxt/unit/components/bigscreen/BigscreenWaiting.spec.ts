// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import BigscreenWaiting from '~/components/bigscreen/BigscreenWaiting.vue'

const joinUrl = 'https://choicepie.app/join?code=ABCD12'

describe('BigscreenWaiting', () => {
  it('QR Code 編碼加入網址', async () => {
    const wrapper = await mountSuspended(BigscreenWaiting, { props: { code: 'ABCD12', joinUrl, playerCount: 0 } })

    expect(wrapper.find('img').attributes('src')).toContain(encodeURIComponent(joinUrl))
  })

  it('大字顯示房間碼', async () => {
    const wrapper = await mountSuspended(BigscreenWaiting, { props: { code: 'ABCD12', joinUrl, playerCount: 0 } })

    expect(wrapper.find('[data-testid="room-code"]').text()).toBe('ABCD12')
  })

  it('顯示目前加入人數', async () => {
    const wrapper = await mountSuspended(BigscreenWaiting, { props: { code: 'ABCD12', joinUrl, playerCount: 17 } })

    expect(wrapper.find('[data-testid="player-count"]').text()).toContain('17')
  })
})
