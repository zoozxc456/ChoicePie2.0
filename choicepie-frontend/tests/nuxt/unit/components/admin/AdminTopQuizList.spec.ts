// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import AdminTopQuizList from '~/components/admin/AdminTopQuizList.vue'
import type { QuizSummaryDto } from '~/types/api'

const quiz = (id: string, title: string): QuizSummaryDto => ({
  id, title, creatorName: 'Alice', challengeCount: 10, coverImageUrl: null, coverEmoji: '📝', coverGradient: 'primary'
} as QuizSummaryDto)

describe('AdminTopQuizList', () => {
  it('依序列出熱門題庫與排序編號', async () => {
    const wrapper = await mountSuspended(AdminTopQuizList, { props: { quizzes: [quiz('a', 'First'), quiz('b', 'Second')] } })
    const items = wrapper.findAll('li')

    expect(items).toHaveLength(2)
    expect(items[0]!.find('[data-testid="top-quiz-position"]').text()).toBe('1')
    expect(items[1]!.text()).toContain('Second')
  })

  it('沒有資料時顯示空狀態文字', async () => {
    const wrapper = await mountSuspended(AdminTopQuizList, { props: { quizzes: [] } })

    expect(wrapper.find('li').exists()).toBe(false)
    expect(wrapper.find('[data-testid="top-quiz-empty"]').exists()).toBe(true)
  })
})
