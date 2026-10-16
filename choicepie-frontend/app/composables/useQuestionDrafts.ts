import { computed, ref } from 'vue'
import type { Question } from '~/types/quiz'

export const createBlankQuestion = (): Question => ({
  id: `draft-q-${Date.now()}-${Math.random().toString(36).slice(2, 8)}`,
  text: '',
  options: ['', '', '', ''],
  answerIndex: 0,
  explanation: ''
})

/** 題目與所有選項都已填寫 */
export const isQuestionComplete = (question: Question) =>
  !!question.text.trim() && question.options.every(option => option.trim())

const cloneQuestion = (question: Question): Question => ({ ...question, options: [...question.options] })

/**
 * 出題頁（手動 / AI）共用的本地題目草稿：新增、刪除、逐欄更新與展開狀態。
 * 只管理前端狀態，不呼叫 API。
 */
export const useQuestionDrafts = (initial: Question[] = []) => {
  const questions = ref<Question[]>(initial.map(cloneQuestion))
  const editingIndex = ref<number | null>(null)

  const patch = (index: number, changes: Partial<Question>) => {
    const current = questions.value[index]
    if (!current) return
    questions.value[index] = { ...current, ...changes }
  }

  const replaceAll = (next: Question[]) => {
    questions.value = next.map(cloneQuestion)
    editingIndex.value = null
  }

  const addQuestion = () => {
    questions.value.push(createBlankQuestion())
    editingIndex.value = questions.value.length - 1
  }

  const removeQuestion = (index: number) => {
    questions.value.splice(index, 1)
    if (editingIndex.value === null) return
    if (editingIndex.value === index) editingIndex.value = null
    else if (editingIndex.value > index) editingIndex.value--
  }

  const toggleEditing = (index: number) => {
    editingIndex.value = editingIndex.value === index ? null : index
  }

  const updateText = (index: number, text: string) => patch(index, { text })
  const updateExplanation = (index: number, explanation: string) => patch(index, { explanation })
  const setCorrectAnswer = (index: number, answerIndex: number) => patch(index, { answerIndex })

  const updateOption = (index: number, optionIndex: number, value: string) => {
    const options = [...(questions.value[index]?.options ?? [])]
    options[optionIndex] = value
    patch(index, { options })
  }

  const validQuestions = computed(() => questions.value.filter(isQuestionComplete))

  return {
    questions,
    editingIndex,
    validQuestions,
    replaceAll,
    addQuestion,
    removeQuestion,
    toggleEditing,
    updateText,
    updateExplanation,
    updateOption,
    setCorrectAnswer
  }
}
