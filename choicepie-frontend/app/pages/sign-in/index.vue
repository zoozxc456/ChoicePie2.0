<template>
  <div class="flex items-center justify-center min-h-[calc(100vh-56px)] px-4 py-8">
    <div class="w-full max-w-sm">
      <!-- Logo -->
      <div class="text-center mb-8">
        <div class="text-6xl mb-3">
          🥧
        </div>
        <h1 class="text-2xl font-bold mb-1">
          {{ t('signIn.title') }}
        </h1>
        <p style="color: var(--cp-text-secondary); font-size: 14px;">
          {{ t('signIn.subtitle') }}
        </p>
      </div>

      <!-- Card -->
      <div
        class="rounded-2xl p-6 bg-white mb-4"
        style="border: 1px solid var(--cp-border); box-shadow: var(--cp-shadow-md);"
      >
        <!-- Google Register -->
        <UButton
          block
          size="lg"
          color="neutral"
          variant="outline"
          class="mb-4"
          @click="auth.loginWithGoogle"
        >
          <template #leading>
            <span class="font-bold text-base">G</span>
          </template>
          {{ t('signIn.googleRegister') }}
        </UButton>

        <div class="flex items-center gap-3 mb-4">
          <hr
            class="flex-1"
            style="border-color: var(--cp-border);"
          >
          <span
            class="text-xs"
            style="color: var(--cp-text-muted);"
          >{{ t('signIn.orDivider') }}</span>
          <hr
            class="flex-1"
            style="border-color: var(--cp-border);"
          >
        </div>

        <!-- Form -->
        <UForm
          :schema="registerSchema"
          :state="registerState"
          class="flex flex-col gap-3"
          @submit="handleRegister"
        >
          <UFormField
            name="name"
            :label="t('signIn.fields.name')"
          >
            <UInput
              v-model="registerState.name"
              :placeholder="t('signIn.namePlaceholder')"
              size="lg"
              maxlength="20"
              class="w-full"
            />
          </UFormField>

          <UFormField
            name="email"
            :label="t('signIn.fields.email')"
          >
            <UInput
              v-model="registerState.email"
              type="email"
              :placeholder="t('signIn.emailPlaceholder')"
              size="lg"
              class="w-full"
            />
          </UFormField>

          <UFormField
            name="password"
            :label="t('signIn.fields.password')"
          >
            <UInput
              v-model="registerState.password"
              type="password"
              :placeholder="t('signIn.passwordPlaceholder')"
              size="lg"
              class="w-full"
            />
            <div
              v-if="passwordStrength"
              class="flex items-center gap-2 mt-1.5"
            >
              <div class="flex gap-1">
                <div
                  v-for="i in 3"
                  :key="i"
                  class="h-1 w-8 rounded-full transition-all"
                  :style="`background: ${i <= (['weak', 'medium', 'strong'].indexOf(passwordStrength.level) + 1) ? passwordStrength.color : 'var(--cp-border)'};`"
                />
              </div>
              <span
                class="text-xs font-medium"
                :style="`color: ${passwordStrength.color};`"
              >
                {{ passwordStrength.label }}
              </span>
            </div>
          </UFormField>

          <UFormField
            name="confirmPassword"
            :label="t('signIn.fields.confirmPassword')"
          >
            <UInput
              v-model="registerState.confirmPassword"
              type="password"
              :placeholder="t('signIn.confirmPasswordPlaceholder')"
              size="lg"
              class="w-full"
            />
          </UFormField>

          <p
            v-if="error"
            class="text-sm"
            style="color: var(--cp-danger);"
          >
            {{ error }}
          </p>

          <UButton
            type="submit"
            block
            size="lg"
            color="primary"
            :loading="isLoading"
            class="mt-1"
          >
            {{ t('signIn.submitBtn') }}
          </UButton>
        </UForm>
      </div>

      <!-- Login link -->
      <p
        class="text-center text-sm"
        style="color: var(--cp-text-muted);"
      >
        {{ t('signIn.haveAccount') }}
        <NuxtLink
          :to="`/login${redirect !== '/library' ? `?redirect=${redirect}` : ''}`"
          class="font-semibold"
          style="color: var(--cp-primary);"
        >
          {{ t('signIn.loginLink') }}
        </NuxtLink>
      </p>
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

const passwordStrength = computed(() => {
  const p = registerState.password
  if (p.length === 0) return null
  if (p.length < 8) return { level: 'weak', label: t('signIn.strength.tooShort'), color: 'var(--cp-danger)' }
  const hasUpper = /[A-Z]/.test(p)
  const hasNum = /[0-9]/.test(p)
  const hasSymbol = /[^A-Za-z0-9]/.test(p)
  const score = [hasUpper, hasNum, hasSymbol].filter(Boolean).length
  if (score === 0) return { level: 'weak', label: t('signIn.strength.weak'), color: 'var(--cp-danger)' }
  if (score === 1) return { level: 'medium', label: t('signIn.strength.medium'), color: 'var(--cp-warning)' }
  return { level: 'strong', label: t('signIn.strength.strong'), color: 'var(--cp-success)' }
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
</script>

<script lang="ts">
export default {
  name: 'SignInPage'
}
</script>

<style scoped lang="scss">
</style>
