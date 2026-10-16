// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import AdminNavCard from '~/components/admin/AdminNavCard.vue'

const props = { to: '/admin/quizzes', icon: 'i-lucide-book-open', label: 'Quizzes', count: 12 }

describe('AdminNavCard', () => {
  it('連到對應的管理頁並顯示標籤', async () => {
    const wrapper = await mountSuspended(AdminNavCard, { props })

    expect(wrapper.find('a').attributes('href')).toBe('/admin/quizzes')
    expect(wrapper.text()).toContain('Quizzes')
  })

  it('顯示數量徽章', async () => {
    const wrapper = await mountSuspended(AdminNavCard, { props })

    expect(wrapper.find('[data-testid="nav-count"]').text()).toBe('12')
  })

  it('count 為 null 時不顯示徽章', async () => {
    const wrapper = await mountSuspended(AdminNavCard, { props: { ...props, count: null } })

    expect(wrapper.find('[data-testid="nav-count"]').exists()).toBe(false)
  })

  it('highlight 時徽章為警示色', async () => {
    const normal = await mountSuspended(AdminNavCard, { props })
    const highlighted = await mountSuspended(AdminNavCard, { props: { ...props, highlight: true } })

    expect(normal.find('[data-testid="nav-count"]').classes()).toContain('bg-neutral-100')
    expect(highlighted.find('[data-testid="nav-count"]').classes()).toContain('bg-warning-100')
  })
})
