// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import RelatedQuizList from '~/components/library/detail/RelatedQuizList.vue'
import { buildQuiz } from '../../../fixtures/quiz'

describe('RelatedQuizList', () => {
  it('每個相關題庫連到自己的詳情頁', async () => {
    const quizzes = [buildQuiz({ id: 'a', title: 'A' }), buildQuiz({ id: 'b', title: 'B' })]
    const wrapper = await mountSuspended(RelatedQuizList, { props: { quizzes } })

    expect(wrapper.findAll('[data-testid="related-quiz"]').map(link => link.attributes('href'))).toEqual(['/library/a', '/library/b'])
  })

  it('沒有相關題庫時整個區塊不渲染', async () => {
    const wrapper = await mountSuspended(RelatedQuizList, { props: { quizzes: [] } })

    expect(wrapper.find('section').exists()).toBe(false)
  })
})
