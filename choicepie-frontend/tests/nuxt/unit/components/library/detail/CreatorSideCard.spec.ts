// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import CreatorSideCard from '~/components/library/detail/CreatorSideCard.vue'

const profile = { id: 'creator-1', name: 'Alice', avatar: null, quizCount: 12, challengeCount: 340, isFollowing: false }

const mount = (props: Record<string, unknown> = {}) =>
  mountSuspended(CreatorSideCard, {
    props: { creatorId: 'creator-1', creatorName: 'Alice', profile, canFollow: true, ...props }
  })

describe('CreatorSideCard', () => {
  it('出題者名稱連到出題者頁面', async () => {
    const wrapper = await mount()

    const link = wrapper.find('[data-testid="creator-link"]')
    expect(link.attributes('href')).toBe('/creators/creator-1')
    expect(link.text()).toBe('Alice')
  })

  it('有 profile 時顯示題庫數與挑戰數', async () => {
    const wrapper = await mount()

    const stats = wrapper.find('[data-testid="creator-stats"]').text()
    expect(stats).toContain('12')
    expect(stats).toContain('340')
  })

  it('profile 尚未載入時不顯示統計與追蹤按鈕', async () => {
    const wrapper = await mount({ profile: null })

    expect(wrapper.find('[data-testid="creator-stats"]').exists()).toBe(false)
    expect(wrapper.find('button').exists()).toBe(false)
  })

  it('canFollow 為 false（自己的題庫）時不顯示追蹤按鈕', async () => {
    const wrapper = await mount({ canFollow: false })

    expect(wrapper.find('button').exists()).toBe(false)
  })

  it('點擊追蹤按鈕發出 toggleFollow', async () => {
    const wrapper = await mount()

    await wrapper.find('button').trigger('click')

    expect(wrapper.emitted('toggleFollow')).toHaveLength(1)
  })
})
