// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import WrongAnswerReview from '~/components/history/WrongAnswerReview.vue'

const wrongAnswers = [{
  questionText: 'Largest planet?',
  options: ['Mercury', 'Venus', 'Jupiter', 'Mars'],
  myAnswerIndex: 1,
  correctAnswerIndex: 2,
  explanation: 'Jupiter is the biggest.'
}]

describe('WrongAnswerReview', () => {
  it('每題列出題目與解析', async () => {
    const wrapper = await mountSuspended(WrongAnswerReview, { props: { wrongAnswers } })

    const item = wrapper.find('[data-testid="review-item"]')
    expect(item.text()).toContain('1. Largest planet?')
    expect(item.find('[data-testid="review-explanation"]').text()).toContain('Jupiter is the biggest.')
  })

  it('正確答案標綠、我的答案標紅、其餘為中性', async () => {
    const wrapper = await mountSuspended(WrongAnswerReview, { props: { wrongAnswers } })
    const options = wrapper.findAll('[data-testid="review-option"]')

    expect(options[2]!.classes()).toContain('text-cp-success')
    expect(options[1]!.classes()).toContain('text-cp-danger')
    expect(options[0]!.classes()).toContain('text-cp-text-secondary')
    expect(options[3]!.classes()).toContain('text-cp-text-secondary')
  })

  it('正確答案與我的答案附上標註', async () => {
    const wrapper = await mountSuspended(WrongAnswerReview, { props: { wrongAnswers } })
    const options = wrapper.findAll('[data-testid="review-option"]')

    expect(options[1]!.find('[data-testid="my-answer-tag"]').exists()).toBe(true)
    expect(options[2]!.find('[data-testid="correct-answer-tag"]').exists()).toBe(true)
    expect(options[0]!.find('[data-testid="my-answer-tag"]').exists()).toBe(false)
  })

  it('全對時不列題目', async () => {
    const wrapper = await mountSuspended(WrongAnswerReview, { props: { wrongAnswers: [] } })

    expect(wrapper.findAll('[data-testid="review-item"]')).toHaveLength(0)
    expect(wrapper.find('[data-testid="review-subtitle"]').exists()).toBe(true)
  })
})
