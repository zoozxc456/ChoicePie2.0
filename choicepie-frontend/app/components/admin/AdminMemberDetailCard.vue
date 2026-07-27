<template>
  <div class="relative grid grid-cols-[1fr_180px] rounded-2xl bg-white border border-neutral-200 shadow-sm overflow-hidden">
    <div class="p-6 min-w-0">
      <div class="flex items-center gap-3 min-w-0">
        <img
          v-if="member.avatar"
          :src="member.avatar"
          class="w-12 h-12 rounded-full object-cover shrink-0"
          alt=""
        >
        <div
          v-else
          class="w-12 h-12 rounded-full bg-neutral-100 flex items-center justify-center text-lg font-bold text-neutral-400 shrink-0"
        >
          {{ member.name.charAt(0) }}
        </div>
        <div class="min-w-0">
          <p class="text-lg font-bold truncate">
            {{ member.name }}
          </p>
          <p class="text-base text-neutral-400 truncate">
            {{ member.email }}
          </p>
        </div>
      </div>

      <dl class="grid grid-cols-2 gap-4 mt-5 pt-5 text-base border-t border-dashed border-neutral-200">
        <div>
          <dt class="text-neutral-400">
            {{ t('adminMemberDetail.createdAt') }}
          </dt>
          <dd class="mt-0.5 font-medium">
            {{ formatDate(member.createdAt) }}
          </dd>
        </div>
        <div>
          <dt class="text-neutral-400">
            {{ t('adminMemberDetail.lastAiGenerationAt') }}
          </dt>
          <dd class="mt-0.5 font-medium">
            {{ member.lastAiGenerationAt ? formatDate(member.lastAiGenerationAt) : t('adminMemberDetail.never') }}
          </dd>
        </div>
      </dl>

      <div class="flex justify-end mt-6">
        <UButton
          v-if="!member.isSuspended"
          size="sm"
          color="error"
          variant="soft"
          @click="emit('suspend')"
        >
          {{ t('adminMembers.suspendAction') }}
        </UButton>
        <UButton
          v-else
          size="sm"
          color="primary"
          variant="soft"
          :loading="isUnsuspending"
          @click="emit('unsuspend')"
        >
          {{ t('adminMembers.unsuspendAction') }}
        </UButton>
      </div>
    </div>

    <AdminStatusStub
      size="lg"
      :icon="member.isSuspended ? 'i-lucide-shield-off' : 'i-lucide-shield-check'"
      :label="member.isSuspended ? t('adminMembers.statusSuspended') : t('adminMembers.statusActive')"
      :border-class="member.isSuspended ? 'border-error-200' : 'border-neutral-200'"
      :bg-class="member.isSuspended ? 'bg-error-50' : 'bg-success-50'"
      :text-class="member.isSuspended ? 'text-error-700' : 'text-success-700'"
    >
      <template #subtext>
        <p
          v-if="!member.isSuspended"
          class="text-sm text-neutral-400"
        >
          {{ t('adminMemberDetail.statusHealthy') }}
        </p>
        <p
          v-else
          class="text-sm text-error-600"
        >
          {{ member.suspendedUntil
            ? t('adminMembers.suspendedUntil', { date: formatDate(member.suspendedUntil) })
            : t('adminMembers.suspendedPermanently') }}
        </p>
      </template>

      <template
        v-if="member.isSuspended"
        #reason
      >
        <p class="w-full mt-1 pt-2.5 border-t border-dashed border-error-200 text-xs leading-relaxed text-error-600">
          {{ member.suspendedReason }}
        </p>
      </template>
    </AdminStatusStub>
  </div>
</template>

<script setup lang="ts">
import type { AdminMemberDetailDto } from '~/types/api'

interface Props {
  member: AdminMemberDetailDto
  isUnsuspending: boolean
}

defineProps<Props>()

const emit = defineEmits<{
  suspend: []
  unsuspend: []
}>()

const { t, locale } = useI18n()

const formatDate = (iso: string) => new Date(iso).toLocaleDateString(locale.value)
</script>

<script lang="ts">
export default {
  name: 'AdminMemberDetailCard'
}
</script>

<style scoped lang="scss"></style>
