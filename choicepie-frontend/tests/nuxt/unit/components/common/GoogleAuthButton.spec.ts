// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import GoogleAuthButton from '~/components/common/GoogleAuthButton.vue'

describe('GoogleAuthButton', () => {
  it('顯示文字並在點擊時發出 click', async () => {
    const wrapper = await mountSuspended(GoogleAuthButton, { props: { label: 'Continue with Google' } })

    expect(wrapper.text()).toContain('Continue with Google')
    await wrapper.find('button').trigger('click')
    expect(wrapper.emitted('click')).toHaveLength(1)
  })

  it('type="button"，放在表單旁不會觸發送出', async () => {
    const wrapper = await mountSuspended(GoogleAuthButton, { props: { label: 'G' } })

    expect(wrapper.find('button').attributes('type')).toBe('button')
  })
})
