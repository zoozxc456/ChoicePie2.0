<template>
  <div class="rounded-2xl bg-white border border-neutral-200 p-5">
    <div class="flex items-center justify-between mb-3">
      <p class="text-lg font-bold">
        {{ t('adminMemberDetail.tierUsageTitle') }}
      </p>
      <select
        :value="member.tierId ?? ''"
        class="rounded-xl border border-neutral-200 px-2 py-1.5 text-sm"
        :disabled="isAssigningTier"
        @change="handleTierChange"
      >
        <option
          v-if="!member.tierId"
          value=""
          disabled
        >
          {{ t('adminMemberDetail.tierUnassigned') }}
        </option>
        <option
          v-for="tier in tiers"
          :key="tier.id"
          :value="tier.id"
        >
          {{ tier.name }}
        </option>
      </select>
    </div>

    <div
      v-if="isLoadingUsage"
      class="flex justify-center py-8"
    >
      <UIcon
        name="i-lucide-loader-2"
        class="animate-spin text-2xl text-primary-500"
      />
    </div>

    <dl
      v-else-if="currentTier"
      class="grid grid-cols-2 gap-4 text-base"
    >
      <div>
        <dt class="text-neutral-400">
          {{ t('adminMemberDetail.todayGenerationCount') }}
        </dt>
        <dd class="mt-0.5 font-medium">
          {{ t('adminMemberDetail.usageOverLimit', {
            used: aiUsage?.todayGenerationCount ?? 0,
            limit: currentTier.dailyGenerationLimit
          }) }}
        </dd>
      </div>
      <div>
        <dt class="text-neutral-400">
          {{ t('adminMemberDetail.todayTokensUsed') }}
        </dt>
        <dd class="mt-0.5 font-medium">
          {{ t('adminMemberDetail.usageOverLimit', {
            used: aiUsage?.todayTokensUsed ?? 0,
            limit: currentTier.dailyTokenBudget
          }) }}
        </dd>
      </div>
      <div>
        <dt class="text-neutral-400">
          {{ t('adminMemberDetail.totalGenerationCount') }}
        </dt>
        <dd class="mt-0.5 font-medium">
          {{ aiUsage?.totalGenerationCount ?? 0 }}
        </dd>
      </div>
      <div>
        <dt class="text-neutral-400">
          {{ t('adminMemberDetail.totalTokensUsed') }}
        </dt>
        <dd class="mt-0.5 font-medium">
          {{ aiUsage?.totalTokensUsed ?? 0 }}
        </dd>
      </div>
    </dl>

    <p
      v-else
      class="text-base text-neutral-400 text-center py-8"
    >
      {{ t('adminMemberDetail.tierUnassigned') }}
    </p>
  </div>
</template>

<script setup lang="ts">
import type { AdminMemberAiUsageDto, AdminMemberDetailDto, MembershipTierDto } from '~/types/api'

interface Props {
  member: AdminMemberDetailDto
  tiers: MembershipTierDto[]
  aiUsage: AdminMemberAiUsageDto | null
  isLoadingUsage: boolean
  isAssigningTier: boolean
}

const props = defineProps<Props>()

const emit = defineEmits<{
  assignTier: [tierId: string]
}>()

const { t } = useI18n()

const currentTier = computed(() => props.tiers.find(tier => tier.id === props.member.tierId) ?? null)

const handleTierChange = (event: Event) => {
  const tierId = (event.target as HTMLSelectElement).value
  if (tierId) emit('assignTier', tierId)
}
</script>

<script lang="ts">
export default {
  name: 'AdminMemberTierUsageCard'
}
</script>

<style scoped lang="scss"></style>
