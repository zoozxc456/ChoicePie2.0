// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import PasswordInput from '~/components/common/PasswordInput.vue'

describe('PasswordInput', () => {
  it('預設以密碼型態遮蔽輸入', async () => {
    const wrapper = await mountSuspended(PasswordInput, { props: { modelValue: '' } })

    expect(wrapper.find('input').attributes('type')).toBe('password')
  })

  it('點擊切換鈕後顯示明碼，再點一次恢復遮蔽', async () => {
    const wrapper = await mountSuspended(PasswordInput, { props: { modelValue: '' } })
    const toggle = wrapper.find('[data-testid="password-visibility-toggle"]')

    await toggle.trigger('click')
    expect(wrapper.find('input').attributes('type')).toBe('text')

    await toggle.trigger('click')
    expect(wrapper.find('input').attributes('type')).toBe('password')
  })

  it('切換時發出 update:visible，讓多個欄位可共用同一個顯示狀態', async () => {
    const wrapper = await mountSuspended(PasswordInput, { props: { modelValue: '', visible: false } })

    await wrapper.find('[data-testid="password-visibility-toggle"]').trigger('click')

    expect(wrapper.emitted('update:visible')).toEqual([[true]])
  })

  it('由外部控制 visible 時依 prop 顯示', async () => {
    const wrapper = await mountSuspended(PasswordInput, { props: { modelValue: '', visible: true } })

    expect(wrapper.find('input').attributes('type')).toBe('text')
  })

  it('輸入時發出 update:modelValue', async () => {
    const wrapper = await mountSuspended(PasswordInput, { props: { modelValue: '' } })

    await wrapper.find('input').setValue('secret123')

    expect(wrapper.emitted('update:modelValue')?.at(-1)).toEqual(['secret123'])
  })

  it('showToggle=false 時不顯示切換鈕', async () => {
    const wrapper = await mountSuspended(PasswordInput, { props: { modelValue: '', showToggle: false } })

    expect(wrapper.find('[data-testid="password-visibility-toggle"]').exists()).toBe(false)
  })

  it('傳入 placeholder', async () => {
    const wrapper = await mountSuspended(PasswordInput, { props: { modelValue: '', placeholder: 'Password' } })

    expect(wrapper.find('input').attributes('placeholder')).toBe('Password')
  })
})
