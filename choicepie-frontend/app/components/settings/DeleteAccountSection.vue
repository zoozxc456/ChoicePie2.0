<template>
  <SettingsSectionCard
    :title="t('settings.delete.title')"
    :description="t('settings.delete.description')"
    danger
  >
    <UButton
      color="error"
      variant="outline"
      size="lg"
      class="font-bold h-12 rounded-2xl self-start px-6"
      @click="openModal"
    >
      {{ t('settings.delete.button') }}
    </UButton>

    <DeleteAccountModal
      :open="isOpen"
      :has-password="accountStore.account?.hasPassword ?? false"
      :email="accountStore.account?.email ?? ''"
      :is-submitting="isSubmitting"
      :error="error"
      @confirm="handleConfirm"
      @cancel="isOpen = false"
    />
  </SettingsSectionCard>
</template>

<script setup lang="ts">
import SettingsSectionCard from '~/components/settings/SettingsSectionCard.vue'
import DeleteAccountModal from '~/components/settings/DeleteAccountModal.vue'
import type { DeleteAccountRequest } from '~/types/account'

const { t } = useI18n()
const toast = useToast()
const accountStore = useAccountStore()

const isOpen = ref(false)
const isSubmitting = ref(false)
const error = ref('')

const openModal = () => {
  error.value = ''
  isOpen.value = true
}

const handleConfirm = async (payload: DeleteAccountRequest) => {
  error.value = ''
  isSubmitting.value = true
  try {
    await accountStore.deleteAccount(payload)
    toast.add({ title: t('settings.delete.doneTitle'), description: t('settings.delete.doneDescription'), color: 'success' })
    await navigateTo('/')
  } catch (e) {
    error.value = getApiErrorMessage(e, t('settings.common.failed'))
  } finally {
    isSubmitting.value = false
  }
}
</script>

<style scoped lang="scss"></style>
