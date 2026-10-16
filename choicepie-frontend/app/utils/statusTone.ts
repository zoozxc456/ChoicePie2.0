import type { Difficulty } from '~/types/quiz'

export type StatusBadgeTone = 'neutral' | 'success' | 'warning' | 'error' | 'primary'

const QUIZ_STATUS_TONE: Record<string, StatusBadgeTone> = {
  published: 'success',
  draft: 'neutral',
  archived: 'warning',
  deleted: 'neutral',
  takendown: 'error'
}

const QUIZ_REPORT_STATUS_TONE: Record<string, StatusBadgeTone> = {
  Pending: 'warning',
  Resolved: 'error',
  Dismissed: 'neutral'
}

/** 題庫狀態（published / draft / archived / deleted / takendown）對應的徽章色調 */
export const quizStatusTone = (status: string): StatusBadgeTone => QUIZ_STATUS_TONE[status] ?? 'neutral'

/** 檢舉狀態（Pending / Resolved / Dismissed）對應的徽章色調 */
export const quizReportStatusTone = (status: string): StatusBadgeTone => QUIZ_REPORT_STATUS_TONE[status] ?? 'neutral'

const DIFFICULTY_TONE: Record<Difficulty, StatusBadgeTone> = {
  beginner: 'success',
  intermediate: 'warning',
  expert: 'error'
}

/** 題庫難度對應的徽章色調 */
export const difficultyTone = (difficulty: Difficulty): StatusBadgeTone => DIFFICULTY_TONE[difficulty]
