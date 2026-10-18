import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { effectScope, nextTick } from 'vue'
import { useDebouncedSearch } from '~/composables/useDebouncedSearch'

describe('useDebouncedSearch', () => {
  beforeEach(() => {
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  const setup = (delay?: number) => {
    const onSearch = vi.fn()
    const scope = effectScope()
    const { search } = scope.run(() => useDebouncedSearch(onSearch, delay))!
    return { onSearch, search, scope }
  }

  it('停止輸入超過延遲時間後才觸發', async () => {
    const { onSearch, search } = setup()

    search.value = 'abc'
    await nextTick()
    vi.advanceTimersByTime(299)
    expect(onSearch).not.toHaveBeenCalled()

    vi.advanceTimersByTime(1)
    expect(onSearch).toHaveBeenCalledWith('abc')
  })

  it('連續輸入只觸發最後一次', async () => {
    const { onSearch, search } = setup()

    search.value = 'a'
    await nextTick()
    vi.advanceTimersByTime(200)
    search.value = 'ab'
    await nextTick()
    vi.advanceTimersByTime(300)

    expect(onSearch).toHaveBeenCalledTimes(1)
    expect(onSearch).toHaveBeenCalledWith('ab')
  })

  it('清空搜尋時以 undefined 觸發', async () => {
    const { onSearch, search } = setup()

    search.value = 'a'
    await nextTick()
    vi.advanceTimersByTime(300)
    search.value = ''
    await nextTick()
    vi.advanceTimersByTime(300)

    expect(onSearch).toHaveBeenLastCalledWith(undefined)
  })

  it('可自訂延遲時間', async () => {
    const { onSearch, search } = setup(1000)

    search.value = 'x'
    await nextTick()
    vi.advanceTimersByTime(999)
    expect(onSearch).not.toHaveBeenCalled()
    vi.advanceTimersByTime(1)
    expect(onSearch).toHaveBeenCalled()
  })

  it('元件卸載後不再觸發', async () => {
    const { onSearch, search, scope } = setup()

    search.value = 'x'
    await nextTick()
    scope.stop()
    vi.advanceTimersByTime(300)

    expect(onSearch).not.toHaveBeenCalled()
  })
})
