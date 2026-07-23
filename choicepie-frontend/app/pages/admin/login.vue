<template>
  <div class="min-h-[calc(100vh-56px)] flex flex-col items-center px-6 py-16">
    <NuxtLink
      to="/admin"
      class="text-6xl mb-6"
    >
      🥧
    </NuxtLink>

    <div class="w-full max-w-md rounded-2xl bg-white shadow-cp-lg p-7 flex flex-col gap-3.5">
      <h1 class="text-xl font-extrabold text-center mb-1">
        {{ t('adminLogin.title') }}
      </h1>

      <UForm
        :schema="adminLoginSchema"
        :state="loginState"
        class="flex flex-col gap-3.5"
        @submit="handleLogin"
      >
        <UFormField name="email">
          <UInput
            v-model="loginState.email"
            type="email"
            :placeholder="t('adminLogin.emailPlaceholder')"
            size="lg"
            class="w-full"
            :ui="{ base: 'bg-neutral-100 h-12 text-sm px-4' }"
          />
        </UFormField>
        <UFormField name="password">
          <UInput
            v-model="loginState.password"
            :type="showPassword ? 'text' : 'password'"
            :placeholder="t('adminLogin.passwordPlaceholder')"
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
          :loading="adminAuth.isLoading"
        >
          {{ t('adminLogin.loginBtn') }}
        </UButton>
      </UForm>
    </div>
  </div>
</template>

<script setup lang="ts">
import type { FormSubmitEvent } from '@nuxt/ui'
import { ApiError } from '~/composables/useApi'
import { useAdminLoginSchema, type AdminLoginSchema } from '~/types/adminAuth'

definePageMeta({ layout: 'admin' })

const { t } = useI18n()
const adminLoginSchema = useAdminLoginSchema()
const adminAuth = useAdminAuthStore()
const route = useRoute()

const error = ref('')
const showPassword = ref(false)

const redirect = computed(() => (route.query.redirect as string) || '/admin')

if (adminAuth.isLoggedIn) {
  await navigateTo(redirect.value)
}

const loginState = reactive<AdminLoginSchema>({
  email: '',
  password: ''
})

const handleLogin = async (event: FormSubmitEvent<AdminLoginSchema>) => {
  error.value = ''
  if (event.data) {
    try {
      await adminAuth.loginWithEmail(loginState)
      await navigateTo(redirect.value)
    } catch (e: unknown) {
      error.value = e instanceof ApiError ? e.message : t('adminLogin.loginError')
    }
  }
}

const onToggleShowPassword = () => {
  showPassword.value = !showPassword.value
}
</script>

<script lang="ts">
export default {
  name: 'AdminLoginPage'
}
</script>

<style scoped lang="scss"></style>
