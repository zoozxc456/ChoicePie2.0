export interface AdminQuizStatusStyle {
  icon: string
  borderClass: string
  bgClass: string
  textClass: string
}

const NEUTRAL: Omit<AdminQuizStatusStyle, 'icon'> = {
  borderClass: 'border-neutral-200',
  bgClass: 'bg-neutral-100',
  textClass: 'text-neutral-600'
}

const STATUS_STYLES: Record<string, AdminQuizStatusStyle> = {
  draft: { icon: 'i-lucide-file-edit', ...NEUTRAL },
  published: { icon: 'i-lucide-check-circle-2', borderClass: 'border-success-200', bgClass: 'bg-success-50', textClass: 'text-success-700' },
  archived: { icon: 'i-lucide-archive', borderClass: 'border-warning-200', bgClass: 'bg-warning-50', textClass: 'text-warning-700' },
  deleted: { icon: 'i-lucide-trash-2', ...NEUTRAL },
  takendown: { icon: 'i-lucide-shield-off', borderClass: 'border-error-200', bgClass: 'bg-error-50', textClass: 'text-error-700' }
}

/** 後台題庫列表右側狀態欄（AdminStatusStub）的圖示與配色 */
export const adminQuizStatusStyle = (status: string): AdminQuizStatusStyle =>
  STATUS_STYLES[status] ?? { icon: 'i-lucide-circle', ...NEUTRAL }
