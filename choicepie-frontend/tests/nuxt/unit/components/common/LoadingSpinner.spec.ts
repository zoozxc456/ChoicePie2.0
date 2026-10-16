// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import LoadingSpinner from '~/components/common/LoadingSpinner.vue'

describe('LoadingSpinner', () => {
  it('以 role="status" 標示載入中，供輔助科技讀取', async () => {
    const wrapper = await mountSuspended(LoadingSpinner)

    expect(wrapper.attributes('role')).toBe('status')
    expect(wrapper.attributes('aria-busy')).toBe('true')
  })

  it('預設為 lg：大圖示與較寬的上下留白', async () => {
    const wrapper = await mountSuspended(LoadingSpinner)

    expect(wrapper.classes()).toContain('py-20')
    expect(wrapper.find('[data-testid="spinner-icon"]').classes()).toEqual(expect.arrayContaining(['animate-spin', 'text-4xl']))
  })

  it('size="md"：較小圖示與較窄的上下留白（後台列表使用）', async () => {
    const wrapper = await mountSuspended(LoadingSpinner, { props: { size: 'md' } })

    expect(wrapper.classes()).toContain('py-16')
    expect(wrapper.find('[data-testid="spinner-icon"]').classes()).toContain('text-3xl')
  })
  it('size="sm"：卡片內的小型載入狀態', async () => {
    const wrapper = await mountSuspended(LoadingSpinner, { props: { size: 'sm' } })

    expect(wrapper.classes()).toContain('py-8')
    expect(wrapper.find('[data-testid="spinner-icon"]').classes()).toContain('text-2xl')
  })
})
