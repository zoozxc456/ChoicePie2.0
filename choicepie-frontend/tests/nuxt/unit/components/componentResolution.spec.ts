import { describe, it, expect } from 'vitest'
import { readdirSync, readFileSync } from 'node:fs'
import { basename, dirname, join, relative, sep } from 'node:path'

/**
 * Nuxt 自動註冊子資料夾內的元件時會加上路徑前綴（components/common/AuthCardLayout.vue → CommonAuthCardLayout），
 * 樣板直接寫 <AuthCardLayout> 又沒有 import 時，Vue 解析不到元件、整塊不渲染，只在瀏覽器留下 hydration 警告。
 * 這裡靜態掃描所有 .vue 檔，確保用到的專案元件不是明確 import，就是用 Nuxt 自動註冊的名稱。
 */

const APP_DIR = join(process.cwd(), 'app')
const COMPONENTS_DIR = join(APP_DIR, 'components')

const walkVueFiles = (dir: string): string[] =>
  readdirSync(dir, { withFileTypes: true }).flatMap((entry) => {
    const full = join(dir, entry.name)
    if (entry.isDirectory()) return walkVueFiles(full)
    return entry.name.endsWith('.vue') ? [full] : []
  })

const splitByCase = (str: string) => str.split(/[-_]|(?=[A-Z])/).filter(Boolean)
const pascal = (parts: string[]) => parts.map(p => p.charAt(0).toUpperCase() + p.slice(1)).join('')

// 與 Nuxt components scanner 相同的命名規則：路徑前綴與檔名開頭重複的部分會被省略
const toAutoName = (file: string) => {
  const dir = relative(COMPONENTS_DIR, dirname(file))
  const prefixParts = dir ? dir.split(sep).flatMap(splitByCase) : []
  const fileNameParts = splitByCase(basename(file, '.vue'))
  const nameParts: string[] = []
  while (prefixParts.length && prefixParts[0]!.toLowerCase() !== fileNameParts[0]?.toLowerCase()) {
    nameParts.push(prefixParts.shift()!)
  }
  return pascal(nameParts) + pascal(fileNameParts)
}

const componentFiles = walkVueFiles(COMPONENTS_DIR)
const autoNames = new Set(componentFiles.map(toAutoName))
const componentBaseNames = new Set(componentFiles.map(f => basename(f, '.vue')))

const findUnresolvedTags = (file: string) => {
  const source = readFileSync(file, 'utf8')
  const template = source.match(/<template>[\s\S]*<\/template>/)?.[0] ?? ''
  const tags = new Set([...template.matchAll(/<([A-Z][A-Za-z0-9]+)/g)].map(m => m[1]!))
  return [...tags].filter(tag =>
    componentBaseNames.has(tag)
    && !autoNames.has(tag)
    && !new RegExp(`import\\s+${tag}\\b`).test(source))
}

describe('component resolution', () => {
  it('toAutoName 依 Nuxt 規則加上不重複的路徑前綴', () => {
    expect(toAutoName(join(COMPONENTS_DIR, 'common', 'AuthCardLayout.vue'))).toBe('CommonAuthCardLayout')
    expect(toAutoName(join(COMPONENTS_DIR, 'admin', 'AdminListRow.vue'))).toBe('AdminListRow')
    expect(toAutoName(join(COMPONENTS_DIR, 'gameRoom', 'host', 'HostGameEnded.vue'))).toBe('GameRoomHostGameEnded')
  })

  it('所有 .vue 檔使用的專案元件都能被解析（已 import 或使用自動註冊名稱）', () => {
    const unresolved = walkVueFiles(APP_DIR).flatMap(file =>
      findUnresolvedTags(file).map(tag => `${relative(process.cwd(), file)}: <${tag}>`))
    expect(unresolved).toEqual([])
  })
})
