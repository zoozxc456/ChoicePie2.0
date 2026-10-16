<template>
  <div>
    <h1 class="text-lg font-extrabold mb-4">
      {{ t('adminMembers.title') }}
    </h1>

    <AdminSearchInput
      v-model="search"
      :placeholder="t('adminMembers.searchPlaceholder')"
    />

    <LoadingSpinner
      v-if="adminMemberStore.isLoading"
      size="md"
    />

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
      <AdminListRow
        v-for="member in members"
        :key="member.id"
        :to="`/admin/members/${member.id}`"
        :aria-label="member.name"
      >
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
        <template #action>
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
        </template>
        <template #stub>
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
        </template>
      </AdminListRow>
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
import AdminListRow from '~/components/admin/AdminListRow.vue'
import LoadingSpinner from '~/components/common/LoadingSpinner.vue'
import { useDebouncedSearch } from '~/composables/useDebouncedSearch'
import { totalPagesOf } from '~/utils/pagination'

definePageMeta({ layout: 'admin', middleware: ['admin-auth'] })

const { t, locale } = useI18n()
const adminMemberStore = useAdminMemberStore()

const isModalOpen = ref(false)
const targetMemberId = ref<string | null>(null)

const members = computed(() => adminMemberStore.members?.items ?? [])
const pageNumber = computed(() => adminMemberStore.members?.pageNumber ?? 1)
const totalPages = computed(() => totalPagesOf(adminMemberStore.members))

await adminMemberStore.fetchMembers()

const formatDate = (iso: string) => formatLocaleDate(iso, locale.value)

const { search } = useDebouncedSearch((term) => {
  adminMemberStore.fetchMembers({ search: term, pageNumber: 1 })
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
