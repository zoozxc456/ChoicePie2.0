<template>
  <div v-if="adminAuth.adminUser">
    <LoadingSpinner
      v-if="adminDashboardStore.isLoading && !summary"
      size="md"
    />

    <template v-else-if="summary">
      <NuxtLink
        v-if="summary.pendingQuizReportCount > 0"
        to="/admin/quiz-reports"
        class="flex items-center justify-between gap-3 rounded-2xl bg-warning-50 border border-warning-200 text-warning-800 px-4 py-3 mb-4 hover:border-warning-300 transition"
      >
        <span class="flex items-center gap-2 font-semibold text-sm">
          <UIcon
            name="i-lucide-alert-triangle"
            class="text-lg"
          />
          {{ t('adminDashboard.pendingReportsAlert', { count: summary.pendingQuizReportCount }) }}
        </span>
        <span class="text-sm font-bold whitespace-nowrap">
          {{ t('adminDashboard.viewReports') }}
        </span>
      </NuxtLink>

      <div class="grid grid-cols-2 sm:grid-cols-4 gap-4 mb-2">
        <AdminStatCard
          v-for="stat in stats"
          :key="stat.label"
          :value="stat.value"
          :label="stat.label"
        />
      </div>

      <div class="grid grid-cols-1 sm:grid-cols-2 gap-4 mb-6">
        <div class="rounded-2xl bg-white border border-neutral-200 p-6 h-72 flex flex-col">
          <p class="text-lg font-extrabold mb-3">
            {{ t('adminDashboard.charts.newMembers') }}
          </p>
          <AdminDashboardTrendChart
            :label="t('adminDashboard.charts.newMembers')"
            color="#f8931d"
            :data-by-day="summary.newMembersByDay"
          />
        </div>
        <div class="rounded-2xl bg-white border border-neutral-200 p-6 h-72 flex flex-col">
          <p class="text-lg font-extrabold mb-3">
            {{ t('adminDashboard.charts.newQuizzes') }}
          </p>
          <AdminDashboardTrendChart
            :label="t('adminDashboard.charts.newQuizzes')"
            color="#64789a"
            :data-by-day="summary.newQuizzesByDay"
          />
        </div>
      </div>

      <AdminTopQuizList
        class="mb-6"
        :quizzes="summary.topQuizzes"
      />
    </template>

    <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
      <AdminNavCard
        v-for="card in navCards"
        :key="card.to"
        v-bind="card"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import LoadingSpinner from '~/components/common/LoadingSpinner.vue'
import AdminNavCard from '~/components/admin/AdminNavCard.vue'
import AdminStatCard from '~/components/admin/AdminStatCard.vue'
import AdminTopQuizList from '~/components/admin/AdminTopQuizList.vue'

definePageMeta({ layout: 'admin', middleware: ['admin-auth'] })

const { t } = useI18n()
const adminAuth = useAdminAuthStore()
const adminDashboardStore = useAdminDashboardStore()

const summary = computed(() => adminDashboardStore.summary)

await adminDashboardStore.fetchSummary()

const stats = computed(() => summary.value
  ? [
      { value: summary.value.totalMemberCount, label: t('adminDashboard.stats.totalMembers') },
      { value: summary.value.totalQuizCount, label: t('adminDashboard.stats.totalQuizzes') },
      { value: summary.value.suspendedMemberCount, label: t('adminDashboard.stats.suspendedMembers') },
      { value: summary.value.takenDownQuizCount, label: t('adminDashboard.stats.takenDownQuizzes') }
    ]
  : [])

const navCards = computed(() => [
  {
    to: '/admin/quizzes',
    icon: 'i-lucide-book-open',
    label: t('adminDashboard.nav.quizzes'),
    count: summary.value?.totalQuizCount ?? null,
    highlight: false
  },
  {
    to: '/admin/members',
    icon: 'i-lucide-users',
    label: t('adminDashboard.nav.members'),
    count: summary.value?.totalMemberCount ?? null,
    highlight: false
  },
  {
    to: '/admin/quiz-reports',
    icon: 'i-lucide-flag',
    label: t('adminDashboard.nav.quizReports'),
    count: summary.value?.pendingQuizReportCount ?? null,
    highlight: (summary.value?.pendingQuizReportCount ?? 0) > 0
  }
])
</script>

<script lang="ts">
export default {
  name: 'AdminDashboardPage'
}
</script>

<style scoped lang="scss"></style>
