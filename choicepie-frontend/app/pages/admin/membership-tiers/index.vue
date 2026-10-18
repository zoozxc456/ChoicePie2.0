<template>
  <div>
    <div class="flex items-center justify-between mb-4">
      <h1 class="text-lg font-extrabold">
        {{ t('adminMembershipTiers.title') }}
      </h1>
      <UButton
        size="sm"
        color="primary"
        icon="i-lucide-plus"
        @click="openCreateModal"
      >
        {{ t('adminMembershipTiers.createAction') }}
      </UButton>
    </div>

    <LoadingSpinner
      v-if="adminMembershipTierStore.isLoading"
      size="md"
    />

    <p
      v-else-if="!tiers.length"
      class="text-base text-neutral-400 text-center py-16"
    >
      {{ t('adminMembershipTiers.empty') }}
    </p>

    <div
      v-else
      class="flex flex-col gap-3"
    >
      <AdminMembershipTierRow
        v-for="tier in tiers"
        :key="tier.id"
        :tier="tier"
        @edit="openEditModal(tier)"
      />
    </div>

    <MembershipTierFormModal
      :open="isModalOpen"
      :is-submitting="adminMembershipTierStore.isSaving"
      :tier="editingTier"
      @confirm="handleConfirm"
      @cancel="closeModal"
    />
  </div>
</template>

<script setup lang="ts">
import AdminMembershipTierRow from '~/components/admin/AdminMembershipTierRow.vue'
import MembershipTierFormModal from '~/components/admin/MembershipTierFormModal.vue'
import LoadingSpinner from '~/components/common/LoadingSpinner.vue'
import { useModalTarget } from '~/composables/useModalTarget'
import type { MembershipTierDto } from '~/types/api'

definePageMeta({ layout: 'admin', middleware: ['admin-auth'] })

const { t } = useI18n()
const adminMembershipTierStore = useAdminMembershipTierStore()

const { isOpen: isModalOpen, target: editingTier, open, close: closeModal } = useModalTarget<MembershipTierDto>()

const tiers = computed(() => adminMembershipTierStore.tiers)

await adminMembershipTierStore.fetchTiers()

const openCreateModal = () => open(null)

const openEditModal = (tier: MembershipTierDto) => open(tier)

const handleConfirm = async (name: string, dailyGenerationLimit: number, dailyTokenBudget: number) => {
  try {
    if (editingTier.value) {
      await adminMembershipTierStore.updateTier(editingTier.value.id, { name, dailyGenerationLimit, dailyTokenBudget })
    } else {
      await adminMembershipTierStore.createTier({ name, dailyGenerationLimit, dailyTokenBudget })
    }
    closeModal()
  } catch {
    // 錯誤已寫入 adminMembershipTierStore.error，這裡不需要額外處理
  }
}
</script>

<script lang="ts">
export default {
  name: 'AdminMembershipTiersPage'
}
</script>

<style scoped lang="scss"></style>
