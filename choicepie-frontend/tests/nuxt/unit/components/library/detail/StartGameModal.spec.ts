// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import StartGameModal from '~/components/library/detail/StartGameModal.vue'
import { buildQuiz } from '../../../fixtures/quiz'

const mount = (props: Record<string, unknown> = {}) =>
  mountSuspended(StartGameModal, { props: { open: true, quiz: buildQuiz(), ...props } })

const timeOptions = (wrapper: Awaited<ReturnType<typeof mount>>) => wrapper.findAll('[data-testid="time-limit-option"]')

describe('StartGameModal', () => {
  it('open 為 false 時不渲染對話框', async () => {
    const wrapper = await mount({ open: false })

    expect(wrapper.find('[role="dialog"]').exists()).toBe(false)
  })

  it('提供 10 / 20 / 30 / 60 秒選項，預設 20 秒', async () => {
    const wrapper = await mount()

    expect(timeOptions(wrapper).map(o => o.attributes('data-seconds'))).toEqual(['10', '20', '30', '60'])
    expect(timeOptions(wrapper).map(o => o.attributes('aria-pressed'))).toEqual(['false', 'true', 'false', 'false'])
  })

  it('建立房間時帶出選取的秒數', async () => {
    const wrapper = await mount()

    await timeOptions(wrapper)[2]!.trigger('click')
    await wrapper.find('[data-testid="create-room"]').trigger('click')

    expect(wrapper.emitted('create')).toEqual([[30]])
  })

  it('按取消發出 cancel', async () => {
    const wrapper = await mount()

    await wrapper.find('[data-testid="cancel"]').trigger('click')

    expect(wrapper.emitted('cancel')).toHaveLength(1)
  })

  it('點擊背景遮罩發出 cancel', async () => {
    const wrapper = await mount()

    await wrapper.find('[data-testid="modal-backdrop"]').trigger('click')

    expect(wrapper.emitted('cancel')).toHaveLength(1)
  })

  it('顯示題庫標題摘要', async () => {
    const wrapper = await mount({ quiz: buildQuiz({ title: 'Ocean Facts' }) })

    expect(wrapper.find('[data-testid="quiz-summary"]').text()).toContain('Ocean Facts')
  })
})
