// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import QuizMetaForm from '~/components/library/editor/QuizMetaForm.vue'

const mount = (props: Record<string, unknown> = {}) =>
  mountSuspended(QuizMetaForm, { props: { title: 'T', description: 'D', tags: 'a, b', ...props } })

describe('QuizMetaForm', () => {
  it('帶入目前的標題、描述與標籤', async () => {
    const wrapper = await mount()

    expect((wrapper.find('input[data-testid="meta-title"]').element as HTMLInputElement).value).toBe('T')
    expect((wrapper.find('textarea').element as HTMLTextAreaElement).value).toBe('D')
    expect((wrapper.find('input[data-testid="meta-tags"]').element as HTMLInputElement).value).toBe('a, b')
  })

  it('修改欄位時發出對應的 update 事件', async () => {
    const wrapper = await mount()

    await wrapper.find('input[data-testid="meta-title"]').setValue('New')
    await wrapper.find('textarea').setValue('Desc')
    await wrapper.find('input[data-testid="meta-tags"]').setValue('x')

    expect(wrapper.emitted('update:title')?.at(-1)).toEqual(['New'])
    expect(wrapper.emitted('update:description')?.at(-1)).toEqual(['Desc'])
    expect(wrapper.emitted('update:tags')?.at(-1)).toEqual(['x'])
  })

  it('點擊儲存發出 save', async () => {
    const wrapper = await mount()

    await wrapper.find('[data-testid="meta-save"]').trigger('click')

    expect(wrapper.emitted('save')).toHaveLength(1)
  })
})
