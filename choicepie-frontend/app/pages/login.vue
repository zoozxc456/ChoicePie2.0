<template>
  <div class="flex items-center justify-center min-h-[calc(100vh-56px)] px-4">
    <div class="w-full max-w-sm">
      <!-- Logo -->
      <div class="text-center mb-8">
        <div class="text-6xl mb-3">
          🥧
        </div>
        <h1 class="text-2xl font-bold mb-1">
          {{ t('login.title') }}
        </h1>
        <p style="color: var(--cp-text-secondary); font-size: 14px;">
          {{ t('login.subtitle') }}
        </p>
      </div>

      <!-- Card -->
      <div
        class="rounded-2xl p-6 bg-white"
        style="border: 1px solid var(--cp-border); box-shadow: var(--cp-shadow-md);"
      >
        <!-- Google Login -->
        <UButton
          block
          size="lg"
          color="neutral"
          variant="outline"
          class="mb-3"
          :loading="auth.isLoading"
          @click="auth.loginWithGoogle"
        >
          <template #leading>
            <span class="font-bold text-base">G</span>
          </template>
          {{ t('login.googleLogin') }}
        </UButton>

        <div class="flex items-center gap-3 my-4">
          <hr
            class="flex-1"
            style="border-color: var(--cp-border);"
          >
          <span
            class="text-xs"
            style="color: var(--cp-text-muted);"
          >{{ t('login.orDivider') }}</span>
          <hr
            class="flex-1"
            style="border-color: var(--cp-border);"
          >
        </div>

        <!-- Email Login -->
        <UForm
          :schema="loginSchema"
          :state="loginState"
          class="flex flex-col gap-3"
          @submit="handleEmailLogin"
        >
          <UFormField name="email">
            <UInput
              v-model="loginState.email"
              type="email"
              :placeholder="t('login.emailPlaceholder')"
              size="lg"
              class="w-full"
            />
          </UFormField>
          <UFormField name="password">
            <UInput
              v-model="loginState.password"
              type="password"
              :placeholder="t('login.passwordPlaceholder')"
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
            :loading="auth.isLoading"
          >
            {{ t('login.loginBtn') }}
          </UButton>
        </UForm>

        <!-- Guest hint -->
        <hr
          class="my-4"
          style="border-color: var(--cp-border);"
        >
        <p
          class="text-center text-sm"
          style="color: var(--cp-text-muted);"
        >
          {{ t('login.guestHint') }}
          <NuxtLink
            to="/join"
            class="font-semibold"
            style="color: var(--cp-primary);"
          >
            {{ t('login.noLoginNeeded') }}
          </NuxtLink>
        </p>
      </div>

      <!-- Host benefits -->
      <div
        class="mt-4 rounded-xl p-4 bg-white"
        style="border: 1px solid var(--cp-border);"
      >
        <p
          class="text-xs font-semibold mb-3"
          style="color: var(--cp-text-muted);"
        >
          {{ t('login.hostBenefits.title') }}
        </p>
        <ul class="flex flex-col gap-2">
          <li
            v-for="item in hostBenefits"
            :key="item"
            class="flex items-center gap-2 text-sm"
          >
            <span style="color: var(--cp-success);">✓</span> {{ item }}
          </li>
        </ul>
      </div>
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

const redirect = computed(() => (route.query.redirect as string) || '/library')

const hostBenefits = computed(() => [
  t('login.hostBenefits.aiGenerate'),
  t('login.hostBenefits.createRoom'),
  t('login.hostBenefits.viewHistory'),
  t('login.hostBenefits.reuseLibrary')
])

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
</script>

<script lang="ts">
export default {
  name: 'LoginPage'
}
</script>

<style scoped lang="scss">
</style>
