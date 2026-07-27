import { useAdminMemberClientApi } from '~/services/admin/member'
import type {
  AdminListMembersQuery,
  AdminMemberCommentDto,
  AdminMemberDetailDto,
  AdminMemberSummaryDto,
  GameSessionSummaryDto,
  PagedResult,
  QuizSummaryDto
} from '~/types/api'

export const useAdminMemberStore = defineStore('adminMember', () => {
  const adminMemberApi = useAdminMemberClientApi()

  const members = ref<PagedResult<AdminMemberSummaryDto> | null>(null)
  const currentMember = ref<AdminMemberDetailDto | null>(null)
  const memberQuizzes = ref<PagedResult<QuizSummaryDto> | null>(null)
  const memberHostedSessions = ref<PagedResult<GameSessionSummaryDto> | null>(null)
  const memberPlayedSessions = ref<PagedResult<GameSessionSummaryDto> | null>(null)
  const memberComments = ref<PagedResult<AdminMemberCommentDto> | null>(null)
  const isLoading = ref(false)
  const isLoadingDetail = ref(false)
  const isLoadingQuizzes = ref(false)
  const isLoadingHostedSessions = ref(false)
  const isLoadingPlayedSessions = ref(false)
  const isLoadingComments = ref(false)
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

  const fetchMemberById = async (id: string) => {
    isLoadingDetail.value = true
    error.value = null
    try {
      currentMember.value = await adminMemberApi.fetchMemberById(id)
      return currentMember.value
    } catch (e) {
      error.value = '無法載入會員資料'
      console.error(e)
      throw e
    } finally {
      isLoadingDetail.value = false
    }
  }

  const fetchMemberQuizzes = async (id: string) => {
    isLoadingQuizzes.value = true
    try {
      memberQuizzes.value = await adminMemberApi.fetchMemberQuizzes(id)
      return memberQuizzes.value
    } catch (e) {
      console.error(e)
      throw e
    } finally {
      isLoadingQuizzes.value = false
    }
  }

  const fetchMemberHostedSessions = async (id: string) => {
    isLoadingHostedSessions.value = true
    try {
      memberHostedSessions.value = await adminMemberApi.fetchMemberHostedSessions(id)
      return memberHostedSessions.value
    } catch (e) {
      console.error(e)
      throw e
    } finally {
      isLoadingHostedSessions.value = false
    }
  }

  const fetchMemberPlayedSessions = async (id: string) => {
    isLoadingPlayedSessions.value = true
    try {
      memberPlayedSessions.value = await adminMemberApi.fetchMemberPlayedSessions(id)
      return memberPlayedSessions.value
    } catch (e) {
      console.error(e)
      throw e
    } finally {
      isLoadingPlayedSessions.value = false
    }
  }

  const fetchMemberComments = async (id: string) => {
    isLoadingComments.value = true
    try {
      memberComments.value = await adminMemberApi.fetchMemberComments(id)
      return memberComments.value
    } catch (e) {
      console.error(e)
      throw e
    } finally {
      isLoadingComments.value = false
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
      if (currentMember.value?.id === id) {
        currentMember.value = { ...currentMember.value, isSuspended: true, suspendedReason: reason, suspendedUntil: until }
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
      if (currentMember.value?.id === id) {
        currentMember.value = { ...currentMember.value, isSuspended: false, suspendedReason: null, suspendedUntil: null }
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
    currentMember,
    memberQuizzes,
    memberHostedSessions,
    memberPlayedSessions,
    memberComments,
    isLoading,
    isLoadingDetail,
    isLoadingQuizzes,
    isLoadingHostedSessions,
    isLoadingPlayedSessions,
    isLoadingComments,
    isSuspending,
    isUnsuspending,
    error,
    fetchMembers,
    fetchMemberById,
    fetchMemberQuizzes,
    fetchMemberHostedSessions,
    fetchMemberPlayedSessions,
    fetchMemberComments,
    suspendMember,
    unsuspendMember
  }
})
