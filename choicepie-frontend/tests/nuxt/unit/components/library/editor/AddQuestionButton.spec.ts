// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import AddQuestionButton from '~/components/library/editor/AddQuestionButton.vue'

describe('AddQuestionButton', () => {
  it('顯示文字並在點擊時發出 add', async () => {
    const wrapper = await mountSuspended(AddQuestionButton, { props: { label: 'Add question' } })

    expect(wrapper.text()).toContain('Add question')
    await wrapper.trigger('click')
    expect(wrapper.emitted('add')).toHaveLength(1)
  })

  it('disabled 時停用', async () => {
    const wrapper = await mountSuspended(AddQuestionButton, { props: { label: 'Add', disabled: true } })

    expect(wrapper.attributes('disabled')).toBeDefined()
  })

  it('type="button"，不會送出外層表單', async () => {
    const wrapper = await mountSuspended(AddQuestionButton, { props: { label: 'Add' } })

    expect(wrapper.attributes('type')).toBe('button')
  })
})
