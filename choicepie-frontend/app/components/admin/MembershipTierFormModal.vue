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
            {{ tier ? t('adminMembershipTiers.editTitle') : t('adminMembershipTiers.createTitle') }}
          </h2>

          <label class="text-xs font-semibold text-neutral-500 mb-1 block">
            {{ t('adminMembershipTiers.nameLabel') }}
          </label>
          <input
            v-model="name"
            type="text"
            class="w-full rounded-xl border border-neutral-200 px-3 py-2 text-sm mb-3"
            :placeholder="t('adminMembershipTiers.namePlaceholder')"
          >

          <label class="text-xs font-semibold text-neutral-500 mb-1 block">
            {{ t('adminMembershipTiers.dailyGenerationLimitLabel') }}
          </label>
          <input
            v-model.number="dailyGenerationLimit"
            type="number"
            min="0"
            class="w-full rounded-xl border border-neutral-200 px-3 py-2 text-sm mb-3"
          >

          <label class="text-xs font-semibold text-neutral-500 mb-1 block">
            {{ t('adminMembershipTiers.dailyTokenBudgetLabel') }}
          </label>
          <input
            v-model.number="dailyTokenBudget"
            type="number"
            min="0"
            class="w-full rounded-xl border border-neutral-200 px-3 py-2 text-sm"
          >

          <p
            v-if="showError"
            class="text-xs text-error-500 mt-3"
          >
            {{ t('adminMembershipTiers.validationError') }}
          </p>
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
            color="primary"
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
import type { MembershipTierDto } from '~/types/api'

interface Props {
  open: boolean
  isSubmitting: boolean
  tier: MembershipTierDto | null
}

interface Emits {
  (e: 'confirm', name: string, dailyGenerationLimit: number, dailyTokenBudget: number): void
  (e: 'cancel'): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const { t } = useI18n()

const name = ref('')
const dailyGenerationLimit = ref(0)
const dailyTokenBudget = ref(0)
const showError = ref(false)

watch(() => props.open, (isOpen) => {
  if (isOpen) {
    name.value = props.tier?.name ?? ''
    dailyGenerationLimit.value = props.tier?.dailyGenerationLimit ?? 0
    dailyTokenBudget.value = props.tier?.dailyTokenBudget ?? 0
    showError.value = false
  }
})

const handleConfirm = () => {
  if (!name.value.trim() || dailyGenerationLimit.value < 0 || dailyTokenBudget.value < 0) {
    showError.value = true
    return
  }
  emit('confirm', name.value.trim(), dailyGenerationLimit.value, dailyTokenBudget.value)
}

const handleCancel = () => {
  emit('cancel')
}
</script>

<script lang="ts">
export default {
  name: 'MembershipTierFormModal'
}
</script>

<style scoped lang="scss"></style>
