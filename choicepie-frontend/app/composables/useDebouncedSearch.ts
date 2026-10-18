import { onScopeDispose, ref, watch } from 'vue'

/**
 * 搜尋框輸入停止 delay 毫秒後才觸發 onSearch；清空時傳 undefined（代表不篩選）。
 */
export const useDebouncedSearch = (onSearch: (term: string | undefined) => void, delay = 300) => {
  const search = ref('')
  let timer: ReturnType<typeof setTimeout> | null = null

  const clear = () => {
    if (timer) clearTimeout(timer)
    timer = null
  }

  watch(search, (term) => {
    clear()
    timer = setTimeout(() => onSearch(term || undefined), delay)
  })

  onScopeDispose(clear)

  return { search }
}
