<template>
  <div class="min-h-screen bg-neutral-50">
    <header class="bg-white border-b border-neutral-200 px-6 h-14 flex items-center justify-between">
      <NuxtLink
        to="/admin"
        class="font-extrabold text-sm"
      >
        🥧 {{ t('adminDashboard.title') }}
      </NuxtLink>
      <div
        v-if="adminAuth.adminUser"
        class="flex items-center gap-3 text-sm"
      >
        <span class="text-neutral-600">{{ adminAuth.adminUser.name }}</span>
        <UButton
          size="sm"
          color="neutral"
          variant="outline"
          @click="adminAuth.logout()"
        >
          {{ t('adminDashboard.logout') }}
        </UButton>
      </div>
    </header>
    <main class="p-6">
      <slot />
    </main>
  </div>
</template>

<script setup lang="ts">
const { t } = useI18n()
const adminAuth = useAdminAuthStore()
</script>

<script lang="ts">
export default {
  name: 'AdminLayout'
}
</script>

<style scoped lang="scss"></style>
