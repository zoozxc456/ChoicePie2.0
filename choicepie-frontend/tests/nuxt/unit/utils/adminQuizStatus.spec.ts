import { describe, it, expect } from 'vitest'
import { adminQuizStatusStyle } from '~/utils/adminQuizStatus'

describe('adminQuizStatusStyle', () => {
  it.each([
    ['draft', 'i-lucide-file-edit', 'border-neutral-200', 'bg-neutral-100', 'text-neutral-600'],
    ['published', 'i-lucide-check-circle-2', 'border-success-200', 'bg-success-50', 'text-success-700'],
    ['archived', 'i-lucide-archive', 'border-warning-200', 'bg-warning-50', 'text-warning-700'],
    ['deleted', 'i-lucide-trash-2', 'border-neutral-200', 'bg-neutral-100', 'text-neutral-600'],
    ['takendown', 'i-lucide-shield-off', 'border-error-200', 'bg-error-50', 'text-error-700']
  ])('%s', (status, icon, borderClass, bgClass, textClass) => {
    expect(adminQuizStatusStyle(status)).toEqual({ icon, borderClass, bgClass, textClass })
  })

  it('未知狀態使用中性樣式', () => {
    expect(adminQuizStatusStyle('unknown')).toEqual({
      icon: 'i-lucide-circle',
      borderClass: 'border-neutral-200',
      bgClass: 'bg-neutral-100',
      textClass: 'text-neutral-600'
    })
  })
})
