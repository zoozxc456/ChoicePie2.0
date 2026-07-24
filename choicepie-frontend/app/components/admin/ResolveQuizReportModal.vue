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
            {{ action === 'resolve' ? t('adminQuizReports.resolveAction') : t('adminQuizReports.dismissAction') }}
          </h2>
          <label class="text-xs font-semibold text-neutral-500 mb-1 block">
            {{ t('adminQuizReports.noteLabel') }}
          </label>
          <textarea
            v-model="note"
            rows="3"
            class="w-full rounded-xl border border-neutral-200 px-3 py-2 text-sm resize-none"
            :placeholder="t('adminQuizReports.notePlaceholder')"
          />
        </div>
        <div class="flex gap-3 px-6 pb-6">
          <UButton
            block
            color="neutral"
            variant="ghost"
            @click="handleCancel"
          >
            {{ t('adminQuizReports.cancel') }}
          </UButton>
          <UButton
            block
            :color="action === 'resolve' ? 'error' : 'primary'"
            :loading="isSubmitting"
            @click="handleConfirm"
          >
            {{ t('adminQuizReports.confirm') }}
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
  action: 'resolve' | 'dismiss'
}

interface Emits {
  (e: 'confirm', note?: string): void
  (e: 'cancel'): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const { t } = useI18n()

const note = ref('')

watch(() => props.open, (isOpen) => {
  if (isOpen) {
    note.value = ''
  }
})

const handleConfirm = () => {
  emit('confirm', note.value.trim() || undefined)
}

const handleCancel = () => {
  emit('cancel')
}
</script>

<script lang="ts">
export default {
  name: 'ResolveQuizReportModal'
}
</script>

<style scoped lang="scss"></style>
