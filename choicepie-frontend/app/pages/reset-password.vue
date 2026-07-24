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
            <UInput
              v-model="formState.password"
              :type="showPassword ? 'text' : 'password'"
              :placeholder="t('resetPassword.passwordPlaceholder')"
              size="lg"
              class="w-full"
              :ui="{ base: 'bg-neutral-100 h-12 text-sm px-4' }"
            >
              <template #trailing>
                <UButton
                  color="neutral"
                  variant="link"
                  size="sm"
                  :padded="false"
                  :icon="showPassword ? 'i-lucide-eye-off' : 'i-lucide-eye'"
                  @click="showPassword = !showPassword"
                />
              </template>
            </UInput>
          </UFormField>
          <UFormField name="confirmPassword">
            <UInput
              v-model="formState.confirmPassword"
              :type="showPassword ? 'text' : 'password'"
              :placeholder="t('resetPassword.confirmPasswordPlaceholder')"
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
    </div>
  </div>
</template>

<script setup lang="ts">
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
