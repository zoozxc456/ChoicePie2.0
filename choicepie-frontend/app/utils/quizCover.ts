import type { QuizCoverInput } from '~/stores/quiz'

export const COVER_COLOR_OPTIONS = ['primary', 'secondary', 'success', 'danger', 'warning', 'info'] as const
export const COVER_EMOJI_OPTIONS = ['📝', '🎯', '🧠', '🎲', '🏆', '📚', '⚡', '🎨']

const COLOR_CLASS_MAP: Record<string, string> = {
  primary: 'bg-cp-primary',
  secondary: 'bg-cp-secondary',
  success: 'bg-cp-success',
  danger: 'bg-cp-danger',
  warning: 'bg-cp-warning',
  info: 'bg-cp-info'
}

/** 題庫封面底色的 class；未知顏色退回 secondary */
export const coverColorClass = (gradient: string) => COLOR_CLASS_MAP[gradient] ?? 'bg-cp-secondary'

/** 新題庫的預設封面（無圖片、📝、primary） */
export const createDefaultCover = (): QuizCoverInput => ({
  coverImageUrl: null,
  coverEmoji: COVER_EMOJI_OPTIONS[0]!,
  coverGradient: 'primary'
})
