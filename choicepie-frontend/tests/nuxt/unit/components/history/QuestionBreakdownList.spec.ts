// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import QuestionBreakdownList from '~/components/history/QuestionBreakdownList.vue'

const answered = {
  questionText: 'Largest planet?',
  options: [
    { text: 'Jupiter', pickedCount: 2, isCorrect: true },
    { text: 'Mars', pickedCount: 1, isCorrect: false }
  ],
  correctCount: 2,
  answeredCount: 3,
  averageAnswerTimeMs: 1500
}

const unanswered = {
  questionText: 'Smallest planet?',
  options: [{ text: 'Mercury', pickedCount: 0, isCorrect: true }],
  correctCount: 0,
  answeredCount: 0,
  averageAnswerTimeMs: null
}

describe('QuestionBreakdownList', () => {
  it('每題顯示答對率（四捨五入）', async () => {
    const wrapper = await mountSuspended(QuestionBreakdownList, { props: { questions: [answered] } })

    expect(wrapper.find('[data-testid="correct-rate"]').text()).toContain('67')
  })

  it('每個選項顯示選擇人數與比例', async () => {
    const wrapper = await mountSuspended(QuestionBreakdownList, { props: { questions: [answered] } })
    const stats = wrapper.findAll('[data-testid="option-stat"]').map(s => s.text())

    expect(stats).toEqual(['2 · 67%', '1 · 33%'])
  })

  it('正確選項以成功色標示', async () => {
    const wrapper = await mountSuspended(QuestionBreakdownList, { props: { questions: [answered] } })
    const options = wrapper.findAll('[data-testid="breakdown-option"]')

    expect(options[0]!.classes()).toContain('border-cp-success')
    expect(options[1]!.classes()).not.toContain('border-cp-success')
  })

  it('顯示平均作答秒數（一位小數）', async () => {
    const wrapper = await mountSuspended(QuestionBreakdownList, { props: { questions: [answered] } })

    expect(wrapper.find('[data-testid="average-time"]').text()).toContain('1.5')
  })

  it('無人作答時不列選項、不顯示平均秒數，答對率為 0', async () => {
    const wrapper = await mountSuspended(QuestionBreakdownList, { props: { questions: [unanswered] } })

    expect(wrapper.find('[data-testid="no-answers"]').exists()).toBe(true)
    expect(wrapper.findAll('[data-testid="breakdown-option"]')).toHaveLength(0)
    expect(wrapper.find('[data-testid="average-time"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="correct-rate"]').text()).toContain('0')
  })

  it('沒有題目時整個區塊不渲染', async () => {
    const wrapper = await mountSuspended(QuestionBreakdownList, { props: { questions: [] } })

    expect(wrapper.find('section').exists()).toBe(false)
  })
})
