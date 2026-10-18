// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import QuizDetailHero from '~/components/library/detail/QuizDetailHero.vue'
import { DIFFICULTY_LABEL } from '~/types/quiz'
import { buildQuiz } from '../../../fixtures/quiz'

describe('QuizDetailHero', () => {
  it('顯示題庫標題', async () => {
    const wrapper = await mountSuspended(QuizDetailHero, { props: { quiz: buildQuiz() } })

    expect(wrapper.find('h1').text()).toBe('Space Trivia')
  })

  it('顯示難度與第一個標籤', async () => {
    const wrapper = await mountSuspended(QuizDetailHero, { props: { quiz: buildQuiz({ difficulty: 'expert', tags: ['history', 'europe'] }) } })

    expect(wrapper.find('[data-testid="hero-eyebrow"]').text()).toBe(`${DIFFICULTY_LABEL.expert} · history`)
  })

  it('沒有標籤時不顯示多餘的分隔符號', async () => {
    const wrapper = await mountSuspended(QuizDetailHero, { props: { quiz: buildQuiz({ tags: [] }) } })

    expect(wrapper.find('[data-testid="hero-eyebrow"]').text()).toBe(DIFFICULTY_LABEL.intermediate)
  })

  it('副標顯示出題者名稱', async () => {
    const wrapper = await mountSuspended(QuizDetailHero, { props: { quiz: buildQuiz({ creatorName: 'Zoe' }) } })

    expect(wrapper.find('[data-testid="hero-meta"]').text()).toContain('Zoe')
  })
})
