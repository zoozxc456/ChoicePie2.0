// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import JoinPreviewMock from '~/components/join/JoinPreviewMock.vue'

describe('JoinPreviewMock', () => {
  it('呈現三個示意選項，其中一個標示為正確', async () => {
    const wrapper = await mountSuspended(JoinPreviewMock)

    expect(wrapper.findAll('.option-card')).toHaveLength(3)
    expect(wrapper.findAll('.option-card.correct')).toHaveLength(1)
  })

  it('呈現示意玩家', async () => {
    const wrapper = await mountSuspended(JoinPreviewMock)

    expect(wrapper.findAll('.player-chip').length).toBeGreaterThan(0)
  })
})
