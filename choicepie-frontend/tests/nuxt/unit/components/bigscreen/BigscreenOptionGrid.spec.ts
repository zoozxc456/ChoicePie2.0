// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import BigscreenOptionGrid from '~/components/bigscreen/BigscreenOptionGrid.vue'

const options = ['Mercury', 'Venus', 'Jupiter', 'Mars']

const items = async (correctAnswerIndex: number | null) => {
  const wrapper = await mountSuspended(BigscreenOptionGrid, { props: { options, correctAnswerIndex } })
  return wrapper.findAll('[data-testid="bigscreen-option"]')
}

describe('BigscreenOptionGrid', () => {
  it('依序以 A–D 標示選項', async () => {
    const options = await items(null)

    expect(options.map(o => o.find('[data-testid="option-letter"]').text())).toEqual(['A', 'B', 'C', 'D'])
    expect(options[2]!.text()).toContain('Jupiter')
  })

  it('揭曉前不顯示答對標記，各選項維持自己的顏色', async () => {
    const options = await items(null)

    expect(options.some(o => o.find('[data-testid="correct-mark"]').exists())).toBe(false)
    expect(options[0]!.classes()).toContain('border-cp-danger/40')
  })

  it('揭曉後正確選項標綠並打勾，其餘淡化', async () => {
    const options = await items(2)

    expect(options[2]!.classes()).toContain('border-cp-success')
    expect(options[2]!.find('[data-testid="correct-mark"]').exists()).toBe(true)
    expect(options[0]!.classes()).toContain('border-white/10')
    expect(options[0]!.find('[data-testid="correct-mark"]').exists()).toBe(false)
  })
})
