import { ref, type Ref } from 'vue'

/** 「對某個項目開啟確認 modal」的開關狀態；target 為 null 代表不針對既有項目（例如新增） */
export const useModalTarget = <T>() => {
  const isOpen = ref(false)
  const target = ref<T | null>(null) as Ref<T | null>

  const open = (value: T | null) => {
    target.value = value
    isOpen.value = true
  }

  const close = () => {
    isOpen.value = false
    target.value = null
  }

  return { isOpen, target, open, close }
}
