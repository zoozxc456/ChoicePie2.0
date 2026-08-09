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

    <div
      v-if="adminMembershipTierStore.isLoading"
      class="flex justify-center py-16"
    >
      <UIcon
        name="i-lucide-loader-2"
        class="animate-spin text-3xl text-primary-500"
      />
    </div>

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
      <div
        v-for="tier in tiers"
        :key="tier.id"
        class="rounded-2xl bg-white border border-neutral-200 shadow-sm p-4 flex items-center justify-between gap-3"
      >
        <div class="min-w-0">
          <div class="flex items-center gap-2">
            <p class="text-lg font-bold truncate">
              {{ tier.name }}
            </p>
            <span
              v-if="tier.isDefault"
              class="text-xs font-semibold px-2 py-0.5 rounded-full bg-primary-50 text-primary-700 border border-primary-200"
            >
              {{ t('adminMembershipTiers.defaultBadge') }}
            </span>
          </div>
          <p class="text-base text-neutral-400 mt-0.5">
            {{ t('adminMembershipTiers.limitsLine', {
              count: tier.dailyGenerationLimit,
              tokens: tier.dailyTokenBudget
            }) }}
          </p>
        </div>

        <UButton
          size="sm"
          color="neutral"
          variant="soft"
          @click="openEditModal(tier)"
        >
          {{ t('adminMembershipTiers.editAction') }}
        </UButton>
      </div>
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
import type { MembershipTierDto } from '~/types/api'

definePageMeta({ layout: 'admin', middleware: ['admin-auth'] })

const { t } = useI18n()
const adminMembershipTierStore = useAdminMembershipTierStore()

const isModalOpen = ref(false)
const editingTier = ref<MembershipTierDto | null>(null)

const tiers = computed(() => adminMembershipTierStore.tiers)

await adminMembershipTierStore.fetchTiers()

const openCreateModal = () => {
  editingTier.value = null
  isModalOpen.value = true
}

const openEditModal = (tier: MembershipTierDto) => {
  editingTier.value = tier
  isModalOpen.value = true
}

const closeModal = () => {
  isModalOpen.value = false
  editingTier.value = null
}

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
