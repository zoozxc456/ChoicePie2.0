// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import AdminMembershipTierRow from '~/components/admin/AdminMembershipTierRow.vue'
import type { MembershipTierDto } from '~/types/api'

const tier = (overrides: Partial<MembershipTierDto> = {}): MembershipTierDto => ({
  id: 't1', name: 'Pro', dailyGenerationLimit: 20, dailyTokenBudget: 50000, isDefault: false, createdAt: '2026-01-01T00:00:00Z', ...overrides
})

describe('AdminMembershipTierRow', () => {
  it('顯示等級名稱與額度', async () => {
    const wrapper = await mountSuspended(AdminMembershipTierRow, { props: { tier: tier() } })

    expect(wrapper.text()).toContain('Pro')
    expect(wrapper.find('[data-testid="tier-limits"]').text()).toContain('20')
  })

  it('預設等級顯示預設徽章', async () => {
    const normal = await mountSuspended(AdminMembershipTierRow, { props: { tier: tier() } })
    const defaultTier = await mountSuspended(AdminMembershipTierRow, { props: { tier: tier({ isDefault: true }) } })

    expect(normal.find('[data-testid="tier-default"]').exists()).toBe(false)
    expect(defaultTier.find('[data-testid="tier-default"]').exists()).toBe(true)
  })

  it('點擊編輯發出 edit', async () => {
    const wrapper = await mountSuspended(AdminMembershipTierRow, { props: { tier: tier() } })

    await wrapper.find('[data-testid="tier-edit"]').trigger('click')

    expect(wrapper.emitted('edit')).toHaveLength(1)
  })
})
