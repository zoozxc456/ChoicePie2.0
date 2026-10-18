<template>
  <AuthCardLayout>
    <h1 class="text-xl font-extrabold text-center mb-1">
      {{ t('login.title') }}
    </h1>

    <!-- Google Login -->
    <GoogleAuthButton
      :label="t('login.googleLogin')"
      :loading="auth.isLoading"
      @click="handleGoogleLogin"
    />

    <OrDivider :label="t('login.orDivider')" />

    <!-- Email Login -->
    <UForm
      :schema="loginSchema"
      :state="loginState"
      class="flex flex-col gap-3.5"
      @submit="handleEmailLogin"
    >
      <UFormField name="email">
        <UInput
          v-model="loginState.email"
          type="email"
          :placeholder="t('login.emailPlaceholder')"
          size="lg"
          class="w-full"
          :ui="{ base: 'bg-neutral-100 h-12 text-sm px-4' }"
        />
      </UFormField>
      <UFormField name="password">
        <PasswordInput
          v-model="loginState.password"
          :placeholder="t('login.passwordPlaceholder')"
        />
      </UFormField>
      <NuxtLink
        to="/forgot-password"
        class="text-xs text-neutral-500 text-right -mt-1"
      >
        {{ t('login.forgotPassword') }}
      </NuxtLink>
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
        :loading="auth.isLoading"
      >
        {{ t('login.loginBtn') }}
      </UButton>
    </UForm>

    <hr class="border-neutral-200">

    <NuxtLink
      to="/join"
      class="text-center text-sm text-neutral-600"
    >
      {{ t('login.guestHint') }} {{ t('login.noLoginNeeded') }}
    </NuxtLink>
    <NuxtLink
      :to="redirect !== '/library' ? `/sign-in?redirect=${encodeURIComponent(redirect)}` : '/sign-in'"
      class="text-center text-sm text-neutral-600"
    >
      {{ t('login.noAccount') }} <span class="text-primary-500 font-semibold">{{ t('login.registerLink') }}</span>
    </NuxtLink>

    <template #below>
      <!-- Host benefits -->
      <div
        class="w-full max-w-md mt-5 rounded-2xl bg-white border border-neutral-200 p-4 text-sm text-neutral-600 leading-relaxed"
      >
        <p class="font-bold text-neutral-900 mb-1">
          {{ t('login.hostBenefits.title') }}
        </p>
        {{ t('login.hostBenefits.desc') }}
      </div>
    </template>
  </AuthCardLayout>
</template>

<script setup lang="ts">
import AuthCardLayout from '~/components/common/AuthCardLayout.vue'
import GoogleAuthButton from '~/components/common/GoogleAuthButton.vue'
import OrDivider from '~/components/common/OrDivider.vue'
import PasswordInput from '~/components/common/PasswordInput.vue'
import type { FormSubmitEvent } from '@nuxt/ui'
import { useAuthRedirect } from '~/composables/useAuthRedirect'
import { useLoginSchema, type LoginSchema } from '~/types/auth'
import { getApiErrorMessage } from '~/utils/apiErrorMessage'

definePageMeta({ layout: 'default' })

const { t } = useI18n()
const loginSchema = useLoginSchema()
const auth = useAuthStore()

const error = ref('')

const { redirect, goToRedirect, redirectIfAuthenticated } = useAuthRedirect()

await redirectIfAuthenticated()

const loginState = reactive<LoginSchema>({
  email: '',
  password: ''
})

const handleEmailLogin = async (event: FormSubmitEvent<LoginSchema>) => {
  error.value = ''
  if (event.data) {
    try {
      await auth.loginWithEmail(loginState)
      await goToRedirect()
    } catch (e: unknown) {
      error.value = getApiErrorMessage(e, t('login.loginError'))
    }
  }
}

const handleGoogleLogin = async () => {
  error.value = ''
  try {
    await auth.loginWithGoogle()
    await goToRedirect()
  } catch (e: unknown) {
    error.value = getApiErrorMessage(e, t('login.googleLoginError'))
  }
}
</script>

<script lang="ts">
export default {
  name: 'LoginPage'
}
</script>

<style scoped lang="scss"></style>
