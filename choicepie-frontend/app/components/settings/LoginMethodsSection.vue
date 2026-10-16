<template>
  <SettingsSectionCard
    :title="t('settings.loginMethods.title')"
    :description="t('settings.loginMethods.description')"
  >
    <ul class="flex flex-col gap-3">
      <li class="flex items-center gap-3 rounded-xl bg-neutral-50 px-4 py-3">
        <UIcon
          name="i-lucide-key-round"
          class="w-5 h-5 text-neutral-600"
        />
        <span class="flex-1 text-sm font-semibold">{{ t('settings.loginMethods.password') }}</span>
        <span
          class="text-xs font-semibold"
          :class="account?.hasPassword ? 'text-success-600' : 'text-neutral-500'"
        >
          {{ account?.hasPassword ? t('settings.loginMethods.enabled') : t('settings.loginMethods.notSet') }}
        </span>
      </li>

      <li class="flex items-center gap-3 rounded-xl bg-neutral-50 px-4 py-3">
        <UIcon
          name="i-lucide-globe"
          class="w-5 h-5 text-neutral-600"
        />
        <span class="flex-1 text-sm font-semibold">Google</span>
        <UButton
          v-if="account?.googleLinked"
          color="neutral"
          variant="outline"
          size="sm"
          :disabled="!canUnlink"
          :loading="isBusy"
          @click="handleUnlink"
        >
          {{ t('settings.loginMethods.unlink') }}
        </UButton>
        <UButton
          v-else
          color="primary"
          size="sm"
          :loading="isBusy"
          @click="handleLink"
        >
          {{ t('settings.loginMethods.link') }}
        </UButton>
      </li>
    </ul>

    <p
      v-if="account?.googleLinked && !canUnlink"
      class="text-xs text-neutral-500"
    >
      {{ t('settings.loginMethods.lastMethodHint') }}
    </p>
    <p
      v-if="error"
      class="text-sm text-error-500"
    >
      {{ error }}
    </p>
  </SettingsSectionCard>
</template>

<script setup lang="ts">
import SettingsSectionCard from '~/components/settings/SettingsSectionCard.vue'

const { t } = useI18n()
const accountStore = useAccountStore()

const account = computed(() => accountStore.account)
// 與後端不變條件一致：Google 是唯一登入方式（沒有密碼）時不能解除連結。
const canUnlink = computed(() => !!account.value?.hasPassword)

const isBusy = ref(false)
const error = ref('')

const run = async (action: () => Promise<void>) => {
  error.value = ''
  isBusy.value = true
  try {
    await action()
  } catch (e) {
    error.value = getApiErrorMessage(e, t('settings.common.failed'))
  } finally {
    isBusy.value = false
  }
}

const handleLink = () => run(accountStore.linkGoogle)
const handleUnlink = () => run(accountStore.unlinkGoogle)
</script>

<style scoped lang="scss"></style>
