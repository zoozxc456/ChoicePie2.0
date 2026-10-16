// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import AdminQuizReportCard from '~/components/admin/AdminQuizReportCard.vue'
import type { QuizReportDto } from '~/types/api'

const report = (overrides: Partial<QuizReportDto> = {}): QuizReportDto => ({
  id: 'r1',
  quizId: 'q1',
  quizTitle: 'Space Trivia',
  reporterId: 'u1',
  reporterName: 'Bob',
  reason: 'spam',
  description: 'Looks like spam',
  status: 'Pending',
  resolvedBy: null,
  resolvedAt: null,
  resolutionNote: null,
  createdAt: '2026-01-01T00:00:00Z',
  ...overrides
})

describe('AdminQuizReportCard', () => {
  it('顯示題庫標題、檢舉人與描述', async () => {
    const wrapper = await mountSuspended(AdminQuizReportCard, { props: { report: report() } })

    expect(wrapper.text()).toContain('Space Trivia')
    expect(wrapper.text()).toContain('Bob')
    expect(wrapper.find('[data-testid="report-description"]').text()).toBe('Looks like spam')
  })

  it('沒有描述時不渲染描述段落', async () => {
    const wrapper = await mountSuspended(AdminQuizReportCard, { props: { report: report({ description: null }) } })

    expect(wrapper.find('[data-testid="report-description"]').exists()).toBe(false)
  })

  it('待處理時可駁回或處理，並發出對應事件', async () => {
    const wrapper = await mountSuspended(AdminQuizReportCard, { props: { report: report() } })

    await wrapper.find('[data-testid="report-dismiss"]').trigger('click')
    await wrapper.find('[data-testid="report-resolve"]').trigger('click')

    expect(wrapper.emitted('dismiss')).toHaveLength(1)
    expect(wrapper.emitted('resolve')).toHaveLength(1)
  })

  it('已處理的檢舉不顯示操作按鈕', async () => {
    const wrapper = await mountSuspended(AdminQuizReportCard, { props: { report: report({ status: 'Resolved' }) } })

    expect(wrapper.find('[data-testid="report-resolve"]').exists()).toBe(false)
    expect(wrapper.find('[data-testid="report-dismiss"]').exists()).toBe(false)
  })

  it('狀態徽章依狀態上色', async () => {
    const wrapper = await mountSuspended(AdminQuizReportCard, { props: { report: report({ status: 'Resolved' }) } })

    expect(wrapper.find('[data-testid="report-status"]').classes()).toContain('bg-error-100')
  })
})
