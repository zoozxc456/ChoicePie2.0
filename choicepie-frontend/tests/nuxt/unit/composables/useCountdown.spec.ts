import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { effectScope } from 'vue'
import { useCountdown } from '~/composables/useCountdown'

describe('useCountdown', () => {
  beforeEach(() => {
    vi.useFakeTimers()
  })

  afterEach(() => {
    vi.useRealTimers()
  })

  const setup = (seconds = 10) => {
    const onExpire = vi.fn()
    const scope = effectScope()
    const countdown = scope.run(() => useCountdown(seconds, onExpire))!
    return { onExpire, scope, countdown }
  }

  it('start 前停在初始秒數且不倒數', () => {
    const { countdown } = setup()

    vi.advanceTimersByTime(3000)

    expect(countdown.timeLeft.value).toBe(10)
    expect(countdown.percent.value).toBe(100)
  })

  it('start 後每秒減 1，percent 跟著下降', () => {
    const { countdown } = setup()

    countdown.start()
    vi.advanceTimersByTime(3000)

    expect(countdown.timeLeft.value).toBe(7)
    expect(countdown.percent.value).toBe(70)
  })

  it('剩 5 秒以內（不含 0）為 urgent', () => {
    const { countdown } = setup()
    countdown.start()

    vi.advanceTimersByTime(4000)
    expect(countdown.isUrgent.value).toBe(false)

    vi.advanceTimersByTime(1000)
    expect(countdown.timeLeft.value).toBe(5)
    expect(countdown.isUrgent.value).toBe(true)

    vi.advanceTimersByTime(5000)
    expect(countdown.timeLeft.value).toBe(0)
    expect(countdown.isUrgent.value).toBe(false)
  })

  it('歸零後的下一個 tick 呼叫 onExpire 一次並停止', () => {
    const { countdown, onExpire } = setup(3)
    countdown.start()

    vi.advanceTimersByTime(3000)
    expect(onExpire).not.toHaveBeenCalled()

    vi.advanceTimersByTime(1000)
    expect(onExpire).toHaveBeenCalledOnce()

    vi.advanceTimersByTime(5000)
    expect(onExpire).toHaveBeenCalledOnce()
  })

  it('再次 start 會重設秒數，不會疊加多個計時器', () => {
    const { countdown } = setup()
    countdown.start()
    vi.advanceTimersByTime(4000)

    countdown.start()
    expect(countdown.timeLeft.value).toBe(10)

    vi.advanceTimersByTime(1000)
    expect(countdown.timeLeft.value).toBe(9)
  })

  it('stop 後不再倒數也不觸發 onExpire', () => {
    const { countdown, onExpire } = setup(2)
    countdown.start()
    vi.advanceTimersByTime(1000)

    countdown.stop()
    vi.advanceTimersByTime(5000)

    expect(countdown.timeLeft.value).toBe(1)
    expect(onExpire).not.toHaveBeenCalled()
  })

  it('元件卸載時自動停止', () => {
    const { countdown, onExpire, scope } = setup(2)
    countdown.start()

    scope.stop()
    vi.advanceTimersByTime(5000)

    expect(countdown.timeLeft.value).toBe(2)
    expect(onExpire).not.toHaveBeenCalled()
  })
})
