<template>
  <div class="rounded-2xl bg-white border border-neutral-200 p-4 flex items-center justify-between gap-3">
    <div class="min-w-0">
      <div class="flex items-center gap-2">
        <p class="text-lg font-bold truncate">
          {{ tier.name }}
        </p>
        <span
          v-if="tier.isDefault"
          data-testid="tier-default"
          class="text-xs font-semibold px-2 py-0.5 rounded-full bg-primary-50 text-primary-700 border border-primary-200"
        >
          {{ t('adminMembershipTiers.defaultBadge') }}
        </span>
      </div>
      <p
        data-testid="tier-limits"
        class="text-base text-neutral-400 mt-0.5"
      >
        {{ t('adminMembershipTiers.limitsLine', {
          count: tier.dailyGenerationLimit,
          tokens: tier.dailyTokenBudget
        }) }}
      </p>
    </div>
    <UButton
      data-testid="tier-edit"
      size="sm"
      color="neutral"
      variant="soft"
      @click="emit('edit')"
    >
      {{ t('adminMembershipTiers.editAction') }}
    </UButton>
  </div>
</template>

<script setup lang="ts">
import type { MembershipTierDto } from '~/types/api'

interface Props {
  tier: MembershipTierDto
}

defineProps<Props>()

const emit = defineEmits<{
  edit: []
}>()

const { t } = useI18n()
</script>

<script lang="ts">
export default {
  name: 'AdminMembershipTierRow'
}
</script>

<style scoped lang="scss"></style>
