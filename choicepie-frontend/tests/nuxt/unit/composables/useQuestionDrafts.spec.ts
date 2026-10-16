import { describe, it, expect } from 'vitest'
import { createBlankQuestion, isQuestionComplete, useQuestionDrafts } from '~/composables/useQuestionDrafts'
import type { Question } from '~/types/quiz'

const filled = (id: string, text = 'Q'): Question => ({ id, text, options: ['a', 'b', 'c', 'd'], answerIndex: 0, explanation: '' })

describe('createBlankQuestion', () => {
  it('四個空選項、預設第一個為正解', () => {
    const q = createBlankQuestion()

    expect(q).toMatchObject({ text: '', options: ['', '', '', ''], answerIndex: 0, explanation: '' })
  })

  it('每次產生不同的 id', () => {
    expect(createBlankQuestion().id).not.toBe(createBlankQuestion().id)
  })
})

describe('isQuestionComplete', () => {
  it('題目與所有選項都有內容才算完整', () => {
    expect(isQuestionComplete(filled('1'))).toBe(true)
    expect(isQuestionComplete({ ...filled('1'), text: '  ' })).toBe(false)
    expect(isQuestionComplete({ ...filled('1'), options: ['a', '', 'c', 'd'] })).toBe(false)
  })
})

describe('useQuestionDrafts', () => {
  it('以初始題目建立草稿，且不會改到傳入的物件', () => {
    const initial = [filled('1')]
    const drafts = useQuestionDrafts(initial)

    drafts.updateOption(0, 1, 'changed')

    expect(drafts.questions.value[0]!.options[1]).toBe('changed')
    expect(initial[0]!.options[1]).toBe('b')
  })

  it('addQuestion 新增空白題並展開它', () => {
    const drafts = useQuestionDrafts([filled('1')])

    drafts.addQuestion()

    expect(drafts.questions.value).toHaveLength(2)
    expect(drafts.questions.value[1]!.text).toBe('')
    expect(drafts.editingIndex.value).toBe(1)
  })

  it('toggleEditing 切換展開，再點一次收合', () => {
    const drafts = useQuestionDrafts([filled('1'), filled('2')])

    drafts.toggleEditing(1)
    expect(drafts.editingIndex.value).toBe(1)

    drafts.toggleEditing(1)
    expect(drafts.editingIndex.value).toBeNull()
  })

  it('刪除正在編輯的題目時收合', () => {
    const drafts = useQuestionDrafts([filled('1'), filled('2')])
    drafts.toggleEditing(1)

    drafts.removeQuestion(1)

    expect(drafts.questions.value.map(q => q.id)).toEqual(['1'])
    expect(drafts.editingIndex.value).toBeNull()
  })

  it('刪除前面的題目時，展開中的題目索引跟著前移', () => {
    const drafts = useQuestionDrafts([filled('1'), filled('2'), filled('3')])
    drafts.toggleEditing(2)

    drafts.removeQuestion(0)

    expect(drafts.editingIndex.value).toBe(1)
    expect(drafts.questions.value[drafts.editingIndex.value!]!.id).toBe('3')
  })

  it('updateText / updateExplanation / setCorrectAnswer 更新對應欄位', () => {
    const drafts = useQuestionDrafts([filled('1')])

    drafts.updateText(0, 'New text')
    drafts.updateExplanation(0, 'Because')
    drafts.setCorrectAnswer(0, 3)

    expect(drafts.questions.value[0]).toMatchObject({ text: 'New text', explanation: 'Because', answerIndex: 3 })
  })

  it('validQuestions 只包含完整的題目', () => {
    const drafts = useQuestionDrafts([filled('1'), createBlankQuestion()])

    expect(drafts.validQuestions.value.map(q => q.id)).toEqual(['1'])
  })

  it('replaceAll 換掉所有題目並收合', () => {
    const drafts = useQuestionDrafts([filled('1')])
    drafts.toggleEditing(0)

    drafts.replaceAll([filled('a'), filled('b')])

    expect(drafts.questions.value.map(q => q.id)).toEqual(['a', 'b'])
    expect(drafts.editingIndex.value).toBeNull()
  })
})
