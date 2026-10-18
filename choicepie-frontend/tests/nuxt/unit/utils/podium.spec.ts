import { describe, it, expect } from 'vitest'
import { buildPodium, rankColorClass, rankGlowClass, rankTextClass } from '~/utils/podium'

const entry = (rank: number, nickname: string, score = 100) => ({ rank, nickname, score })

describe('buildPodium', () => {
  it('依 銀 / 金 / 銅 的版面順序排列', () => {
    const podium = buildPodium([entry(1, 'Gold'), entry(2, 'Silver'), entry(3, 'Bronze')])

    expect(podium.map(slot => slot.place)).toEqual([2, 1, 3])
    expect(podium.map(slot => slot.entry.nickname)).toEqual(['Silver', 'Gold', 'Bronze'])
  })

  it('依 entry.rank 取人，而非陣列索引', () => {
    const podium = buildPodium([entry(3, 'Bronze'), entry(1, 'Gold'), entry(2, 'Silver')])

    expect(podium.find(slot => slot.place === 1)?.entry.nickname).toBe('Gold')
    expect(podium.find(slot => slot.place === 2)?.entry.nickname).toBe('Silver')
    expect(podium.find(slot => slot.place === 3)?.entry.nickname).toBe('Bronze')
  })

  it('同分並列時不會把第二位並列者錯標成下一名', () => {
    const podium = buildPodium([entry(1, 'A', 500), entry(1, 'B', 500), entry(3, 'C', 100)])

    expect(podium.map(slot => [slot.place, slot.entry.rank])).toEqual([[1, 1], [3, 3]])
  })

  it('人數不足三人時略過缺少的名次', () => {
    const podium = buildPodium([entry(1, 'Solo')])

    expect(podium).toHaveLength(1)
    expect(podium[0]).toMatchObject({ place: 1, order: 1 })
  })

  it('order 為版面位置，供進場動畫錯開使用', () => {
    const podium = buildPodium([entry(1, 'Gold'), entry(2, 'Silver'), entry(3, 'Bronze')])

    expect(podium.map(slot => slot.order)).toEqual([0, 1, 2])
  })

  it('空排名回傳空陣列', () => {
    expect(buildPodium([])).toEqual([])
  })
})

describe('rank class helpers', () => {
  it('前三名對應固定的背景色，其餘使用 fallback', () => {
    expect(rankColorClass(1)).toBe('bg-cp-primary')
    expect(rankColorClass(2)).toBe('bg-cp-info')
    expect(rankColorClass(3)).toBe('bg-cp-warning')
    expect(rankColorClass(4)).toBe('bg-neutral-300')
    expect(rankColorClass(4, 'bg-neutral-400')).toBe('bg-neutral-400')
  })

  it('前三名對應文字色', () => {
    expect(rankTextClass(1)).toBe('text-cp-primary')
    expect(rankTextClass(2)).toBe('text-cp-info')
    expect(rankTextClass(3)).toBe('text-cp-warning')
    expect(rankTextClass(4)).toBe('')
  })

  it('前三名對應光暈 class，其餘為空字串', () => {
    expect(rankGlowClass(1)).toBe('rank-1')
    expect(rankGlowClass(2)).toBe('rank-2')
    expect(rankGlowClass(3)).toBe('rank-3')
    expect(rankGlowClass(4)).toBe('')
  })
})
