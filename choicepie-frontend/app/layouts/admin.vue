<template>
  <div class="h-screen flex bg-neutral-100">
    <aside class="w-60 shrink-0 h-screen sticky top-0 bg-secondary-900 text-white flex flex-col p-4">
      <NuxtLink
        to="/admin"
        class="flex items-center gap-2 px-2 py-2 mb-6 font-extrabold text-base"
      >
        <span class="text-xl">🥧</span>
        <span>{{ t('adminDashboard.title') }}</span>
      </NuxtLink>

      <nav class="flex flex-col gap-1">
        <NuxtLink
          v-for="item in navItems"
          :key="item.to"
          :to="item.to"
          class="flex items-center gap-3 rounded-xl px-3 py-2.5 text-sm font-medium text-secondary-200 hover:bg-secondary-800 hover:text-white transition-colors"
          active-class="!bg-primary-500 !text-white"
        >
          <UIcon
            :name="item.icon"
            class="text-lg"
          />
          {{ item.label }}
        </NuxtLink>
      </nav>

      <div class="mt-auto pt-4 border-t border-secondary-800">
        <div
          v-if="adminAuth.adminUser"
          class="flex items-center gap-2 rounded-xl px-2 py-2"
        >
          <div class="w-8 h-8 rounded-full bg-primary-500 flex items-center justify-center text-sm font-bold shrink-0">
            {{ adminAuth.adminUser.name.charAt(0) }}
          </div>
          <div class="flex-1 min-w-0">
            <p class="text-sm font-semibold truncate">
              {{ adminAuth.adminUser.name }}
            </p>
            <p class="text-xs text-secondary-300 truncate">
              {{ adminAuth.adminUser.email }}
            </p>
          </div>
          <UButton
            icon="i-lucide-log-out"
            size="sm"
            color="neutral"
            variant="ghost"
            class="text-secondary-200 hover:text-white"
            :aria-label="t('adminDashboard.logout')"
            @click="adminAuth.logout()"
          />
        </div>
      </div>
    </aside>

    <div class="flex-1 min-w-0 h-screen flex flex-col">
      <header class="h-14 shrink-0 bg-white border-b border-neutral-200 px-6 flex items-center">
        <h1 class="text-sm font-bold text-neutral-800">
          {{ pageTitle }}
        </h1>
      </header>
      <main class="flex-1 min-h-0 p-6 overflow-y-auto">
        <slot />
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
const { t } = useI18n()
const adminAuth = useAdminAuthStore()
const route = useRoute()

const navItems = computed(() => [
  { to: '/admin', icon: 'i-lucide-layout-dashboard', label: t('adminDashboard.title') },
  { to: '/admin/quizzes', icon: 'i-lucide-book-open', label: t('adminDashboard.nav.quizzes') },
  { to: '/admin/members', icon: 'i-lucide-users', label: t('adminDashboard.nav.members') },
  { to: '/admin/quiz-reports', icon: 'i-lucide-flag', label: t('adminDashboard.nav.quizReports') }
])

const pageTitle = computed(() => navItems.value.find(item => item.to === route.path)?.label ?? t('adminDashboard.title'))
</script>

<script lang="ts">
export default {
  name: 'AdminLayout'
}
</script>

<style scoped lang="scss"></style>
