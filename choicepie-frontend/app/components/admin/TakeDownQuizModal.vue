<template>
  <Transition name="fade">
    <div
      v-if="open"
      class="fixed inset-0 z-50 flex items-center justify-center bg-black/50"
      @click.self="handleCancel"
    >
      <div class="rounded-2xl bg-white w-full max-w-md mx-4 overflow-hidden shadow-cp-xl">
        <div class="px-6 pt-6 pb-4">
          <h2 class="text-xl font-bold mb-3">
            {{ t('adminQuizzes.takeDownAction') }}
          </h2>
          <label class="text-xs font-semibold text-neutral-500 mb-1 block">
            {{ t('adminQuizzes.takedownReasonLabel') }}
          </label>
          <textarea
            v-model="reason"
            rows="3"
            class="w-full rounded-xl border border-neutral-200 px-3 py-2 text-sm resize-none"
            :placeholder="t('adminQuizzes.takedownReasonPlaceholder')"
          />
          <p
            v-if="showError"
            class="text-xs text-error-500 mt-1"
          >
            {{ t('adminQuizzes.takedownReasonRequired') }}
          </p>
        </div>
        <div class="flex gap-3 px-6 pb-6">
          <UButton
            block
            color="neutral"
            variant="ghost"
            @click="handleCancel"
          >
            {{ t('adminQuizzes.cancel') }}
          </UButton>
          <UButton
            block
            color="error"
            :loading="isSubmitting"
            @click="handleConfirm"
          >
            {{ t('adminQuizzes.confirm') }}
          </UButton>
        </div>
      </div>
    </div>
  </Transition>
</template>

<script setup lang="ts">
interface Props {
  open: boolean
  isSubmitting: boolean
}

interface Emits {
  (e: 'confirm', reason: string): void
  (e: 'cancel'): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const { t } = useI18n()

const reason = ref('')
const showError = ref(false)

watch(() => props.open, (isOpen) => {
  if (isOpen) {
    reason.value = ''
    showError.value = false
  }
})

const handleConfirm = () => {
  if (!reason.value.trim()) {
    showError.value = true
    return
  }
  emit('confirm', reason.value.trim())
}

const handleCancel = () => {
  emit('cancel')
}
</script>

<script lang="ts">
export default {
  name: 'TakeDownQuizModal'
}
</script>

<style scoped lang="scss"></style>
