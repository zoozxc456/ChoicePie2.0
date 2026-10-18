// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import AiSourceInput from '~/components/library/editor/AiSourceInput.vue'

const mount = (modelValue: string) => mountSuspended(AiSourceInput, { props: { modelValue } })

describe('AiSourceInput', () => {
  it('輸入時發出 update:modelValue', async () => {
    const wrapper = await mount('')

    await wrapper.find('textarea').setValue('Hello')

    expect(wrapper.emitted('update:modelValue')?.at(-1)).toEqual(['Hello'])
  })

  it('一般文字顯示字數', async () => {
    const wrapper = await mount('Hello world')

    expect(wrapper.find('[data-testid="source-hint"]').text()).toContain('11')
  })

  it('網址顯示網址提示而非字數', async () => {
    const urlWrapper = await mount('https://example.com')
    const textWrapper = await mount('https://example.com and text')

    expect(urlWrapper.find('[data-testid="source-hint"]').text()).not.toBe(textWrapper.find('[data-testid="source-hint"]').text())
    expect(urlWrapper.find('[data-testid="source-hint"]').text()).not.toContain('19')
  })

  it('字數不足時顯示最少字數提醒', async () => {
    const wrapper = await mount('short')

    expect(wrapper.find('[data-testid="min-chars-hint"]').exists()).toBe(true)
  })

  it('空白或網址不顯示最少字數提醒', async () => {
    expect((await mount('')).find('[data-testid="min-chars-hint"]').exists()).toBe(false)
    expect((await mount('https://example.com')).find('[data-testid="min-chars-hint"]').exists()).toBe(false)
  })

  it('有內容時外框改為主色', async () => {
    expect((await mount('x')).find('textarea').classes()).toContain('border-primary-500')
    expect((await mount('')).find('textarea').classes()).toContain('border-neutral-200')
  })
})
