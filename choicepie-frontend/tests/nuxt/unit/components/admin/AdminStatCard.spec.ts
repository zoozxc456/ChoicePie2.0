// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import AdminStatCard from '~/components/admin/AdminStatCard.vue'

describe('AdminStatCard', () => {
  it('顯示數值與標籤', async () => {
    const wrapper = await mountSuspended(AdminStatCard, { props: { value: 128, label: 'Members' } })

    expect(wrapper.find('[data-testid="stat-value"]').text()).toBe('128')
    expect(wrapper.find('[data-testid="stat-label"]').text()).toBe('Members')
  })
})
