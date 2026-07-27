<template>
  <div v-if="adminAuth.adminUser">
    <h2 class="text-lg font-extrabold mb-1">
      {{ t('adminDashboard.welcome', { name: adminAuth.adminUser.name }) }}
    </h2>
    <p class="text-base text-neutral-500 mb-6">
      {{ adminAuth.adminUser.email }} · {{ adminAuth.adminUser.role }}
    </p>

    <div class="grid grid-cols-1 sm:grid-cols-3 gap-4">
      <NuxtLink
        v-for="card in navCards"
        :key="card.to"
        :to="card.to"
        class="rounded-2xl bg-white border border-neutral-200 p-5 flex flex-col gap-3 hover:border-primary-300 hover:shadow-cp-md transition"
      >
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

const navCards = computed(() => [
  { to: '/admin/quizzes', icon: 'i-lucide-book-open', label: t('adminDashboard.nav.quizzes') },
  { to: '/admin/members', icon: 'i-lucide-users', label: t('adminDashboard.nav.members') },
  { to: '/admin/quiz-reports', icon: 'i-lucide-flag', label: t('adminDashboard.nav.quizReports') }
])
</script>

<script lang="ts">
export default {
  name: 'AdminDashboardPage'
}
</script>

<style scoped lang="scss"></style>
