import { useAccountClientApi } from '~/services/account/client'
import type { AccountDto, ChangePasswordRequest, DeleteAccountRequest, ProfileDto } from '~/types/account'

export const useAccountStore = defineStore('account', () => {
  const accountApi = useAccountClientApi()
  const auth = useAuthStore()

  const account = ref<AccountDto | null>(null)
  const isLoading = ref(false)

  // 名稱／頭像變更後同步 auth.user，讓 header 等使用 auth.user 的地方即時更新。
  const syncProfile = (profile: ProfileDto) => {
    if (account.value) {
      account.value = { ...account.value, name: profile.name, avatar: profile.avatar }
    }
    if (auth.user) {
      auth.setUser({ ...auth.user, name: profile.name, avatar: profile.avatar ?? undefined })
    }
  }

  const fetchAccount = async () => {
    isLoading.value = true
    try {
      account.value = await accountApi.fetchAccount()
    } finally {
      isLoading.value = false
    }
  }

  const updateName = async (name: string) => {
    syncProfile(await accountApi.updateProfile(name))
  }

  const uploadAvatar = async (file: File) => {
    syncProfile(await accountApi.uploadAvatar(file))
  }

  const changePassword = async (payload: ChangePasswordRequest) => {
    await accountApi.changePassword(payload)
    if (account.value) account.value = { ...account.value, hasPassword: true }
  }

  const linkGoogle = async () => {
    const { requestIdToken } = useGoogleIdentity()
    const idToken = await requestIdToken()
    await accountApi.linkGoogle(idToken)
    if (account.value) account.value = { ...account.value, googleLinked: true }
  }

  const unlinkGoogle = async () => {
    await accountApi.unlinkGoogle()
    if (account.value) account.value = { ...account.value, googleLinked: false }
  }

  // 成功後伺服器端 session 已被 BFF 清除；這裡清本地狀態，導頁與提示交給呼叫端（需要 Nuxt context）。
  const deleteAccount = async (payload: DeleteAccountRequest) => {
    await accountApi.deleteAccount(payload)
    account.value = null
    auth.clearSession()
  }

  return {
    account, isLoading,
    fetchAccount, updateName, uploadAvatar, changePassword, linkGoogle, unlinkGoogle, deleteAccount
  }
})
