// @vitest-environment nuxt
import { describe, it, expect, afterEach, vi } from 'vitest'
import { mountSuspended } from '@nuxt/test-utils/runtime'
import StepTimeline from '~/components/howToUse/StepTimeline.vue'

const steps = [
  { title: 'Create', desc: 'Make a quiz' },
  { title: 'Share', desc: 'Share the code' },
  { title: 'Play', desc: 'Start the game' }
]

type ObserverCallback = (entries: Array<{ isIntersecting: boolean, target: Element }>) => void

const stubObserver = () => {
  const instance: { callback: ObserverCallback | null, observed: Element[] } = { callback: null, observed: [] }
  vi.stubGlobal('IntersectionObserver', class {
    constructor(callback: ObserverCallback) { instance.callback = callback }
    observe(el: Element) { instance.observed.push(el) }
    disconnect() { instance.observed = [] }
  })
  return instance
}

describe('StepTimeline', () => {
  afterEach(() => {
    vi.unstubAllGlobals()
  })

  it('依序編號每個步驟', async () => {
    stubObserver()
    const wrapper = await mountSuspended(StepTimeline, { props: { steps, firstBadge: 'Start here' } })

    expect(wrapper.findAll('[data-testid="step-number"]').map(n => n.text())).toEqual(['1', '2', '3'])
  })

  it('第一步放大呈現並附上徽章，其餘為精簡卡片', async () => {
    stubObserver()
    const wrapper = await mountSuspended(StepTimeline, { props: { steps, firstBadge: 'Start here' } })

    expect(wrapper.findAll('[data-testid="step-featured"]')).toHaveLength(1)
    expect(wrapper.find('[data-testid="step-featured"]').text()).toContain('Start here')
    expect(wrapper.find('[data-testid="step-featured"]').text()).toContain('Create')
    expect(wrapper.findAll('[data-testid="step-compact"]')).toHaveLength(2)
  })

  it('accent="secondary" 改用次要色節點', async () => {
    stubObserver()
    const wrapper = await mountSuspended(StepTimeline, { props: { steps, firstBadge: 'x', accent: 'secondary' } })

    expect(wrapper.find('[data-testid="step-number"]').classes()).toContain('bg-cp-secondary')
  })

  it('步驟進入畫面後才套用進場 class', async () => {
    const observer = stubObserver()
    const wrapper = await mountSuspended(StepTimeline, { props: { steps, firstBadge: 'x' } })
    await nextTick()
    const items = wrapper.findAll('[data-testid="step"]')
    expect(items[1]!.classes()).not.toContain('step-reveal--visible')

    observer.callback!([{ isIntersecting: true, target: items[1]!.element }])
    await nextTick()

    expect(items[1]!.classes()).toContain('step-reveal--visible')
    expect(items[0]!.classes()).not.toContain('step-reveal--visible')
  })

  it('瀏覽器不支援 IntersectionObserver 時直接全部顯示', async () => {
    vi.stubGlobal('IntersectionObserver', undefined)
    const wrapper = await mountSuspended(StepTimeline, { props: { steps, firstBadge: 'x' } })
    await nextTick()

    expect(wrapper.findAll('[data-testid="step"]').every(s => s.classes().includes('step-reveal--visible'))).toBe(true)
  })
})
