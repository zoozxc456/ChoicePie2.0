// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import ReadySummaryCard from '~/components/library/editor/ReadySummaryCard.vue'

describe('ReadySummaryCard', () => {
  it('顯示標題、副標與題數文字', async () => {
    const wrapper = await mountSuspended(ReadySummaryCard, {
      props: { title: 'Ready', subtitle: 'Check it', countLabel: '5 questions' }
    })

    expect(wrapper.find('[data-testid="ready-title"]').text()).toBe('Ready')
    expect(wrapper.find('[data-testid="ready-subtitle"]').text()).toBe('Check it')
    expect(wrapper.find('[data-testid="ready-count"]').text()).toBe('5 questions')
  })
})
