<template>
  <div class="min-h-[calc(100vh-56px)] bg-neutral-50 flex flex-col items-center px-6 py-16">
    <NuxtLink
      to="/"
      class="text-6xl mb-6"
    >
      🥧
    </NuxtLink>

    <div class="w-full max-w-md rounded-2xl bg-white shadow-cp-lg p-7 flex flex-col gap-3.5">
      <h1 class="text-xl font-extrabold text-center mb-1">
        {{ t('forgotPassword.title') }}
      </h1>
      <p class="text-sm text-neutral-500 text-center mb-1">
        {{ t('forgotPassword.description') }}
      </p>

      <UForm
        v-if="!isSubmitted"
        :schema="forgotPasswordSchema"
        :state="formState"
        class="flex flex-col gap-3.5"
        @submit="handleSubmit"
      >
        <UFormField name="email">
          <UInput
            v-model="formState.email"
            type="email"
            :placeholder="t('forgotPassword.emailPlaceholder')"
            size="lg"
            class="w-full"
            :ui="{ base: 'bg-neutral-100 h-12 text-sm px-4' }"
          />
        </UFormField>
        <p
          v-if="error"
          class="text-sm text-error-500"
        >
          {{ error }}
        </p>
        <UButton
          type="submit"
          block
          size="lg"
          color="primary"
          class="font-bold h-12 rounded-2xl"
          :loading="auth.isForgotPasswordLoading"
        >
          {{ t('forgotPassword.submitBtn') }}
        </UButton>
      </UForm>

      <p
        v-else
        class="text-sm text-neutral-600 text-center"
      >
        {{ t('forgotPassword.submitted') }}
      </p>

      <hr class="border-neutral-200">

      <NuxtLink
        to="/login"
        class="text-center text-sm text-neutral-600"
      >
        {{ t('forgotPassword.backToLogin') }}
      </NuxtLink>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { FormSubmitEvent } from '@nuxt/ui'
import { useForgotPasswordSchema, type ForgotPasswordSchema } from '~/types/auth'

definePageMeta({ layout: 'default' })

const { t } = useI18n()
const forgotPasswordSchema = useForgotPasswordSchema()
const auth = useAuthStore()

const error = ref('')
const isSubmitted = ref(false)

const formState = reactive<ForgotPasswordSchema>({
  email: ''
})

const handleSubmit = async (event: FormSubmitEvent<ForgotPasswordSchema>) => {
  error.value = ''
  if (event.data) {
    try {
      await auth.forgotPassword(formState)
      isSubmitted.value = true
    } catch {
      error.value = t('forgotPassword.submitError')
    }
  }
}
</script>

<script lang="ts">
export default {
  name: 'ForgotPasswordPage'
}
</script>

<style scoped lang="scss"></style>
