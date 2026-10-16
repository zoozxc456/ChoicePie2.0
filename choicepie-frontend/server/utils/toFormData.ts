import type { H3Event } from 'h3'
import { readMultipartFormData } from 'h3'

// 把 h3 解析出來的 multipart parts 重組成瀏覽器原生 FormData，讓 ofetch 在轉發給後端時
// 能自動偵測並設定正確的 multipart boundary（h3 的 readMultipartFormData 本身不回傳可直接
// 重新送出的 FormData 物件）。
export const toFormData = async (event: H3Event): Promise<FormData> => {
  const parts = await readMultipartFormData(event) ?? []
  const formData = new FormData()

  for (const part of parts) {
    if (part.filename) {
      formData.append(part.name ?? 'file', new Blob([new Uint8Array(part.data)], { type: part.type }), part.filename)
    } else {
      formData.append(part.name ?? '', part.data.toString())
    }
  }

  return formData
}
