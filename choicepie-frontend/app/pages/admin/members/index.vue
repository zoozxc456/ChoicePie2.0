<template>
  <div>
    <h1 class="text-lg font-extrabold mb-4">
      {{ t('adminMembers.title') }}
    </h1>

    <AdminSearchInput
      v-model="search"
      :placeholder="t('adminMembers.searchPlaceholder')"
    />

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
      class="text-base text-neutral-400 text-center py-16"
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
        class="relative grid grid-cols-[1fr_112px] rounded-2xl bg-white border border-neutral-200 shadow-sm overflow-hidden transition-colors hover:border-primary-300"
      >
        <NuxtLink
          :to="`/admin/members/${member.id}`"
          class="absolute inset-0 z-10"
          :aria-label="member.name"
        />

        <div class="p-4 min-w-0 flex items-center justify-between gap-3 pointer-events-none">
          <div class="min-w-0 flex-1">
            <div class="flex items-center gap-2 min-w-0">
              <p class="text-lg font-bold truncate">
                {{ member.name }}
              </p>
              <span
                v-if="member.tierName"
                class="shrink-0 text-xs font-semibold px-2 py-0.5 rounded-full bg-primary-50 text-primary-700 border border-primary-200"
              >
                {{ member.tierName }}
              </span>
            </div>
            <p class="text-base text-neutral-400 mt-0.5 truncate">
              {{ member.email }}
            </p>
          </div>

          <div class="relative shrink-0">
            <UButton
              v-if="!member.isSuspended"
              size="sm"
              color="error"
              variant="soft"
              class="pointer-events-auto"
              @click="openSuspendModal(member.id)"
            >
              {{ t('adminMembers.suspendAction') }}
            </UButton>
            <UButton
              v-else
              size="sm"
              color="primary"
              variant="soft"
              class="pointer-events-auto"
              :loading="adminMemberStore.isUnsuspending"
              @click="handleUnsuspend(member.id)"
            >
              {{ t('adminMembers.unsuspendAction') }}
            </UButton>
          </div>
        </div>

        <AdminStatusStub
          :icon="member.isSuspended ? 'i-lucide-shield-off' : 'i-lucide-shield-check'"
          :label="member.isSuspended ? t('adminMembers.statusSuspended') : t('adminMembers.statusActive')"
          :border-class="member.isSuspended ? 'border-error-200' : 'border-neutral-200'"
          :bg-class="member.isSuspended ? 'bg-error-50' : 'bg-success-50'"
          :text-class="member.isSuspended ? 'text-error-700' : 'text-success-700'"
        >
          <template
            v-if="member.isSuspended"
            #subtext
          >
            <p class="text-xs text-error-600 leading-snug">
              {{ member.suspendedUntil
                ? t('adminMembers.suspendedUntil', { date: formatDate(member.suspendedUntil) })
                : t('adminMembers.suspendedPermanently') }}
            </p>
          </template>
        </AdminStatusStub>
      </div>
    </div>

    <AdminPagination
      :page-number="pageNumber"
      :total-pages="totalPages"
      @update:page-number="handlePageChange"
    />

    <AdminSuspendMemberModal
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
const pageNumber = computed(() => adminMemberStore.members?.pageNumber ?? 1)
const totalPages = computed(() => adminMemberStore.members?.totalPages
  ?? Math.ceil((adminMemberStore.members?.totalCount ?? 0) / (adminMemberStore.members?.pageSize ?? 1)))

await adminMemberStore.fetchMembers()

const formatDate = (iso: string) => new Date(iso).toLocaleDateString(locale.value)

watch(search, () => {
  if (searchDebounce) clearTimeout(searchDebounce)
  searchDebounce = setTimeout(() => {
    adminMemberStore.fetchMembers({ search: search.value || undefined, pageNumber: 1 })
  }, 300)
})

const handlePageChange = (page: number) => {
  adminMemberStore.fetchMembers({ search: search.value || undefined, pageNumber: page })
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
