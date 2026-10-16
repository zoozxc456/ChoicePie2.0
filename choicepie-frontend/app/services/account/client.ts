import type { AccountDto, ProfileDto, ChangePasswordRequest, DeleteAccountRequest } from '~/types/account'

export const useAccountClientApi = () => {
  const api = useApi()

  return {
    fetchAccount: () =>
      api.get<AccountDto>('/api/v1/account'),
    updateProfile: (name: string) =>
      api.put<ProfileDto>('/api/v1/account/profile', { name }),
    uploadAvatar: (file: File) => {
      const formData = new FormData()
      formData.append('file', file)
      return api.postForm<ProfileDto>('/api/v1/account/avatar', formData)
    },
    changePassword: (payload: ChangePasswordRequest) =>
      api.put('/api/v1/account/password', payload),
    linkGoogle: (idToken: string) =>
      api.post('/api/v1/account/google', { idToken }),
    unlinkGoogle: () =>
      api.del('/api/v1/account/google'),
    // useApi.del 不支援 request body，改走 BFF 的 POST /account/delete（見 server/api/v1/account/delete.post.ts），
    // 由它轉成後端的 DELETE /account 並清除 BFF session cookie。
    deleteAccount: (payload: DeleteAccountRequest) =>
      api.post('/api/v1/account/delete', payload)
  }
}
