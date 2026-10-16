// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import QuizQuestionList from '~/components/library/detail/QuizQuestionList.vue'
import { buildQuiz } from '../../../fixtures/quiz'

describe('QuizQuestionList', () => {
  it('依序列出題目並從 1 開始編號', async () => {
    const wrapper = await mountSuspended(QuizQuestionList, { props: { questions: buildQuiz().questions } })
    const items = wrapper.findAll('[data-testid="question-item"]')

    expect(items).toHaveLength(2)
    expect(items[0]!.find('[data-testid="question-number"]').text()).toBe('1')
    expect(items[1]!.text()).toContain('Largest planet?')
  })
})
