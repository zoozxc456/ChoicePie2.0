import { describe, it, expect } from 'vitest'
import { useModalTarget } from '~/composables/useModalTarget'

describe('useModalTarget', () => {
  it('初始為關閉且沒有目標', () => {
    const modal = useModalTarget<string>()

    expect(modal.isOpen.value).toBe(false)
    expect(modal.target.value).toBeNull()
  })

  it('open 記住目標並開啟', () => {
    const modal = useModalTarget<{ id: string, action: 'resolve' | 'dismiss' }>()

    modal.open({ id: 'r1', action: 'dismiss' })

    expect(modal.isOpen.value).toBe(true)
    expect(modal.target.value).toEqual({ id: 'r1', action: 'dismiss' })
  })

  it('close 關閉並清除目標', () => {
    const modal = useModalTarget<string>()
    modal.open('m1')

    modal.close()

    expect(modal.isOpen.value).toBe(false)
    expect(modal.target.value).toBeNull()
  })

  it('不帶目標開啟（例如「新增」）時目標為 null', () => {
    const modal = useModalTarget<string>()
    modal.open('m1')
    modal.close()

    modal.open(null)

    expect(modal.isOpen.value).toBe(true)
    expect(modal.target.value).toBeNull()
  })
})
