// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import CreateMethodCard from '~/components/library/editor/CreateMethodCard.vue'

const props = {
  to: '/library/new/ai',
  icon: 'i-lucide-sparkles',
  title: 'AI',
  description: 'Generate with AI',
  ctaLabel: 'Start'
}

describe('CreateMethodCard', () => {
  it('連到指定的出題方式並顯示內容', async () => {
    const wrapper = await mountSuspended(CreateMethodCard, { props })

    expect(wrapper.find('a').attributes('href')).toBe('/library/new/ai')
    expect(wrapper.find('[data-testid="method-title"]').text()).toBe('AI')
    expect(wrapper.find('[data-testid="method-description"]').text()).toBe('Generate with AI')
    expect(wrapper.find('[data-testid="method-cta"]').text()).toBe('Start')
  })

  it('可用時有 hover 浮起效果', async () => {
    const wrapper = await mountSuspended(CreateMethodCard, { props })

    expect(wrapper.find('a').classes()).toContain('hover:-translate-y-0.5')
  })

  it('dimmed 時淡化且沒有 hover 效果', async () => {
    const wrapper = await mountSuspended(CreateMethodCard, { props: { ...props, dimmed: true } })

    expect(wrapper.find('a').classes()).toContain('opacity-60')
    expect(wrapper.find('a').classes()).not.toContain('hover:-translate-y-0.5')
  })

  it('badge slot 顯示在說明與按鈕之間', async () => {
    const wrapper = await mountSuspended(CreateMethodCard, { props, slots: { badge: () => 'Quota left' } })

    expect(wrapper.text()).toContain('Quota left')
  })
})
