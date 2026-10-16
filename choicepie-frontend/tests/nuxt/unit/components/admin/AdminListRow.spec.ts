// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import AdminListRow from '~/components/admin/AdminListRow.vue'

const mount = () => mountSuspended(AdminListRow, {
  props: { to: '/admin/members/1', ariaLabel: 'Alice' },
  slots: {
    default: () => 'Main content',
    action: () => 'Suspend',
    stub: () => 'Active'
  }
})

describe('AdminListRow', () => {
  it('整列以覆蓋連結導向詳情頁', async () => {
    const wrapper = await mount()
    const link = wrapper.find('[data-testid="row-link"]')

    expect(link.attributes('href')).toBe('/admin/members/1')
    expect(link.attributes('aria-label')).toBe('Alice')
  })

  it('渲染主要內容、操作與狀態欄', async () => {
    const wrapper = await mount()

    expect(wrapper.find('[data-testid="row-content"]').text()).toContain('Main content')
    expect(wrapper.find('[data-testid="row-action"]').text()).toBe('Suspend')
    expect(wrapper.text()).toContain('Active')
  })

  it('操作區疊在覆蓋連結之上，點按鈕不會被連結攔截', async () => {
    const wrapper = await mount()
    const action = wrapper.find('[data-testid="row-action"]')

    expect(action.classes()).toEqual(expect.arrayContaining(['relative', 'z-20', 'pointer-events-auto']))
    expect(wrapper.find('[data-testid="row-link"]').classes()).toContain('z-10')
  })
})
