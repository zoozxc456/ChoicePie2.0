<template>
  <SettingsSectionCard
    :title="hasPassword ? t('settings.password.changeTitle') : t('settings.password.setTitle')"
    :description="hasPassword ? t('settings.password.changeDescription') : t('settings.password.setDescription')"
  >
    <form
      class="flex flex-col gap-3.5"
      @submit.prevent="handleSubmit"
    >
      <UFormField
        v-if="hasPassword"
        :label="t('settings.password.current')"
        :error="errors.current || undefined"
      >
        <UInput
          v-model="form.current"
          type="password"
          autocomplete="current-password"
          size="lg"
          class="w-full"
          :ui="{ base: 'bg-neutral-100 h-12 text-sm px-4' }"
        />
      </UFormField>

      <UFormField
        :label="t('settings.password.new')"
        :error="errors.next || undefined"
      >
        <UInput
          v-model="form.next"
          type="password"
          autocomplete="new-password"
          size="lg"
          class="w-full"
          :ui="{ base: 'bg-neutral-100 h-12 text-sm px-4' }"
        />
      </UFormField>

      <UFormField
        :label="t('settings.password.confirm')"
        :error="errors.confirm || undefined"
      >
        <UInput
          v-model="form.confirm"
          type="password"
          autocomplete="new-password"
          size="lg"
          class="w-full"
          :ui="{ base: 'bg-neutral-100 h-12 text-sm px-4' }"
        />
      </UFormField>

      <p
        v-if="submitError"
        class="text-sm text-error-500"
      >
        {{ submitError }}
      </p>
      <p
        v-if="done"
        class="text-sm text-success-600"
      >
        {{ hasPassword ? t('settings.password.changed') : t('settings.password.setDone') }}
      </p>
      <p class="text-xs text-neutral-500">
        {{ t('settings.password.otherSessionsNote') }}
      </p>

      <UButton
        type="submit"
        size="lg"
        color="primary"
        class="font-bold h-12 rounded-2xl self-start px-6"
        :loading="isSubmitting"
      >
        {{ hasPassword ? t('settings.password.changeSubmit') : t('settings.password.setSubmit') }}
      </UButton>
    </form>
  </SettingsSectionCard>
</template>

<script setup lang="ts">
import SettingsSectionCard from '~/components/settings/SettingsSectionCard.vue'

const PASSWORD_MIN = 8

const { t } = useI18n()
const accountStore = useAccountStore()

const hasPassword = computed(() => accountStore.account?.hasPassword ?? false)

const form = reactive({ current: '', next: '', confirm: '' })
const errors = reactive({ current: '', next: '', confirm: '' })
const submitError = ref('')
const done = ref(false)
const isSubmitting = ref(false)

const validate = () => {
  errors.current = hasPassword.value && !form.current ? t('settings.password.currentRequired') : ''
  errors.next = form.next.length < PASSWORD_MIN ? t('settings.password.min', { min: PASSWORD_MIN }) : ''
  errors.confirm = form.confirm !== form.next ? t('settings.password.mismatch') : ''
  return !errors.current && !errors.next && !errors.confirm
}

const handleSubmit = async () => {
  done.value = false
  submitError.value = ''
  if (!validate()) return

  isSubmitting.value = true
  try {
    await accountStore.changePassword({
      currentPassword: hasPassword.value ? form.current : undefined,
      newPassword: form.next
    })
    form.current = ''
    form.next = ''
    form.confirm = ''
    done.value = true
  } catch (e) {
    submitError.value = getApiErrorMessage(e, t('settings.common.failed'))
  } finally {
    isSubmitting.value = false
  }
}
</script>

<style scoped lang="scss"></style>
