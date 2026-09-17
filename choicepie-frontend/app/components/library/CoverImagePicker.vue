<template>
  <div class="bg-white border border-neutral-200 rounded-2xl p-5 flex flex-col gap-3">
    <label class="text-[13px] font-bold">
      {{ t('coverPicker.label') }}
    </label>

    <div class="flex items-start gap-4">
      <QuizCoverThumbnail
        :cover-image-url="modelValue.coverImageUrl"
        :cover-emoji="modelValue.coverEmoji"
        :cover-gradient="modelValue.coverGradient"
        size="sm"
      />

      <div class="flex-1 flex flex-col gap-3">
        <div class="flex gap-2">
          <button
            type="button"
            class="h-9 px-3.5 rounded-full text-xs font-semibold"
            :class="mode === 'color' ? 'bg-secondary-800 text-white' : 'bg-neutral-100 text-neutral-600'"
            @click="mode = 'color'"
          >
            {{ t('coverPicker.colorMode') }}
          </button>
          <button
            type="button"
            class="h-9 px-3.5 rounded-full text-xs font-semibold"
            :class="mode === 'upload' ? 'bg-secondary-800 text-white' : 'bg-neutral-100 text-neutral-600'"
            @click="mode = 'upload'"
          >
            {{ t('coverPicker.uploadMode') }}
          </button>
        </div>

        <div
          v-if="mode === 'color'"
          class="flex flex-col gap-3"
        >
          <div class="flex gap-2">
            <button
              v-for="color in COLOR_OPTIONS"
              :key="color"
              type="button"
              class="w-8 h-8 rounded-full transition-transform"
              :class="[COLOR_CLASS_MAP[color], modelValue.coverGradient === color ? 'ring-2 ring-offset-2 ring-secondary-800 scale-110' : '']"
              :aria-label="color"
              @click="selectColor(color)"
            />
          </div>
          <div class="flex flex-wrap gap-2">
            <button
              v-for="emoji in EMOJI_OPTIONS"
              :key="emoji"
              type="button"
              class="w-9 h-9 rounded-xl text-lg flex items-center justify-center"
              :class="modelValue.coverEmoji === emoji ? 'bg-secondary-800 text-white' : 'bg-neutral-100'"
              @click="selectEmoji(emoji)"
            >
              {{ emoji }}
            </button>
          </div>
        </div>

        <div v-else>
          <input
            ref="fileInputRef"
            type="file"
            accept="image/jpeg,image/png,image/webp"
            class="hidden"
            @change="handleFileChange"
          >
          <button
            type="button"
            class="w-full h-11 rounded-xl border-[1.5px] border-dashed border-neutral-200 hover:bg-neutral-100 text-neutral-600 font-semibold text-xs"
            :disabled="isUploading"
            @click="fileInputRef?.click()"
          >
            {{ isUploading ? t('coverPicker.uploading') : (modelValue.coverImageUrl ? t('coverPicker.changeImage') : t('coverPicker.uploadHint')) }}
          </button>
          <p
            v-if="uploadError"
            class="text-xs text-error-500 mt-2"
          >
            {{ uploadError }}
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import QuizCoverThumbnail from '~/components/library/QuizCoverThumbnail.vue'
import { useQuizStore } from '~/stores/quiz'
import type { QuizCoverInput } from '~/stores/quiz'

interface Props {
  modelValue: QuizCoverInput
}

interface Emits {
  (e: 'update:modelValue', value: QuizCoverInput): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const { t } = useI18n()
const quizStore = useQuizStore()

const MAX_FILE_SIZE_BYTES = 5 * 1024 * 1024
const ALLOWED_CONTENT_TYPES = ['image/jpeg', 'image/png', 'image/webp']
const COLOR_OPTIONS = ['primary', 'secondary', 'success', 'danger', 'warning', 'info'] as const
const EMOJI_OPTIONS = ['📝', '🎯', '🧠', '🎲', '🏆', '📚', '⚡', '🎨']

const COLOR_CLASS_MAP: Record<string, string> = {
  primary: 'bg-cp-primary',
  secondary: 'bg-cp-secondary',
  success: 'bg-cp-success',
  danger: 'bg-cp-danger',
  warning: 'bg-cp-warning',
  info: 'bg-cp-info'
}

const mode = ref<'color' | 'upload'>(props.modelValue.coverImageUrl ? 'upload' : 'color')
const fileInputRef = ref<HTMLInputElement | null>(null)
const isUploading = ref(false)
const uploadError = ref<string | null>(null)

const selectColor = (color: string) => {
  emit('update:modelValue', { ...props.modelValue, coverImageUrl: null, coverGradient: color })
}

const selectEmoji = (emoji: string) => {
  emit('update:modelValue', { ...props.modelValue, coverImageUrl: null, coverEmoji: emoji })
}

const handleFileChange = async (event: Event) => {
  const file = (event.target as HTMLInputElement).files?.[0]
  if (!file) return

  uploadError.value = null

  if (!ALLOWED_CONTENT_TYPES.includes(file.type)) {
    uploadError.value = t('coverPicker.invalidFileType')
    return
  }
  if (file.size > MAX_FILE_SIZE_BYTES) {
    uploadError.value = t('coverPicker.fileTooLarge')
    return
  }

  isUploading.value = true
  try {
    const imageUrl = await quizStore.uploadCoverImage(file)
    emit('update:modelValue', { ...props.modelValue, coverImageUrl: imageUrl })
  } catch (e) {
    uploadError.value = t('coverPicker.uploadFailed')
    console.error(e)
  } finally {
    isUploading.value = false
  }
}
</script>

<style scoped lang="scss"></style>
