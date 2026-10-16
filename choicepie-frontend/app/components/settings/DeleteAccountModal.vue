<template>
  <Transition name="fade">
    <div
      v-if="open"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/50"
      @click.self="emit('cancel')"
    >
      <form
        class="rounded-2xl bg-white w-full max-w-md mx-4 overflow-hidden shadow-cp-xl"
        @submit.prevent="handleConfirm"
      >
        <div class="px-6 pt-6 pb-4 flex flex-col gap-3">
          <h2 class="text-xl font-bold text-error-600">
            {{ t('settings.delete.modalTitle') }}
          </h2>
          <p class="text-sm text-neutral-600">
            {{ t('settings.delete.warning') }}
          </p>

          <UFormField
            v-if="hasPassword"
            :label="t('settings.delete.passwordLabel')"
          >
            <UInput
              v-model="password"
              type="password"
              autocomplete="current-password"
              size="lg"
              class="w-full"
              :ui="{ base: 'bg-neutral-100 h-12 text-sm px-4' }"
            />
          </UFormField>
          <UFormField
            v-else
            :label="t('settings.delete.emailLabel', { email })"
          >
            <UInput
              v-model="confirmEmail"
              type="email"
              size="lg"
              class="w-full"
              :placeholder="email"
              :ui="{ base: 'bg-neutral-100 h-12 text-sm px-4' }"
            />
          </UFormField>

          <p
            v-if="error"
            class="text-sm text-error-500"
          >
            {{ error }}
          </p>
        </div>
        <div class="flex gap-3 px-6 pb-6">
          <UButton
            block
            color="neutral"
            variant="ghost"
            @click="emit('cancel')"
          >
            {{ t('settings.delete.cancel') }}
          </UButton>
          <UButton
            type="submit"
            block
            color="error"
            :loading="isSubmitting"
            :disabled="!canSubmit"
          >
            {{ t('settings.delete.confirm') }}
          </UButton>
        </div>
      </form>
    </div>
  </Transition>
</template>

<script setup lang="ts">
import type { DeleteAccountRequest } from '~/types/account'

interface Props {
  open: boolean
  hasPassword: boolean
  email: string
  isSubmitting: boolean
  error: string
}

interface Emits {
  (e: 'confirm', payload: DeleteAccountRequest): void
  (e: 'cancel'): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const { t } = useI18n()

const password = ref('')
const confirmEmail = ref('')

const canSubmit = computed(() => props.hasPassword ? password.value.length > 0 : confirmEmail.value.trim().length > 0)

watch(() => props.open, (isOpen) => {
  if (isOpen) {
    password.value = ''
    confirmEmail.value = ''
  }
})

const handleConfirm = () => {
  emit('confirm', props.hasPassword
    ? { password: password.value }
    : { confirmEmail: confirmEmail.value.trim() })
}
</script>

<style scoped lang="scss"></style>
