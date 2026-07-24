import { useAdminMemberClientApi } from '~/services/admin/member'
import type { AdminListMembersQuery, AdminMemberSummaryDto, PagedResult } from '~/types/api'

export const useAdminMemberStore = defineStore('adminMember', () => {
  const adminMemberApi = useAdminMemberClientApi()

  const members = ref<PagedResult<AdminMemberSummaryDto> | null>(null)
  const isLoading = ref(false)
  const isSuspending = ref(false)
  const isUnsuspending = ref(false)
  const error = ref<string | null>(null)

  const fetchMembers = async (query?: AdminListMembersQuery) => {
    isLoading.value = true
    error.value = null
    try {
      members.value = await adminMemberApi.fetchMembers(query)
      return members.value
    } catch (e) {
      error.value = '無法載入會員列表'
      console.error(e)
      throw e
    } finally {
      isLoading.value = false
    }
  }

  const suspendMember = async (id: string, reason: string, until: string | null) => {
    isSuspending.value = true
    error.value = null
    try {
      await adminMemberApi.suspendMember(id, reason, until)
      if (members.value) {
        members.value = {
          ...members.value,
          items: members.value.items.map(m =>
            m.id === id ? { ...m, isSuspended: true, suspendedReason: reason, suspendedUntil: until } : m)
        }
      }
    } catch (e) {
      error.value = '停權會員失敗，請稍後再試'
      console.error(e)
      throw e
    } finally {
      isSuspending.value = false
    }
  }

  const unsuspendMember = async (id: string) => {
    isUnsuspending.value = true
    error.value = null
    try {
      await adminMemberApi.unsuspendMember(id)
      if (members.value) {
        members.value = {
          ...members.value,
          items: members.value.items.map(m =>
            m.id === id ? { ...m, isSuspended: false, suspendedReason: null, suspendedUntil: null } : m)
        }
      }
    } catch (e) {
      error.value = '解除停權失敗，請稍後再試'
      console.error(e)
      throw e
    } finally {
      isUnsuspending.value = false
    }
  }

  return {
    members,
    isLoading,
    isSuspending,
    isUnsuspending,
    error,
    fetchMembers,
    suspendMember,
    unsuspendMember
  }
})
