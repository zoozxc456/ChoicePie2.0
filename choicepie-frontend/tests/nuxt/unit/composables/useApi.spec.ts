import { describe, it, expect, beforeEach, vi } from 'vitest'
import type { ApiEnvelope } from '~/types/api'
import { fetchMock, useAuthStoreMock } from './mocks/useApi.mock'

const importUseApi = async () => {
  vi.resetModules()
  const mod = await import('~/composables/useApi')
  return mod
}

const envelope = <T>(data: T): ApiEnvelope<T> => ({
  code: 'OK',
  status: true,
  data,
  message: ''
})

const fetchError = (status: number, message = 'error', code = 'ERR') => {
  const err = new Error(message) as Error & { status: number, data: ApiEnvelope<unknown> }
  err.status = status
  err.data = { code, status: false, data: null, message }
  return err
}

describe('useApi', () => {
  beforeEach(() => {
    vi.clearAllMocks()
  })

  describe('成功請求', () => {
    it('get 解開 envelope 並回傳 data', async () => {
      fetchMock.mockResolvedValue(envelope({ id: '1' }))
      const { useApi } = await importUseApi()
      const api = useApi()

      const result = await api.get<{ id: string }>('/api/v1/quizzes/1')

      expect(result).toEqual({ id: '1' })
      expect(fetchMock).toHaveBeenCalledWith('/api/v1/quizzes/1', expect.objectContaining({
        method: 'GET',
        baseURL: 'https://api.example.test',
        credentials: 'include'
      }))
    })

    it('post 帶入 body', async () => {
      fetchMock.mockResolvedValue(envelope({ ok: true }))
      const { useApi } = await importUseApi()
      const api = useApi()

      await api.post('/api/v1/quizzes', { title: 'New' })

      expect(fetchMock).toHaveBeenCalledWith('/api/v1/quizzes', expect.objectContaining({
        method: 'POST',
        body: { title: 'New' }
      }))
    })

    it('put 帶入 body', async () => {
      fetchMock.mockResolvedValue(envelope({ ok: true }))
      const { useApi } = await importUseApi()
      const api = useApi()

      await api.put('/api/v1/quizzes/1', { title: 'Updated' })

      expect(fetchMock).toHaveBeenCalledWith('/api/v1/quizzes/1', expect.objectContaining({
        method: 'PUT',
        body: { title: 'Updated' }
      }))
    })

    it('del 不帶 body', async () => {
      fetchMock.mockResolvedValue(envelope(undefined))
      const { useApi } = await importUseApi()
      const api = useApi()

      await api.del('/api/v1/quizzes/1')

      expect(fetchMock).toHaveBeenCalledWith('/api/v1/quizzes/1', expect.objectContaining({
        method: 'DELETE'
      }))
    })

    it('get 帶入 query', async () => {
      fetchMock.mockResolvedValue(envelope([]))
      const { useApi } = await importUseApi()
      const api = useApi()

      await api.get('/api/v1/quizzes', { tag: 'science' })

      expect(fetchMock).toHaveBeenCalledWith('/api/v1/quizzes', expect.objectContaining({
        query: { tag: 'science' }
      }))
    })
  })

  describe('非 401 錯誤', () => {
    it('轉換成 ApiError 並拋出，不觸發 refresh', async () => {
      fetchMock.mockRejectedValue(fetchError(500, '伺服器錯誤', 'SERVER_ERROR'))
      const { useApi, ApiError } = await importUseApi()
      const api = useApi()

      await expect(api.get('/api/v1/quizzes')).rejects.toMatchObject({
        message: '伺服器錯誤',
        code: 'SERVER_ERROR',
        status: 500
      })
      await expect(api.get('/api/v1/quizzes')).rejects.toBeInstanceOf(ApiError)
      expect(useAuthStoreMock).not.toHaveBeenCalled()
    })

    it('沒有 data.message 時退回 error.message，沒有時退回預設訊息', async () => {
      const err = new Error('network down') as Error & { status: number }
      err.status = 0
      fetchMock.mockRejectedValue(err)
      const { useApi } = await importUseApi()
      const api = useApi()

      await expect(api.get('/api/v1/quizzes')).rejects.toMatchObject({
        message: 'network down',
        code: 'UNKNOWN',
        status: 0
      })
    })
  })

  describe('401 refresh 流程', () => {
    it('refresh 成功時 retry 一次並回傳結果', async () => {
      const fetchMe = vi.fn().mockResolvedValue(true)
      useAuthStoreMock.mockReturnValue({ fetchMe, logout: vi.fn() })
      fetchMock
        .mockRejectedValueOnce(fetchError(401))
        .mockResolvedValueOnce(envelope({ id: '1' }))
      const { useApi } = await importUseApi()
      const api = useApi()

      const result = await api.get<{ id: string }>('/api/v1/quizzes/1')

      expect(result).toEqual({ id: '1' })
      expect(fetchMe).toHaveBeenCalledTimes(1)
      expect(fetchMock).toHaveBeenCalledTimes(2)
    })

    it('refresh 失敗時呼叫 logout 並拋出原始 401 錯誤', async () => {
      const fetchMe = vi.fn().mockResolvedValue(false)
      const logout = vi.fn().mockResolvedValue(undefined)
      useAuthStoreMock.mockReturnValue({ fetchMe, logout })
      fetchMock.mockRejectedValue(fetchError(401))
      const { useApi } = await importUseApi()
      const api = useApi()

      await expect(api.get('/api/v1/quizzes/1')).rejects.toMatchObject({ status: 401 })

      expect(fetchMe).toHaveBeenCalledTimes(1)
      expect(logout).toHaveBeenCalledWith('/')
    })

    it('retry 後仍 401 時直接拋出，不會無限重試', async () => {
      const fetchMe = vi.fn().mockResolvedValue(true)
      useAuthStoreMock.mockReturnValue({ fetchMe, logout: vi.fn() })
      fetchMock.mockRejectedValue(fetchError(401))
      const { useApi } = await importUseApi()
      const api = useApi()

      await expect(api.get('/api/v1/quizzes/1')).rejects.toMatchObject({ status: 401 })

      expect(fetchMe).toHaveBeenCalledTimes(1)
      expect(fetchMock).toHaveBeenCalledTimes(2)
    })

    it('refresh/logout 端點本身 401 不觸發 refresh，直接拋出', async () => {
      useAuthStoreMock.mockReturnValue({ fetchMe: vi.fn(), logout: vi.fn() })
      fetchMock.mockRejectedValue(fetchError(401))
      const { useApi } = await importUseApi()
      const api = useApi()

      await expect(api.post('/api/v1/auth/refresh')).rejects.toMatchObject({ status: 401 })

      expect(useAuthStoreMock().fetchMe).not.toHaveBeenCalled()
    })

    it('同時多個請求 401 時只觸發一次 refresh', async () => {
      const fetchMe = vi.fn().mockResolvedValue(true)
      useAuthStoreMock.mockReturnValue({ fetchMe, logout: vi.fn() })
      fetchMock
        .mockRejectedValueOnce(fetchError(401))
        .mockRejectedValueOnce(fetchError(401))
        .mockResolvedValueOnce(envelope({ id: 'a' }))
        .mockResolvedValueOnce(envelope({ id: 'b' }))
      const { useApi } = await importUseApi()
      const api = useApi()

      const [resultA, resultB] = await Promise.all([
        api.get('/api/v1/a'),
        api.get('/api/v1/b')
      ])

      expect(resultA).toEqual({ id: 'a' })
      expect(resultB).toEqual({ id: 'b' })
      expect(fetchMe).toHaveBeenCalledTimes(1)
    })
  })
})
