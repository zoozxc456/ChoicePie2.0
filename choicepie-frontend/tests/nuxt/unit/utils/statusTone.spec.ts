import { describe, it, expect } from 'vitest'
import { quizStatusTone, quizReportStatusTone, difficultyTone } from '~/utils/statusTone'

describe('quizStatusTone', () => {
  it.each([
    ['published', 'success'],
    ['draft', 'neutral'],
    ['archived', 'warning'],
    ['deleted', 'neutral'],
    ['takendown', 'error']
  ] as const)('%s → %s', (status, tone) => {
    expect(quizStatusTone(status)).toBe(tone)
  })

  it('未知狀態回傳 neutral', () => {
    expect(quizStatusTone('whatever')).toBe('neutral')
  })
})

describe('quizReportStatusTone', () => {
  it.each([
    ['Pending', 'warning'],
    ['Resolved', 'error'],
    ['Dismissed', 'neutral']
  ] as const)('%s → %s', (status, tone) => {
    expect(quizReportStatusTone(status)).toBe(tone)
  })

  it('未知狀態回傳 neutral', () => {
    expect(quizReportStatusTone('Unknown')).toBe('neutral')
  })
})

describe('difficultyTone', () => {
  it.each([
    ['beginner', 'success'],
    ['intermediate', 'warning'],
    ['expert', 'error']
  ] as const)('%s → %s', (difficulty, tone) => {
    expect(difficultyTone(difficulty)).toBe(tone)
  })
})
