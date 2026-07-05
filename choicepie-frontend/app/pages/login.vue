<template>
  <div class="min-h-[calc(100vh-56px)] bg-neutral-50 flex flex-col items-center px-6 py-16">
    <!-- Logo -->
    <NuxtLink
      to="/"
      class="text-6xl mb-6"
    >
      🥧
    </NuxtLink>

    <!-- Card -->
    <div class="w-full max-w-md rounded-2xl bg-white shadow-cp-lg p-7 flex flex-col gap-3.5">
      <h1 class="text-xl font-extrabold text-center mb-1">
        {{ t('login.title') }}
      </h1>

      <!-- Google Login -->
      <UButton
        block
        size="lg"
        color="neutral"
        variant="outline"
        class="font-bold h-12 rounded-2xl"
        :loading="auth.isLoading"
        @click="auth.loginWithGoogle"
      >
        <template #leading>
          <span class="font-extrabold text-[#4285F4]">G</span>
        </template>
        {{ t('login.googleLogin') }}
      </UButton>

      <div class="flex items-center gap-3 text-xs text-neutral-400">
        <hr class="flex-1 border-neutral-200">
        {{ t('login.orDivider') }}
        <hr class="flex-1 border-neutral-200">
      </div>

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
          <UInput
            v-model="loginState.password"
            :type="showPassword ? 'text' : 'password'"
            :placeholder="t('login.passwordPlaceholder')"
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
                @click="onToggleShowPassword"
              />
            </template>
          </UInput>
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
        to="/sign-in"
        class="text-center text-sm text-neutral-600"
      >
        {{ t('login.noAccount') }} <span class="text-primary-500 font-semibold">{{ t('login.registerLink') }}</span>
      </NuxtLink>
    </div>

    <!-- Host benefits -->
    <div class="w-full max-w-md mt-5 rounded-2xl bg-white border border-neutral-200 p-4 text-sm text-neutral-600 leading-relaxed">
      <p class="font-bold text-neutral-900 mb-1">
        {{ t('login.hostBenefits.title') }}
      </p>
      {{ t('login.hostBenefits.desc') }}
    </div>
  </div>
</template>

<script setup lang="ts">
import * as z from 'zod'

definePageMeta({ layout: 'default' })

const { t } = useI18n()
const auth = useAuthStore()
const route = useRoute()

const error = ref('')
const showPassword = ref(false)

const redirect = computed(() => (route.query.redirect as string) || '/library')

if (auth.isLoggedIn) {
  await navigateTo(redirect.value)
}

const loginSchema = z.object({
  email: z.email(t('login.validation.emailInvalid')),
  password: z.string().min(6, t('login.validation.passwordMin'))
})

type LoginForm = z.infer<typeof loginSchema>

const loginState = reactive<LoginForm>({
  email: '',
  password: ''
})

const handleEmailLogin = async () => {
  error.value = ''
  try {
    await auth.loginWithEmail(loginState.email, loginState.password)
    await navigateTo(redirect.value)
  } catch {
    error.value = t('login.loginError')
  }
}

const onToggleShowPassword = () => {
  showPassword.value = !showPassword.value
}
</script>

<script lang="ts">
export default {
  name: 'LoginPage'
}
</script>

<style scoped lang="scss">
</style>
