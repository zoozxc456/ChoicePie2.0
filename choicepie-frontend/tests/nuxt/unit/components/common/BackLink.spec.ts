// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import BackLink from '~/components/common/BackLink.vue'

describe('BackLink', () => {
  it('連到指定路徑並顯示文字', async () => {
    const wrapper = await mountSuspended(BackLink, { props: { to: '/library', label: 'Back to library' } })

    expect(wrapper.find('a').attributes('href')).toBe('/library')
    expect(wrapper.text()).toBe('Back to library')
  })

  it('顯示返回箭頭圖示', async () => {
    const wrapper = await mountSuspended(BackLink, { props: { to: '/library', label: 'Back' } })

    expect(wrapper.find('[data-testid="back-link-icon"]').exists()).toBe(true)
  })
})
