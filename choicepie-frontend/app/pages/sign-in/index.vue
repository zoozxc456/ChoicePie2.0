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
        {{ t('signIn.title') }}
      </h1>

      <!-- Google Register -->
      <UButton
        block
        size="lg"
        color="neutral"
        variant="outline"
        class="font-bold h-12 rounded-2xl"
        @click="auth.loginWithGoogle"
      >
        <template #leading>
          <span class="font-extrabold text-[#4285F4]">G</span>
        </template>
        {{ t('signIn.googleRegister') }}
      </UButton>

      <div class="flex items-center gap-3 text-xs text-neutral-400">
        <hr class="flex-1 border-neutral-200">
        {{ t('signIn.orDivider') }}
        <hr class="flex-1 border-neutral-200">
      </div>

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
          <UInput
            v-model="registerState.password"
            :type="showPassword ? 'text' : 'password'"
            :placeholder="t('signIn.passwordPlaceholder')"
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
          <div
            v-if="passwordStrength"
            class="flex items-center gap-2 mt-1.5"
          >
            <div class="flex gap-1 flex-1">
              <div
                v-for="i in 3"
                :key="i"
                class="h-1 flex-1 rounded-full transition-all"
                :class="i <= (['weak', 'medium', 'strong'].indexOf(passwordStrength.level) + 1)
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
          <UInput
            v-model="registerState.confirmPassword"
            :type="showConfirmPassword ? 'text' : 'password'"
            :placeholder="t('signIn.confirmPasswordPlaceholder')"
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
                :icon="showConfirmPassword ? 'i-lucide-eye-off' : 'i-lucide-eye'"
                @click="onToggleShowConfirmPassword"
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
          :color="isFormValid ? 'primary' : 'neutral'"
          class="font-bold h-12 rounded-2xl"
          :class="{ 'bg-[#e2e8f0]! text-neutral-400!': !isFormValid }"
          :disabled="!isFormValid"
          :loading="isLoading"
        >
          {{ t('signIn.submitBtn') }}
        </UButton>
      </UForm>

      <!-- Login link -->
      <NuxtLink
        :to="`/login${redirect !== '/library' ? `?redirect=${redirect}` : ''}`"
        class="text-center text-sm text-neutral-600"
      >
        {{ t('signIn.haveAccount') }} <span class="text-primary-500 font-semibold">{{ t('signIn.loginLink') }}</span>
      </NuxtLink>
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
const isLoading = ref(false)
const showPassword = ref(false)
const showConfirmPassword = ref(false)

const redirect = computed(() => (route.query.redirect as string) || '/library')

if (auth.isLoggedIn) {
  await navigateTo(redirect.value)
}

const registerSchema = z.object({
  name: z.string().min(2, t('signIn.validation.nameMin')),
  email: z.email(t('signIn.validation.emailInvalid')),
  password: z.string().min(8, t('signIn.validation.passwordMin')),
  confirmPassword: z.string()
}).refine(data => data.password === data.confirmPassword, {
  message: t('signIn.validation.confirmPasswordMatch'),
  path: ['confirmPassword']
})

type RegisterForm = {
  name: string
  email: string
  password: string
  confirmPassword: string
}

const registerState = reactive<RegisterForm>({
  name: '',
  email: '',
  password: '',
  confirmPassword: ''
})

const isFormValid = computed(() => registerSchema.safeParse(registerState).success)

const passwordStrength = computed(() => {
  const p = registerState.password
  if (p.length === 0) return null
  if (p.length < 8) return { level: 'weak', label: t('signIn.strength.tooShort'), barClass: 'bg-error-500', textClass: 'text-error-500' }
  const hasUpper = /[A-Z]/.test(p)
  const hasNum = /[0-9]/.test(p)
  const hasSymbol = /[^A-Za-z0-9]/.test(p)
  const score = [hasUpper, hasNum, hasSymbol].filter(Boolean).length
  if (score === 0) return { level: 'weak', label: t('signIn.strength.weak'), barClass: 'bg-error-500', textClass: 'text-error-500' }
  if (score === 1) return { level: 'medium', label: t('signIn.strength.medium'), barClass: 'bg-warning-500', textClass: 'text-warning-500' }
  return { level: 'strong', label: t('signIn.strength.strong'), barClass: 'bg-success-500', textClass: 'text-success-500' }
})

const handleRegister = async () => {
  isLoading.value = true
  error.value = ''
  try {
    await $fetch('/api/auth/register', {
      method: 'POST',
      body: { name: registerState.name.trim(), email: registerState.email, password: registerState.password }
    })
    await auth.loginWithEmail(registerState.email, registerState.password)
    await navigateTo(redirect.value)
  } catch (e: unknown) {
    const msg = (e as { data?: { message?: string } })?.data?.message
    error.value = msg ?? t('signIn.registerError')
  } finally {
    isLoading.value = false
  }
}

const onToggleShowPassword = () => {
  showPassword.value = !showPassword.value
}

const onToggleShowConfirmPassword = () => {
  showConfirmPassword.value = !showConfirmPassword.value
}
</script>

<script lang="ts">
export default {
  name: 'SignInPage'
}
</script>

<style scoped lang="scss"></style>
