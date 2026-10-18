/** 標籤輸入框（逗號分隔）→ 標籤陣列；去除空白並略過空項目 */
export const parseTags = (input: string) =>
  input.split(',').map(tag => tag.trim()).filter(Boolean)

/** 標籤陣列 → 標籤輸入框字串 */
export const formatTags = (tags: string[]) => tags.join(', ')
