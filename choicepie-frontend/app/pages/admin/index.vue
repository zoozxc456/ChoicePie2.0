<template>
  <div v-if="adminAuth.adminUser">
    <h2 class="text-lg font-extrabold mb-1">
      {{ t('adminDashboard.welcome', { name: adminAuth.adminUser.name }) }}
    </h2>
    <p class="text-base text-neutral-500 mb-6">
      {{ adminAuth.adminUser.email }} · {{ adminAuth.adminUser.role }}
    </p>

    <div
      v-if="adminDashboardStore.isLoading && !summary"
      class="flex justify-center py-16"
    >
      <UIcon
        name="i-lucide-loader-2"
        class="animate-spin text-3xl text-primary-500"
      />
    </div>

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
        <div class="rounded-2xl bg-white border border-neutral-200 p-4">
          <p class="text-2xl font-extrabold">
            {{ summary.totalMemberCount }}
          </p>
          <p class="text-xs text-neutral-500 mt-1">
            {{ t('adminDashboard.stats.totalMembers') }}
          </p>
        </div>
        <div class="rounded-2xl bg-white border border-neutral-200 p-4">
          <p class="text-2xl font-extrabold">
            {{ summary.totalQuizCount }}
          </p>
          <p class="text-xs text-neutral-500 mt-1">
            {{ t('adminDashboard.stats.totalQuizzes') }}
          </p>
        </div>
        <div class="rounded-2xl bg-white border border-neutral-200 p-4">
          <p class="text-2xl font-extrabold">
            {{ summary.suspendedMemberCount }}
          </p>
          <p class="text-xs text-neutral-500 mt-1">
            {{ t('adminDashboard.stats.suspendedMembers') }}
          </p>
        </div>
        <div class="rounded-2xl bg-white border border-neutral-200 p-4">
          <p class="text-2xl font-extrabold">
            {{ summary.takenDownQuizCount }}
          </p>
          <p class="text-xs text-neutral-500 mt-1">
            {{ t('adminDashboard.stats.takenDownQuizzes') }}
          </p>
        </div>
      </div>

      <div class="flex flex-wrap gap-2 mb-6">
        <span class="text-xs px-2.5 py-1 rounded-full bg-neutral-100 text-neutral-600">
          {{ t('adminDashboard.recentActivity.newMembers', { count: summary.newMemberCountLast7Days }) }}
        </span>
        <span class="text-xs px-2.5 py-1 rounded-full bg-neutral-100 text-neutral-600">
          {{ t('adminDashboard.recentActivity.newQuizzes', { count: summary.newQuizCountLast7Days }) }}
        </span>
        <span class="text-xs px-2.5 py-1 rounded-full bg-neutral-100 text-neutral-600">
          {{ t('adminDashboard.recentActivity.takenDown', { count: summary.takenDownQuizCountLast7Days }) }}
        </span>
      </div>
    </template>

    <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
      <NuxtLink
        v-for="card in navCards"
        :key="card.to"
        :to="card.to"
        class="relative rounded-2xl bg-white border border-neutral-200 p-5 flex flex-col gap-3 hover:border-primary-300 hover:shadow-cp-md transition"
      >
        <span
          v-if="card.count !== null"
          class="absolute top-3 right-3 text-[11px] px-2 py-0.5 rounded-full font-bold"
          :class="card.highlight
            ? 'bg-warning-100 text-warning-800'
            : 'bg-neutral-100 text-neutral-600'"
        >
          {{ card.count }}
        </span>
        <UIcon
          :name="card.icon"
          class="text-2xl text-primary-500"
        />
        <span class="font-bold text-base">{{ card.label }}</span>
      </NuxtLink>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['admin-auth'] })

const { t } = useI18n()
const adminAuth = useAdminAuthStore()
const adminDashboardStore = useAdminDashboardStore()

const summary = computed(() => adminDashboardStore.summary)

await adminDashboardStore.fetchSummary()

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
