// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import StatusBadge from '~/components/common/StatusBadge.vue'

describe('StatusBadge', () => {
  it('顯示 slot 內容', async () => {
    const wrapper = await mountSuspended(StatusBadge, { slots: { default: () => 'Published' } })

    expect(wrapper.text()).toBe('Published')
  })

  it('預設為 neutral 色調', async () => {
    const wrapper = await mountSuspended(StatusBadge, { slots: { default: () => 'Draft' } })

    expect(wrapper.classes()).toEqual(expect.arrayContaining(['bg-neutral-100', 'text-neutral-600']))
  })

  it.each([
    ['success', 'bg-success-100', 'text-success-800'],
    ['warning', 'bg-warning-100', 'text-warning-800'],
    ['error', 'bg-error-100', 'text-error-800'],
    ['primary', 'bg-cp-primary-light', 'text-cp-primary']
  ] as const)('tone="%s" 套用對應色', async (tone, bg, text) => {
    const wrapper = await mountSuspended(StatusBadge, { props: { tone }, slots: { default: () => 'x' } })

    expect(wrapper.classes()).toEqual(expect.arrayContaining([bg, text]))
  })
})
