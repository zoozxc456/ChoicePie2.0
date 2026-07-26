<template>
  <div>
    <NuxtLink
      to="/admin/members"
      class="inline-flex items-center gap-1 text-sm text-neutral-500 hover:text-neutral-800 mb-4"
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
      class="text-sm text-neutral-400 text-center py-16"
    >
      {{ t('adminMemberDetail.notFound') }}
    </p>

    <div
      v-else
      class="rounded-2xl bg-white border border-neutral-200 p-6"
    >
      <div class="flex items-start justify-between gap-3">
        <div class="flex items-center gap-3 min-w-0">
          <img
            v-if="member.avatar"
            :src="member.avatar"
            class="w-12 h-12 rounded-full object-cover shrink-0"
            alt=""
          >
          <div
            v-else
            class="w-12 h-12 rounded-full bg-neutral-100 flex items-center justify-center text-lg font-bold text-neutral-400 shrink-0"
          >
            {{ member.name.charAt(0) }}
          </div>
          <div class="min-w-0">
            <p class="text-base font-bold truncate">
              {{ member.name }}
            </p>
            <p class="text-sm text-neutral-400 truncate">
              {{ member.email }}
            </p>
          </div>
        </div>
        <span
          class="text-[11px] px-2 py-1 rounded-full font-semibold whitespace-nowrap shrink-0"
          :class="member.isSuspended ? 'bg-error-100 text-error-800' : 'bg-success-100 text-success-800'"
        >
          {{ member.isSuspended ? t('adminMembers.statusSuspended') : t('adminMembers.statusActive') }}
        </span>
      </div>

      <dl class="grid grid-cols-2 gap-4 mt-6 text-sm">
        <div>
          <dt class="text-neutral-400">
            {{ t('adminMemberDetail.createdAt') }}
          </dt>
          <dd class="mt-0.5 font-medium">
            {{ formatDate(member.createdAt) }}
          </dd>
        </div>
        <div>
          <dt class="text-neutral-400">
            {{ t('adminMemberDetail.lastAiGenerationAt') }}
          </dt>
          <dd class="mt-0.5 font-medium">
            {{ member.lastAiGenerationAt ? formatDate(member.lastAiGenerationAt) : t('adminMemberDetail.never') }}
          </dd>
        </div>
        <div
          v-if="member.isSuspended"
          class="col-span-2"
        >
          <dt class="text-neutral-400">
            {{ t('adminMembers.suspendReasonLabel') }}
          </dt>
          <dd class="mt-0.5 font-medium text-error-500">
            {{ member.suspendedReason }}
          </dd>
        </div>
        <div v-if="member.isSuspended">
          <dt class="text-neutral-400">
            {{ t('adminMemberDetail.suspendedUntilLabel') }}
          </dt>
          <dd class="mt-0.5 font-medium">
            {{ member.suspendedUntil ? formatDate(member.suspendedUntil) : t('adminMembers.suspendedPermanently') }}
          </dd>
        </div>
      </dl>

      <div class="flex justify-end mt-6">
        <UButton
          v-if="!member.isSuspended"
          size="sm"
          color="error"
          variant="soft"
          @click="isModalOpen = true"
        >
          {{ t('adminMembers.suspendAction') }}
        </UButton>
        <UButton
          v-else
          size="sm"
          color="primary"
          variant="soft"
          :loading="adminMemberStore.isUnsuspending"
          @click="handleUnsuspend"
        >
          {{ t('adminMembers.unsuspendAction') }}
        </UButton>
      </div>
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

const { t, locale } = useI18n()
const route = useRoute()
const adminMemberStore = useAdminMemberStore()

const memberId = route.params.id as string
const isModalOpen = ref(false)

const member = computed(() => adminMemberStore.currentMember)

await adminMemberStore.fetchMemberById(memberId)

const formatDate = (iso: string) => new Date(iso).toLocaleDateString(locale.value)

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
