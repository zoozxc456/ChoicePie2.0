<template>
  <div>
    <NuxtLink
      to="/admin/members"
      class="inline-flex items-center gap-1 text-base text-neutral-500 hover:text-neutral-800 mb-4"
    >
      <UIcon name="i-lucide-arrow-left" />
      {{ t('adminMemberDetail.back') }}
    </NuxtLink>

    <div
      v-if="adminMemberStore.isLoadingDetail"
      class="flex justify-center py-16"
    >
      <UIcon
        name="i-lucide-loader-2"
        class="animate-spin text-3xl text-primary-500"
      />
    </div>

    <p
      v-else-if="!member"
      class="text-base text-neutral-400 text-center py-16"
    >
      {{ t('adminMemberDetail.notFound') }}
    </p>

    <AdminMemberDetailCard
      v-else
      :member="member"
      :is-unsuspending="adminMemberStore.isUnsuspending"
      @suspend="isModalOpen = true"
      @unsuspend="handleUnsuspend"
    />

    <AdminMemberTierUsageCard
      v-if="member"
      class="mt-4"
      :member="member"
      :tiers="adminMembershipTierStore.tiers"
      :ai-usage="adminMemberStore.memberAiUsage"
      :is-loading-usage="adminMemberStore.isLoadingAiUsage"
      :is-assigning-tier="adminMemberStore.isAssigningTier"
      @assign-tier="handleAssignTier"
    />

    <div
      v-if="member"
      class="grid grid-cols-1 lg:grid-cols-2 gap-4 mt-4"
    >
      <AdminMemberQuizList
        :quizzes="quizzes"
        :is-loading="adminMemberStore.isLoadingQuizzes"
      />

      <AdminMemberSessionList
        :title="t('adminMemberDetail.hostedSessionsTitle')"
        :sessions="hostedSessions"
        :is-loading="adminMemberStore.isLoadingHostedSessions"
        :empty-text="t('adminMemberDetail.hostedSessionsEmpty')"
      />

      <AdminMemberSessionList
        :title="t('adminMemberDetail.playedSessionsTitle')"
        :sessions="playedSessions"
        :is-loading="adminMemberStore.isLoadingPlayedSessions"
        :empty-text="t('adminMemberDetail.playedSessionsEmpty')"
        :count-text="playedSessions.length
          ? t('adminMemberDetail.playedSessionsCount', { count: playedSessions.length })
          : undefined"
      />

      <AdminMemberCommentList
        :comments="comments"
        :is-loading="adminMemberStore.isLoadingComments"
      />
    </div>

    <AdminSuspendMemberModal
      :open="isModalOpen"
      :is-submitting="adminMemberStore.isSuspending"
      @confirm="handleSuspend"
      @cancel="isModalOpen = false"
    />
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['admin-auth'] })

const { t } = useI18n()
const route = useRoute()
const adminMemberStore = useAdminMemberStore()
const adminMembershipTierStore = useAdminMembershipTierStore()

const memberId = route.params.id as string
const isModalOpen = ref(false)

const member = computed(() => adminMemberStore.currentMember)
const quizzes = computed(() => adminMemberStore.memberQuizzes?.items ?? [])
const hostedSessions = computed(() => adminMemberStore.memberHostedSessions?.items ?? [])
const playedSessions = computed(() => adminMemberStore.memberPlayedSessions?.items ?? [])
const comments = computed(() => adminMemberStore.memberComments?.items ?? [])

await Promise.all([
  adminMemberStore.fetchMemberById(memberId),
  adminMemberStore.fetchMemberQuizzes(memberId),
  adminMemberStore.fetchMemberHostedSessions(memberId),
  adminMemberStore.fetchMemberPlayedSessions(memberId),
  adminMemberStore.fetchMemberComments(memberId),
  adminMemberStore.fetchMemberAiUsage(memberId),
  adminMembershipTierStore.fetchTiers()
])

const handleAssignTier = async (tierId: string) => {
  const tierName = adminMembershipTierStore.tiers.find(tier => tier.id === tierId)?.name ?? ''
  try {
    await adminMemberStore.assignMemberTier(memberId, tierId, tierName)
  } catch {
    // 錯誤已寫入 adminMemberStore.error，這裡不需要額外處理
  }
}

const handleSuspend = async (reason: string, until: string | null) => {
  try {
    await adminMemberStore.suspendMember(memberId, reason, until)
    isModalOpen.value = false
  } catch {
    // 錯誤已寫入 adminMemberStore.error，這裡不需要額外處理
  }
}

const handleUnsuspend = async () => {
  try {
    await adminMemberStore.unsuspendMember(memberId)
  } catch {
    // 錯誤已寫入 adminMemberStore.error，這裡不需要額外處理
  }
}
</script>

<script lang="ts">
export default {
  name: 'AdminMemberDetailPage'
}
</script>

<style scoped lang="scss"></style>
