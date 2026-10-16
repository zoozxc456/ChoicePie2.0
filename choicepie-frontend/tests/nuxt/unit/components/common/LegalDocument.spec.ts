// @vitest-environment nuxt
import { describe, it, expect } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import LegalDocument from '~/components/common/LegalDocument.vue'

describe('LegalDocument', () => {
  it('依 sectionKeys 渲染每個段落', async () => {
    const wrapper = await mountSuspended(LegalDocument, {
      props: { namespace: 'privacy', sectionKeys: ['collection', 'usage', 'contact'], lastUpdated: '2026-07-07' }
    })

    expect(wrapper.findAll('section')).toHaveLength(3)
    expect(wrapper.findAll('section h2').every(h => h.text().length > 0)).toBe(true)
  })

  it('標題取自 namespace，更新日期顯示在副標', async () => {
    const wrapper = await mountSuspended(LegalDocument, {
      props: { namespace: 'terms', sectionKeys: ['acceptance'], lastUpdated: '2026-07-07' }
    })

    expect(wrapper.find('h1').text()).not.toBe('terms.title')
    expect(wrapper.find('[data-testid="last-updated"]').text()).toContain('2026-07-07')
  })
})
