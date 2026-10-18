// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import AiQuotaLocked from '~/components/library/editor/AiQuotaLocked.vue'

describe('AiQuotaLocked', () => {
  it('引導改用手動出題', async () => {
    const wrapper = await mountSuspended(AiQuotaLocked)

    expect(wrapper.find('a').attributes('href')).toBe('/library/new/manual')
  })
})
