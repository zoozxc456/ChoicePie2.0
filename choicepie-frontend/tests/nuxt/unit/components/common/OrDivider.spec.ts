// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import OrDivider from '~/components/common/OrDivider.vue'

describe('OrDivider', () => {
  it('文字兩側各有一條分隔線', async () => {
    const wrapper = await mountSuspended(OrDivider, { props: { label: 'or' } })

    expect(wrapper.text()).toBe('or')
    expect(wrapper.findAll('hr')).toHaveLength(2)
  })
})
