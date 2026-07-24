<template>
  <div
    v-if="adminAuth.adminUser"
    class="max-w-2xl mx-auto rounded-2xl bg-white border border-neutral-200 p-6"
  >
    <h1 class="text-lg font-extrabold mb-4">
      {{ t('adminDashboard.welcome', { name: adminAuth.adminUser.name }) }}
    </h1>
    <dl class="flex flex-col gap-2 text-sm">
      <div class="flex justify-between">
        <dt class="text-neutral-500">
          {{ t('adminDashboard.email') }}
        </dt>
        <dd>{{ adminAuth.adminUser.email }}</dd>
      </div>
      <div class="flex justify-between">
        <dt class="text-neutral-500">
          {{ t('adminDashboard.role') }}
        </dt>
        <dd>{{ adminAuth.adminUser.role }}</dd>
      </div>
    </dl>

    <div class="flex flex-wrap gap-3 mt-6">
      <NuxtLink
        to="/admin/quizzes"
        class="flex-1 min-w-36"
      >
        <UButton
          block
          color="primary"
          variant="soft"
        >
          {{ t('adminDashboard.nav.quizzes') }}
        </UButton>
      </NuxtLink>
      <NuxtLink
        to="/admin/members"
        class="flex-1 min-w-36"
      >
        <UButton
          block
          color="primary"
          variant="soft"
        >
          {{ t('adminDashboard.nav.members') }}
        </UButton>
      </NuxtLink>
      <NuxtLink
        to="/admin/quiz-reports"
        class="flex-1 min-w-36"
      >
        <UButton
          block
          color="primary"
          variant="soft"
        >
          {{ t('adminDashboard.nav.quizReports') }}
        </UButton>
      </NuxtLink>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'admin', middleware: ['admin-auth'] })

const { t } = useI18n()
const adminAuth = useAdminAuthStore()
</script>

<script lang="ts">
export default {
  name: 'AdminDashboardPage'
}
</script>

<style scoped lang="scss"></style>
