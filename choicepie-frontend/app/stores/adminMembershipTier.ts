import { useAdminMembershipTierClientApi } from '~/services/admin/membershipTier'
import type { CreateMembershipTierRequest, MembershipTierDto, UpdateMembershipTierRequest } from '~/types/api'

export const useAdminMembershipTierStore = defineStore('adminMembershipTier', () => {
  const membershipTierApi = useAdminMembershipTierClientApi()

  const tiers = ref<MembershipTierDto[]>([])
  const isLoading = ref(false)
  const isSaving = ref(false)
  const error = ref<string | null>(null)

  const fetchTiers = async () => {
    isLoading.value = true
    error.value = null
    try {
      tiers.value = await membershipTierApi.fetchTiers()
      return tiers.value
    } catch (e) {
      error.value = '無法載入會員等級列表'
      console.error(e)
      throw e
    } finally {
      isLoading.value = false
    }
  }

  const createTier = async (request: CreateMembershipTierRequest) => {
    isSaving.value = true
    error.value = null
    try {
      const created = await membershipTierApi.createTier(request)
      tiers.value = [...tiers.value, created]
      return created
    } catch (e) {
      error.value = '新增會員等級失敗，請稍後再試'
      console.error(e)
      throw e
    } finally {
      isSaving.value = false
    }
  }

  const updateTier = async (id: string, request: UpdateMembershipTierRequest) => {
    isSaving.value = true
    error.value = null
    try {
      const updated = await membershipTierApi.updateTier(id, request)
      tiers.value = tiers.value.map(t => (t.id === id ? updated : t))
      return updated
    } catch (e) {
      error.value = '更新會員等級失敗，請稍後再試'
      console.error(e)
      throw e
    } finally {
      isSaving.value = false
    }
  }

  return {
    tiers,
    isLoading,
    isSaving,
    error,
    fetchTiers,
    createTier,
    updateTier
  }
})
