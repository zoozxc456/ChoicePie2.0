// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import ChipPicker from '~/components/common/ChipPicker.vue'

const options = [
  { value: 3, label: '3' },
  { value: 5, label: '5' },
  { value: 10, label: '10' }
]

describe('ChipPicker', () => {
  it('渲染所有選項並標示目前選取', async () => {
    const wrapper = await mountSuspended(ChipPicker, { props: { options, modelValue: 5 } })

    expect(wrapper.findAll('button').map(b => b.text())).toEqual(['3', '5', '10'])
    expect(wrapper.findAll('button').map(b => b.attributes('aria-pressed'))).toEqual(['false', 'true', 'false'])
  })

  it('點擊發出 update:modelValue', async () => {
    const wrapper = await mountSuspended(ChipPicker, { props: { options, modelValue: 5 } })

    await wrapper.findAll('button')[2]!.trigger('click')

    expect(wrapper.emitted('update:modelValue')).toEqual([[10]])
  })

  it('預設以主色標示選取', async () => {
    const wrapper = await mountSuspended(ChipPicker, { props: { options, modelValue: 3 } })

    expect(wrapper.findAll('button')[0]!.classes()).toContain('bg-primary-500')
  })

  it('tone="secondary" 以次要色標示選取', async () => {
    const wrapper = await mountSuspended(ChipPicker, { props: { options, modelValue: 3, tone: 'secondary' } })

    expect(wrapper.findAll('button')[0]!.classes()).toContain('bg-secondary-800')
  })

  it('按鈕為 type="button"', async () => {
    const wrapper = await mountSuspended(ChipPicker, { props: { options, modelValue: 3 } })

    expect(wrapper.findAll('button').every(b => b.attributes('type') === 'button')).toBe(true)
  })
})
