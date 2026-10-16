// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import FaqList from '~/components/common/FaqList.vue'

const items = [
  { q: 'Is it free?', a: 'Yes.' },
  { q: 'How many players?', a: 'Up to 50.' }
]

describe('FaqList', () => {
  it('每題以可展開的 details 呈現，問題在 summary', async () => {
    const wrapper = await mountSuspended(FaqList, { props: { items } })
    const details = wrapper.findAll('details')

    expect(details).toHaveLength(2)
    expect(details[0]!.find('summary').text()).toBe('Is it free?')
    expect(details[1]!.text()).toContain('Up to 50.')
  })
})
