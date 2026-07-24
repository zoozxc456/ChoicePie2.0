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
            {{ t('libraryDetail.report.title') }}
          </h2>

          <label class="text-xs font-semibold text-neutral-500 mb-1 block">
            {{ t('libraryDetail.report.reasonLabel') }}
          </label>
          <select
            v-model="reason"
            class="w-full rounded-xl border border-neutral-200 px-3 py-2 text-sm mb-3"
          >
            <option
              v-for="option in reasonOptions"
              :key="option.value"
              :value="option.value"
            >
              {{ option.label }}
            </option>
          </select>

          <label class="text-xs font-semibold text-neutral-500 mb-1 block">
            {{ t('libraryDetail.report.descriptionLabel') }}
          </label>
          <textarea
            v-model="description"
            rows="3"
            class="w-full rounded-xl border border-neutral-200 px-3 py-2 text-sm resize-none"
            :placeholder="t('libraryDetail.report.descriptionPlaceholder')"
          />
        </div>
        <div class="flex gap-3 px-6 pb-6">
          <UButton
            block
            color="neutral"
            variant="ghost"
            @click="handleCancel"
          >
            {{ t('libraryDetail.report.cancel') }}
          </UButton>
          <UButton
            block
            color="error"
            :loading="isSubmitting"
            @click="handleConfirm"
          >
            {{ t('libraryDetail.report.confirm') }}
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
  (e: 'confirm', reason: string, description?: string): void
  (e: 'cancel'): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const { t } = useI18n()

const REASON_VALUES = ['InappropriateContent', 'Spam', 'Copyright', 'Other'] as const

const reason = ref<typeof REASON_VALUES[number]>('InappropriateContent')
const description = ref('')

const reasonOptions = computed(() => REASON_VALUES.map(value => ({
  value,
  label: t(`libraryDetail.report.reasons.${value}`)
})))

watch(() => props.open, (isOpen) => {
  if (isOpen) {
    reason.value = 'InappropriateContent'
    description.value = ''
  }
})

const handleConfirm = () => {
  emit('confirm', reason.value, description.value.trim() || undefined)
}

const handleCancel = () => {
  emit('cancel')
}
</script>

<script lang="ts">
export default {
  name: 'ReportQuizModal'
}
</script>

<style scoped lang="scss"></style>
