<template>
  <SettingsSectionCard
    :title="t('settings.profile.title')"
    :description="t('settings.profile.description')"
  >
    <div class="flex items-center gap-4">
      <span class="flex items-center justify-center w-20 h-20 rounded-full overflow-hidden text-2xl font-bold text-white bg-cp-primary shrink-0">
        <img
          v-if="avatarSrc"
          :src="avatarSrc"
          :alt="t('settings.profile.avatarAlt')"
          class="w-full h-full object-cover"
        >
        <template v-else>{{ initial }}</template>
      </span>

      <div class="flex flex-col gap-1.5">
        <input
          ref="fileInputRef"
          type="file"
          :accept="ALLOWED_IMAGE_TYPES.join(',')"
          class="hidden"
          @change="handleFileChange"
        >
        <UButton
          color="neutral"
          variant="outline"
          size="sm"
          :loading="isUploading"
          @click="fileInputRef?.click()"
        >
          {{ isUploading ? t('settings.profile.uploading') : t('settings.profile.changeAvatar') }}
        </UButton>
        <p class="text-xs text-neutral-500">
          {{ t('settings.profile.avatarHint') }}
        </p>
        <p
          v-if="avatarError"
          class="text-xs text-error-500"
        >
          {{ avatarError }}
        </p>
      </div>
    </div>

    <UFormField :label="t('settings.profile.email')">
      <UInput
        :model-value="accountStore.account?.email"
        disabled
        size="lg"
        class="w-full"
        :ui="{ base: 'bg-neutral-100 h-12 text-sm px-4' }"
      />
    </UFormField>

    <form
      class="flex flex-col gap-3.5"
      @submit.prevent="handleSave"
    >
      <UFormField
        :label="t('settings.profile.name')"
        :error="nameError || undefined"
      >
        <UInput
          v-model="name"
          size="lg"
          class="w-full"
          :ui="{ base: 'bg-neutral-100 h-12 text-sm px-4' }"
        />
      </UFormField>

      <p
        v-if="saveError"
        class="text-sm text-error-500"
      >
        {{ saveError }}
      </p>
      <p
        v-if="saved"
        class="text-sm text-success-600"
      >
        {{ t('settings.profile.saved') }}
      </p>

      <UButton
        type="submit"
        size="lg"
        color="primary"
        class="font-bold h-12 rounded-2xl self-start px-6"
        :loading="isSaving"
        :disabled="!isNameChanged"
      >
        {{ t('settings.profile.save') }}
      </UButton>
    </form>
  </SettingsSectionCard>
</template>

<script setup lang="ts">
import SettingsSectionCard from '~/components/settings/SettingsSectionCard.vue'
import { ALLOWED_IMAGE_TYPES, validateImageFile } from '~/utils/imageFile'

const NAME_MIN = 2
const NAME_MAX = 20

const { t } = useI18n()
const accountStore = useAccountStore()

const name = ref(accountStore.account?.name ?? '')
const nameError = ref('')
const saveError = ref('')
const saved = ref(false)
const isSaving = ref(false)

const fileInputRef = ref<HTMLInputElement | null>(null)
const previewUrl = ref<string | null>(null)
const avatarError = ref('')
const isUploading = ref(false)

const avatarSrc = computed(() => previewUrl.value ?? accountStore.account?.avatar ?? null)
const initial = computed(() => (accountStore.account?.name ?? '?').charAt(0).toUpperCase())
const isNameChanged = computed(() => name.value.trim() !== accountStore.account?.name)

watch(() => accountStore.account?.name, (value) => {
  name.value = value ?? ''
})

const handleSave = async () => {
  saved.value = false
  saveError.value = ''
  const trimmed = name.value.trim()
  if (trimmed.length < NAME_MIN || trimmed.length > NAME_MAX) {
    nameError.value = t('settings.profile.nameLength', { min: NAME_MIN, max: NAME_MAX })
    return
  }
  nameError.value = ''

  isSaving.value = true
  try {
    await accountStore.updateName(trimmed)
    saved.value = true
  } catch (e) {
    saveError.value = getApiErrorMessage(e, t('settings.common.failed'))
  } finally {
    isSaving.value = false
  }
}

const handleFileChange = async (event: Event) => {
  const input = event.target as HTMLInputElement
  const file = input.files?.[0]
  input.value = ''
  if (!file) return

  avatarError.value = ''
  const invalid = validateImageFile(file)
  if (invalid) {
    avatarError.value = t(invalid === 'invalidType' ? 'settings.profile.avatarInvalidType' : 'settings.profile.avatarTooLarge')
    return
  }

  previewUrl.value = URL.createObjectURL(file)
  isUploading.value = true
  try {
    await accountStore.uploadAvatar(file)
  } catch (e) {
    avatarError.value = getApiErrorMessage(e, t('settings.common.failed'))
  } finally {
    URL.revokeObjectURL(previewUrl.value)
    previewUrl.value = null
    isUploading.value = false
  }
}
</script>

<style scoped lang="scss"></style>
