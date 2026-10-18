export const ALLOWED_IMAGE_TYPES = ['image/jpeg', 'image/png', 'image/webp']
export const MAX_IMAGE_BYTES = 5 * 1024 * 1024

export type ImageFileError = 'invalidType' | 'tooLarge'

/** 上傳圖片（頭像、題庫封面）的前端檢查；通過時回傳 null */
export const validateImageFile = (file: Pick<File, 'type' | 'size'>): ImageFileError | null => {
  if (!ALLOWED_IMAGE_TYPES.includes(file.type)) return 'invalidType'
  if (file.size > MAX_IMAGE_BYTES) return 'tooLarge'
  return null
}
