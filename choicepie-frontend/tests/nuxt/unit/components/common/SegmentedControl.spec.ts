// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import SegmentedControl from '~/components/common/SegmentedControl.vue'

const options = [
  { value: 'host', label: 'Host' },
  { value: 'player', label: 'Player' }
]

describe('SegmentedControl', () => {
  it('依序渲染每個選項', async () => {
    const wrapper = await mountSuspended(SegmentedControl, { props: { options, modelValue: 'host' } })

    expect(wrapper.findAll('button').map(b => b.text())).toEqual(['Host', 'Player'])
  })

  it('目前選取的選項 aria-pressed 為 true', async () => {
    const wrapper = await mountSuspended(SegmentedControl, { props: { options, modelValue: 'player' } })

    expect(wrapper.findAll('button').map(b => b.attributes('aria-pressed'))).toEqual(['false', 'true'])
  })

  it('點擊選項發出 update:modelValue', async () => {
    const wrapper = await mountSuspended(SegmentedControl, { props: { options, modelValue: 'host' } })

    await wrapper.findAll('button')[1]!.trigger('click')

    expect(wrapper.emitted('update:modelValue')).toEqual([['player']])
  })

  it('按鈕為 type="button"，放在表單內不會觸發送出', async () => {
    const wrapper = await mountSuspended(SegmentedControl, { props: { options, modelValue: 'host' } })

    expect(wrapper.findAll('button').every(b => b.attributes('type') === 'button')).toBe(true)
  })

  it('shape="pill" 使用全圓角', async () => {
    const wrapper = await mountSuspended(SegmentedControl, { props: { options, modelValue: 'host', shape: 'pill' } })

    expect(wrapper.classes()).toContain('rounded-full')
  })
})
