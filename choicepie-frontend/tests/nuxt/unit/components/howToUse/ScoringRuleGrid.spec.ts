// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import ScoringRuleGrid from '~/components/howToUse/ScoringRuleGrid.vue'

const rules = [
  { time: '0–2s', points: 1000 },
  { time: '2–5s', points: 800 }
]

describe('ScoringRuleGrid', () => {
  it('每條規則顯示分數與時間', async () => {
    const wrapper = await mountSuspended(ScoringRuleGrid, { props: { rules, unitLabel: 'pts' } })
    const items = wrapper.findAll('[data-testid="scoring-rule"]')

    expect(items).toHaveLength(2)
    expect(items[0]!.text()).toContain((1000).toLocaleString())
    expect(items[0]!.text()).toContain('0–2s')
    expect(items[0]!.text()).toContain('pts')
  })

  it('只有第一條（最快）規則被強調', async () => {
    const wrapper = await mountSuspended(ScoringRuleGrid, { props: { rules, unitLabel: 'pts' } })
    const items = wrapper.findAll('[data-testid="scoring-rule"]')

    expect(items[0]!.classes()).toContain('border-primary-500/40')
    expect(items[1]!.classes()).not.toContain('border-primary-500/40')
  })
})
