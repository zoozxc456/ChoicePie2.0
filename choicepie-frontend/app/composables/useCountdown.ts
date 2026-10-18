import { computed, onScopeDispose, ref } from 'vue'

/**
 * 每秒遞減的倒數計時；歸零後的下一個 tick 停止並呼叫 onExpire。
 * start() 會重設秒數（換題時呼叫），元件卸載時自動停止。
 */
export const useCountdown = (seconds: number, onExpire: () => void) => {
  const timeLeft = ref(seconds)
  let interval: ReturnType<typeof setInterval> | null = null

  const isUrgent = computed(() => timeLeft.value <= 5 && timeLeft.value > 0)
  const percent = computed(() => (timeLeft.value / seconds) * 100)

  const stop = () => {
    if (interval !== null) {
      clearInterval(interval)
      interval = null
    }
  }

  const start = () => {
    stop()
    timeLeft.value = seconds
    interval = setInterval(() => {
      if (timeLeft.value > 0) {
        timeLeft.value--
      } else {
        stop()
        onExpire()
      }
    }, 1000)
  }

  onScopeDispose(stop)

  return { timeLeft, isUrgent, percent, start, stop }
}
