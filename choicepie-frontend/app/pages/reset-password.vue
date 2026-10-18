<template>
  <AuthCardLayout>
    <AuthStepIndicator
      :steps="[t('forgotPassword.step1'), t('forgotPassword.step2')]"
      :current="1"
    />
    <h1 class="text-xl font-extrabold text-center mb-1">
      {{ t('resetPassword.title') }}
    </h1>

    <p
      v-if="!token"
      class="text-sm text-error-500 text-center"
    >
      {{ t('resetPassword.missingToken') }}
    </p>

    <template v-else-if="!isSubmitted">
      <UForm
        :schema="resetPasswordSchema"
        :state="formState"
        class="flex flex-col gap-3.5"
        @submit="handleSubmit"
      >
        <UFormField name="password">
          <PasswordInput
            v-model="formState.password"
            v-model:visible="showPassword"
            :placeholder="t('resetPassword.passwordPlaceholder')"
          />
        </UFormField>
        <UFormField name="confirmPassword">
          <PasswordInput
            v-model="formState.confirmPassword"
            v-model:visible="showPassword"
            :show-toggle="false"
            :placeholder="t('resetPassword.confirmPasswordPlaceholder')"
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
          :loading="auth.isResetPasswordLoading"
        >
          {{ t('resetPassword.submitBtn') }}
        </UButton>
      </UForm>
    </template>

    <p
      v-else
      class="text-sm text-neutral-600 text-center"
    >
      {{ t('resetPassword.submitted') }}
    </p>

    <hr class="border-neutral-200">

    <NuxtLink
      to="/login"
      class="text-center text-sm text-neutral-600"
    >
      {{ t('resetPassword.backToLogin') }}
    </NuxtLink>
  </AuthCardLayout>
</template>

<script setup lang="ts">
import AuthCardLayout from '~/components/common/AuthCardLayout.vue'
import AuthStepIndicator from '~/components/common/AuthStepIndicator.vue'
import PasswordInput from '~/components/common/PasswordInput.vue'
import type { FormSubmitEvent } from '@nuxt/ui'
import { useResetPasswordSchema, type ResetPasswordSchema } from '~/types/auth'

definePageMeta({ layout: 'default' })

const { t } = useI18n()
const resetPasswordSchema = useResetPasswordSchema()
const auth = useAuthStore()
const route = useRoute()

const token = computed(() => (route.query.token as string) || '')
const error = ref('')
const isSubmitted = ref(false)
const showPassword = ref(false)

const formState = reactive<ResetPasswordSchema>({
  password: '',
  confirmPassword: ''
})

const handleSubmit = async (event: FormSubmitEvent<ResetPasswordSchema>) => {
  error.value = ''
  if (event.data && token.value) {
    try {
      await auth.resetPassword({ token: token.value, ...event.data })
      isSubmitted.value = true
    } catch {
      error.value = t('resetPassword.submitError')
    }
  }
}
</script>

<script lang="ts">
export default {
  name: 'ResetPasswordPage'
}
</script>

<style scoped lang="scss"></style>
