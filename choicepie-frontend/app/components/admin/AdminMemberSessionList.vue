<template>
  <div class="rounded-2xl bg-white border border-neutral-200 p-5">
    <p class="text-lg font-bold mb-3">
      {{ title }}
    </p>
    <div
      v-if="isLoading"
      class="flex justify-center py-8"
    >
      <UIcon
        name="i-lucide-loader-2"
        class="animate-spin text-2xl text-primary-500"
      />
    </div>
    <p
      v-else-if="!sessions.length"
      class="text-base text-neutral-400 text-center py-8"
    >
      {{ emptyText }}
    </p>
    <template v-else>
      <p
        v-if="countText"
        class="text-xs text-neutral-400 mb-2"
      >
        {{ countText }}
      </p>
      <ul class="flex flex-col divide-y divide-neutral-100">
        <li
          v-for="session in sessions"
          :key="session.id"
          class="py-3 flex items-center justify-between gap-3"
        >
          <span class="text-base font-medium truncate">
            {{ session.quizTitle }}
          </span>
          <span class="text-xs text-neutral-400 shrink-0">
            {{ formatDate(session.playedAtUtc) }}
          </span>
        </li>
      </ul>
    </template>
  </div>
</template>

<script setup lang="ts">
import type { GameSessionSummaryDto } from '~/types/api'

interface Props {
  title: string
  sessions: GameSessionSummaryDto[]
  isLoading: boolean
  emptyText: string
  countText?: string
}

defineProps<Props>()

const { locale } = useI18n()

const formatDate = (iso: string) => new Date(iso).toLocaleDateString(locale.value)
</script>

<script lang="ts">
export default {
  name: 'AdminMemberSessionList'
}
</script>

<style scoped lang="scss"></style>
