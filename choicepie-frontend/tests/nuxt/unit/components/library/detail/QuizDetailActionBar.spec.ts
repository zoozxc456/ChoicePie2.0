// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import QuizDetailActionBar from '~/components/library/detail/QuizDetailActionBar.vue'
import { buildQuiz } from '../../../fixtures/quiz'

const mount = (props: Record<string, unknown> = {}) =>
  mountSuspended(QuizDetailActionBar, {
    props: { quiz: buildQuiz(), isOwner: false, isFavorited: false, ...props }
  })

const exists = (wrapper: Awaited<ReturnType<typeof mount>>, id: string) => wrapper.find(`[data-testid="${id}"]`).exists()

describe('QuizDetailActionBar', () => {
  it('任何人都看得到開始與收藏按鈕', async () => {
    const wrapper = await mount()

    expect(exists(wrapper, 'action-start')).toBe(true)
    expect(exists(wrapper, 'action-favorite')).toBe(true)
  })

  it('非擁有者：可檢舉，看不到個人練習、發佈與狀態徽章', async () => {
    const wrapper = await mount({ isOwner: false })

    expect(exists(wrapper, 'action-report')).toBe(true)
    expect(exists(wrapper, 'action-solo')).toBe(false)
    expect(exists(wrapper, 'action-publish')).toBe(false)
    expect(exists(wrapper, 'action-unpublish')).toBe(false)
    expect(exists(wrapper, 'badge-status')).toBe(false)
  })

  it('擁有者 + 已發佈：可取消發佈，不可檢舉', async () => {
    const wrapper = await mount({ isOwner: true, quiz: buildQuiz({ status: 'published' }) })

    expect(exists(wrapper, 'action-solo')).toBe(true)
    expect(exists(wrapper, 'action-unpublish')).toBe(true)
    expect(exists(wrapper, 'action-publish')).toBe(false)
    expect(exists(wrapper, 'action-report')).toBe(false)
    expect(exists(wrapper, 'badge-status')).toBe(true)
  })

  it('擁有者 + 草稿：可發佈', async () => {
    const wrapper = await mount({ isOwner: true, quiz: buildQuiz({ status: 'draft' }) })

    expect(exists(wrapper, 'action-publish')).toBe(true)
    expect(exists(wrapper, 'action-unpublish')).toBe(false)
  })

  it('已檢舉時檢舉按鈕停用', async () => {
    const wrapper = await mount({ hasReported: true })

    expect(wrapper.find('[data-testid="action-report"]').attributes('disabled')).toBeDefined()
  })

  it('已收藏時收藏按鈕標示為按下', async () => {
    const wrapper = await mount({ isFavorited: true })

    expect(wrapper.find('[data-testid="action-favorite"]').attributes('aria-pressed')).toBe('true')
  })

  it.each([
    ['action-start', 'start', {}],
    ['action-favorite', 'toggleFavorite', {}],
    ['action-report', 'report', {}],
    ['action-solo', 'soloPractice', { isOwner: true }],
    ['action-unpublish', 'unpublish', { isOwner: true }],
    ['action-publish', 'publish', { isOwner: true, quiz: buildQuiz({ status: 'draft' }) }]
  ] as const)('點擊 %s 發出 %s', async (testId, event, props) => {
    const wrapper = await mount(props)

    await wrapper.find(`[data-testid="${testId}"]`).trigger('click')

    expect(wrapper.emitted(event)).toHaveLength(1)
  })

  it('顯示通過率徽章', async () => {
    const wrapper = await mount({ quiz: buildQuiz({ passRate: 88 }) })

    expect(wrapper.find('[data-testid="badge-pass-rate"]').text()).toContain('88')
  })
})
