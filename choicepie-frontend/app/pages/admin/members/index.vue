<template>
  <div class="max-w-4xl mx-auto">
    <h1 class="text-lg font-extrabold mb-4">
      {{ t('adminMembers.title') }}
    </h1>

    <input
      v-model="search"
      type="text"
      class="w-full rounded-xl border border-neutral-200 px-4 py-2 text-sm mb-4"
      :placeholder="t('adminMembers.searchPlaceholder')"
      @input="handleSearchInput"
    >

    <div
      v-if="adminMemberStore.isLoading"
      class="flex justify-center py-16"
    >
      <UIcon
        name="i-lucide-loader-2"
        class="animate-spin text-3xl text-primary-500"
      />
    </div>

    <p
      v-else-if="!members.length"
      class="text-sm text-neutral-400 text-center py-16"
    >
      {{ t('adminMembers.empty') }}
    </p>

    <div
      v-else
      class="flex flex-col gap-3"
    >
      <div
        v-for="member in members"
        :key="member.id"
        class="rounded-2xl bg-white border border-neutral-200 p-4"
      >
        <div class="flex items-start justify-between gap-3">
          <div class="min-w-0 flex-1">
            <p class="text-sm font-bold truncate">
              {{ member.name }}
            </p>
            <p class="text-xs text-neutral-400 mt-0.5 truncate">
              {{ member.email }}
            </p>
            <p
              v-if="member.isSuspended"
              class="text-xs text-error-500 mt-1"
            >
              {{ member.suspendedUntil
                ? t('adminMembers.suspendedUntil', { date: formatDate(member.suspendedUntil) })
                : t('adminMembers.suspendedPermanently') }}
            </p>
          </div>
          <span
            class="text-[11px] px-2 py-1 rounded-full font-semibold whitespace-nowrap shrink-0"
            :class="member.isSuspended ? 'bg-error-100 text-error-800' : 'bg-success-100 text-success-800'"
          >
            {{ member.isSuspended ? t('adminMembers.statusSuspended') : t('adminMembers.statusActive') }}
          </span>
        </div>

        <div class="flex justify-end mt-3">
          <UButton
            v-if="!member.isSuspended"
            size="sm"
            color="error"
            variant="soft"
            @click="openSuspendModal(member.id)"
          >
            {{ t('adminMembers.suspendAction') }}
          </UButton>
          <UButton
            v-else
            size="sm"
            color="primary"
            variant="soft"
            :loading="adminMemberStore.isUnsuspending"
            @click="handleUnsuspend(member.id)"
          >
            {{ t('adminMembers.unsuspendAction') }}
          </UButton>
        </div>
      </div>
    </div>

    <SuspendMemberModal
      :open="isModalOpen"
      :is-submitting="adminMemberStore.isSuspending"
      @confirm="handleSuspend"
      @cancel="closeSuspendModal"
    />
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['admin-auth'] })

const { t, locale } = useI18n()
const adminMemberStore = useAdminMemberStore()

const search = ref('')
const isModalOpen = ref(false)
const targetMemberId = ref<string | null>(null)
let searchDebounce: ReturnType<typeof setTimeout> | null = null

const members = computed(() => adminMemberStore.members?.items ?? [])

await adminMemberStore.fetchMembers()

const formatDate = (iso: string) => new Date(iso).toLocaleDateString(locale.value)

const handleSearchInput = () => {
  if (searchDebounce) clearTimeout(searchDebounce)
  searchDebounce = setTimeout(() => {
    adminMemberStore.fetchMembers({ search: search.value || undefined })
  }, 300)
}

const openSuspendModal = (memberId: string) => {
  targetMemberId.value = memberId
  isModalOpen.value = true
}

const closeSuspendModal = () => {
  isModalOpen.value = false
  targetMemberId.value = null
}

const handleSuspend = async (reason: string, until: string | null) => {
  if (!targetMemberId.value) return
  try {
    await adminMemberStore.suspendMember(targetMemberId.value, reason, until)
    closeSuspendModal()
  } catch {
    // 錯誤已寫入 adminMemberStore.error，這裡不需要額外處理
  }
}

const handleUnsuspend = async (memberId: string) => {
  try {
    await adminMemberStore.unsuspendMember(memberId)
  } catch {
    // 錯誤已寫入 adminMemberStore.error，這裡不需要額外處理
  }
}
</script>

<script lang="ts">
export default {
  name: 'AdminMembersPage'
}
</script>

<style scoped lang="scss"></style>
