// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import JoinSteps from '~/components/join/JoinSteps.vue'

describe('JoinSteps', () => {
  it('依序編號列出加入步驟', async () => {
    const wrapper = await mountSuspended(JoinSteps)
    const items = wrapper.findAll('li')

    expect(items).toHaveLength(3)
    expect(items.map(item => item.find('[data-testid="join-step-number"]').text())).toEqual(['1', '2', '3'])
  })
})
