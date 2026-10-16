<template>
  <div class="min-h-[calc(100vh-64px)] bg-neutral-50 px-6 py-10">
    <div class="w-full max-w-xl mx-auto flex flex-col gap-5">
      <h1 class="text-2xl font-extrabold text-cp-text-primary">
        {{ t('settings.title') }}
      </h1>

      <p
        v-if="loadError"
        class="text-sm text-error-500"
      >
        {{ loadError }}
      </p>

      <template v-if="accountStore.account">
        <ProfileSection />
        <PasswordSection />
        <LoginMethodsSection />
        <DeleteAccountSection />
      </template>
      <p
        v-else-if="accountStore.isLoading"
        class="text-sm text-neutral-500"
      >
        {{ t('settings.loading') }}
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
import ProfileSection from '~/components/settings/ProfileSection.vue'
import PasswordSection from '~/components/settings/PasswordSection.vue'
import LoginMethodsSection from '~/components/settings/LoginMethodsSection.vue'
import DeleteAccountSection from '~/components/settings/DeleteAccountSection.vue'

definePageMeta({ layout: 'content', middleware: ['auth'] })

const { t } = useI18n()
useHead({ title: () => t('settings.title') })

const accountStore = useAccountStore()
const loadError = ref('')

try {
  await accountStore.fetchAccount()
} catch (e) {
  loadError.value = getApiErrorMessage(e, t('settings.common.failed'))
}
</script>

<script lang="ts">
export default {
  name: 'SettingsPage'
}
</script>

<style scoped lang="scss"></style>
