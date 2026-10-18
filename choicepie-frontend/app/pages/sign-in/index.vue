<template>
  <AuthCardLayout>
    <h1 class="text-xl font-extrabold text-center mb-1">
      {{ t('signIn.title') }}
    </h1>

    <!-- Google Register -->
    <GoogleAuthButton
      :label="t('signIn.googleRegister')"
      :loading="auth.isLoading"
      @click="handleGoogleLogin"
    />

    <OrDivider :label="t('signIn.orDivider')" />

    <!-- Form -->
    <UForm
      :schema="registerSchema"
      :state="registerState"
      class="flex flex-col gap-3.5"
      @submit="handleRegister"
    >
      <UFormField name="name">
        <UInput
          v-model="registerState.name"
          :placeholder="t('signIn.namePlaceholder')"
          size="lg"
          maxlength="20"
          class="w-full"
          :ui="{ base: 'bg-neutral-100 h-12 text-sm px-4' }"
        />
      </UFormField>

      <UFormField name="email">
        <UInput
          v-model="registerState.email"
          type="email"
          :placeholder="t('signIn.emailPlaceholder')"
          size="lg"
          class="w-full"
          :ui="{ base: 'bg-neutral-100 h-12 text-sm px-4' }"
        />
      </UFormField>

      <UFormField name="password">
        <PasswordInput
          v-model="registerState.password"
          :placeholder="t('signIn.passwordPlaceholder')"
        />
        <div
          v-if="passwordStrength"
          class="flex items-center gap-2 mt-1.5"
        >
          <div class="flex gap-1 flex-1">
            <div
              v-for="i in 3"
              :key="i"
              class="h-1 flex-1 rounded-full transition-all"
              :class="i <= passwordStrength.bars
                ? passwordStrength.barClass
                : 'bg-neutral-200'"
            />
          </div>
          <span
            class="text-xs font-medium"
            :class="passwordStrength.textClass"
          >
            {{ passwordStrength.label }}
          </span>
        </div>
      </UFormField>

      <UFormField name="confirmPassword">
        <PasswordInput
          v-model="registerState.confirmPassword"
          :placeholder="t('signIn.confirmPasswordPlaceholder')"
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
        :color="isFormValid ? 'primary' : 'neutral'"
        class="font-bold h-12 rounded-2xl"
        :class="{ 'bg-cp-disabled! text-neutral-400!': !isFormValid }"
        :disabled="!isFormValid"
        :loading="isLoading"
      >
        {{ t('signIn.submitBtn') }}
      </UButton>
    </UForm>

    <!-- Login link -->
    <NuxtLink
      :to="`/login${redirect !== '/library' ? `?redirect=${encodeURIComponent(redirect)}` : ''}`"
      class="text-center text-sm text-neutral-600"
    >
      {{ t('signIn.haveAccount') }} <span class="text-primary-500 font-semibold">{{ t('signIn.loginLink') }}</span>
    </NuxtLink>
  </AuthCardLayout>
</template>

<script setup lang="ts">
import AuthCardLayout from '~/components/common/AuthCardLayout.vue'
import GoogleAuthButton from '~/components/common/GoogleAuthButton.vue'
import OrDivider from '~/components/common/OrDivider.vue'
import PasswordInput from '~/components/common/PasswordInput.vue'
import { useAuthRedirect } from '~/composables/useAuthRedirect'
import { useRegisterSchema, type RegisterSchema } from '~/types/auth'
import { getApiErrorMessage } from '~/utils/apiErrorMessage'
import { evaluatePasswordStrength, type PasswordStrength } from '~/utils/passwordStrength'

definePageMeta({ layout: 'default' })

const { t } = useI18n()
const registerSchema = useRegisterSchema()
const auth = useAuthStore()

const error = ref('')
const isLoading = ref(false)

const { redirect, goToRedirect, redirectIfAuthenticated } = useAuthRedirect()

await redirectIfAuthenticated()

const registerState = reactive<RegisterSchema>({
  name: '',
  email: '',
  password: '',
  confirmPassword: ''
})

const isFormValid = computed(() => registerSchema.safeParse(registerState).success)

const STRENGTH_DISPLAY: Record<PasswordStrength, { bars: number, barClass: string, textClass: string }> = {
  tooShort: { bars: 1, barClass: 'bg-error-500', textClass: 'text-error-500' },
  weak: { bars: 1, barClass: 'bg-error-500', textClass: 'text-error-500' },
  medium: { bars: 2, barClass: 'bg-warning-500', textClass: 'text-warning-500' },
  strong: { bars: 3, barClass: 'bg-success-500', textClass: 'text-success-500' }
}

const passwordStrength = computed(() => {
  const strength = evaluatePasswordStrength(registerState.password)
  if (!strength) return null
  return { ...STRENGTH_DISPLAY[strength], label: t(`signIn.strength.${strength}`) }
})

const handleRegister = async () => {
  isLoading.value = true
  error.value = ''
  try {
    await auth.register({
      name: registerState.name.trim(),
      email: registerState.email,
      password: registerState.password,
      confirmPassword: registerState.confirmPassword
    })
    await goToRedirect()
  } catch (e: unknown) {
    error.value = getApiErrorMessage(e, t('signIn.registerError'))
  } finally {
    isLoading.value = false
  }
}

const handleGoogleLogin = async () => {
  error.value = ''
  try {
    await auth.loginWithGoogle()
    await goToRedirect()
  } catch (e: unknown) {
    error.value = getApiErrorMessage(e, t('signIn.googleLoginError'))
  }
}
</script>

<script lang="ts">
export default {
  name: 'SignInPage'
}
</script>

<style scoped lang="scss"></style>
