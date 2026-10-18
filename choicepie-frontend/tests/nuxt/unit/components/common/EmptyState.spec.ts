// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import EmptyState from '~/components/common/EmptyState.vue'

describe('EmptyState', () => {
  it('顯示標題與說明', async () => {
    const wrapper = await mountSuspended(EmptyState, { props: { title: 'Nothing here', description: 'Try later' } })

    expect(wrapper.find('[data-testid="empty-title"]').text()).toBe('Nothing here')
    expect(wrapper.find('[data-testid="empty-description"]').text()).toBe('Try later')
  })

  it('未提供說明時不渲染說明段落', async () => {
    const wrapper = await mountSuspended(EmptyState, { props: { title: 'Nothing here' } })

    expect(wrapper.find('[data-testid="empty-description"]').exists()).toBe(false)
  })

  it('提供 icon 時顯示圖示', async () => {
    const wrapper = await mountSuspended(EmptyState, { props: { title: 't', icon: 'i-lucide-heart' } })

    expect(wrapper.find('[data-testid="empty-icon"]').exists()).toBe(true)
    expect(wrapper.find('[data-testid="empty-emoji"]').exists()).toBe(false)
  })

  it('提供 emoji 時顯示搖擺的 emoji', async () => {
    const wrapper = await mountSuspended(EmptyState, { props: { title: 't', emoji: '🥧' } })

    const emoji = wrapper.find('[data-testid="empty-emoji"]')
    expect(emoji.text()).toBe('🥧')
    expect(emoji.classes()).toContain('wobble')
  })

  it('預設 slot 用來放行動按鈕', async () => {
    const wrapper = await mountSuspended(EmptyState, {
      props: { title: 't' },
      slots: { default: () => 'CTA' }
    })

    expect(wrapper.find('[data-testid="empty-actions"]').text()).toBe('CTA')
  })

  it('沒有 slot 時不渲染行動區塊', async () => {
    const wrapper = await mountSuspended(EmptyState, { props: { title: 't' } })

    expect(wrapper.find('[data-testid="empty-actions"]').exists()).toBe(false)
  })
})
