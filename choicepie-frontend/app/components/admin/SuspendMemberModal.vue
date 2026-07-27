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
            {{ t('adminMembers.suspendAction') }}
          </h2>
          <label class="text-xs font-semibold text-neutral-500 mb-1 block">
            {{ t('adminMembers.suspendReasonLabel') }}
          </label>
          <textarea
            v-model="reason"
            rows="3"
            class="w-full rounded-xl border border-neutral-200 px-3 py-2 text-sm resize-none mb-3"
            :placeholder="t('adminMembers.suspendReasonPlaceholder')"
          />
          <p
            v-if="showError"
            class="text-xs text-error-500 -mt-2 mb-3"
          >
            {{ t('adminMembers.suspendReasonRequired') }}
          </p>
          <label class="text-xs font-semibold text-neutral-500 mb-1 block">
            {{ t('adminMembers.suspendUntilLabel') }}
          </label>
          <input
            v-model="until"
            type="date"
            class="w-full rounded-xl border border-neutral-200 px-3 py-2 text-sm"
          >
        </div>
        <div class="flex gap-3 px-6 pb-6">
          <UButton
            block
            color="neutral"
            variant="ghost"
            @click="handleCancel"
          >
            {{ t('adminMembers.cancel') }}
          </UButton>
          <UButton
            block
            color="error"
            :loading="isSubmitting"
            @click="handleConfirm"
          >
            {{ t('adminMembers.confirm') }}
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
  (e: 'confirm', reason: string, until: string | null): void
  (e: 'cancel'): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const { t } = useI18n()

const reason = ref('')
const until = ref('')
const showError = ref(false)

watch(() => props.open, (isOpen) => {
  if (isOpen) {
    reason.value = ''
    until.value = ''
    showError.value = false
  }
})

const handleConfirm = () => {
  if (!reason.value.trim()) {
    showError.value = true
    return
  }
  emit('confirm', reason.value.trim(), until.value ? new Date(until.value).toISOString() : null)
}

const handleCancel = () => {
  emit('cancel')
}
</script>

<script lang="ts">
export default {
  name: 'SuspendMemberModal'
}
</script>

<style scoped lang="scss"></style>
