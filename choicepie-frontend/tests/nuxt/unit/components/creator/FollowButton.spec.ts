// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import FollowButton from '~/components/creator/FollowButton.vue'

describe('FollowButton', () => {
  it('未追蹤時為主色實心按鈕', async () => {
    const wrapper = await mountSuspended(FollowButton, { props: { isFollowing: false } })

    expect(wrapper.attributes('aria-pressed')).toBe('false')
    expect(wrapper.classes()).toContain('bg-cp-primary')
  })

  it('已追蹤時為外框按鈕', async () => {
    const wrapper = await mountSuspended(FollowButton, { props: { isFollowing: true } })

    expect(wrapper.attributes('aria-pressed')).toBe('true')
    expect(wrapper.classes()).toContain('border-cp-border')
    expect(wrapper.classes()).not.toContain('bg-cp-primary')
  })

  it('追蹤與未追蹤顯示不同文字', async () => {
    const following = await mountSuspended(FollowButton, { props: { isFollowing: true } })
    const notFollowing = await mountSuspended(FollowButton, { props: { isFollowing: false } })

    expect(following.text()).not.toBe('')
    expect(following.text()).not.toBe(notFollowing.text())
  })

  it('點擊發出 toggle', async () => {
    const wrapper = await mountSuspended(FollowButton, { props: { isFollowing: false } })

    await wrapper.trigger('click')

    expect(wrapper.emitted('toggle')).toHaveLength(1)
  })

  it('loading 時停用按鈕', async () => {
    const wrapper = await mountSuspended(FollowButton, { props: { isFollowing: false, loading: true } })

    expect(wrapper.attributes('disabled')).toBeDefined()
  })

  it('size="sm" 使用較矮的按鈕', async () => {
    const wrapper = await mountSuspended(FollowButton, { props: { isFollowing: false, size: 'sm' } })

    expect(wrapper.classes()).toContain('h-8')
  })
})
