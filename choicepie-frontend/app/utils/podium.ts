export type PodiumPlace = 1 | 2 | 3

export interface PodiumSlot<T extends { rank: number }> {
  place: PodiumPlace
  /** 版面位置（0 起算），供進場動畫錯開延遲使用 */
  order: number
  entry: T
}

// 頒獎台版面呈現順序為 銀 / 金 / 銅
const PODIUM_DISPLAY_ORDER: readonly PodiumPlace[] = [2, 1, 3]

const RANK_COLOR_CLASS: Record<PodiumPlace, string> = {
  1: 'bg-cp-primary',
  2: 'bg-cp-info',
  3: 'bg-cp-warning'
}

const RANK_TEXT_CLASS: Record<PodiumPlace, string> = {
  1: 'text-cp-primary',
  2: 'text-cp-info',
  3: 'text-cp-warning'
}

const isPodiumPlace = (rank: number): rank is PodiumPlace => rank === 1 || rank === 2 || rank === 3

/**
 * 依 entry.rank（後端給的名次）挑出前三名，不可用陣列索引：
 * 同分並列時陣列順序與名次會不一致，例如兩位並列第 1 名時不存在第 2 名。
 */
export const buildPodium = <T extends { rank: number }>(rankings: readonly T[]): PodiumSlot<T>[] =>
  PODIUM_DISPLAY_ORDER
    .map((place, order) => ({ place, order, entry: rankings.find(r => r.rank === place) }))
    .filter((slot): slot is PodiumSlot<T> => slot.entry !== undefined)

export const rankColorClass = (rank: number, fallback = 'bg-neutral-300') =>
  isPodiumPlace(rank) ? RANK_COLOR_CLASS[rank] : fallback

export const rankTextClass = (rank: number) =>
  isPodiumPlace(rank) ? RANK_TEXT_CLASS[rank] : ''

/** 對應 design-tokens.css 的 .rank-1/2/3 光暈 */
export const rankGlowClass = (rank: number) =>
  isPodiumPlace(rank) ? `rank-${rank}` : ''
