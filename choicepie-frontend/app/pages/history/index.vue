<template>
  <div class="max-w-7xl mx-auto px-6 py-8">
    <div class="mb-7 text-center">
      <h1 class="text-2xl font-black">
        {{ t('history.title') }}
      </h1>
      <p class="text-sm text-cp-text-secondary mt-1.5">
        {{ t('history.subtitle') }}
      </p>
    </div>

    <div class="flex justify-center mb-10">
      <SegmentedControl
        v-model="activeRole"
        :options="roles"
      />
    </div>

    <LoadingSpinner v-if="gameSessionStore.isLoading" />

    <!-- Host history -->
    <template v-else-if="activeRole === 'host'">
      <EmptyState
        v-if="!gameSessionStore.hostedSessions.length"
        emoji="🥧"
        :title="t('history.host.empty')"
        :description="t('history.host.emptyDesc')"
      >
        <NuxtLink to="/library/new">
          <UButton
            color="primary"
            size="lg"
            class="rounded-full px-6 font-bold"
          >
            {{ t('history.host.emptyCta') }}
          </UButton>
        </NuxtLink>
      </EmptyState>
      <div
        v-else
        class="grid gap-4 grid-cols-[repeat(auto-fill,minmax(300px,1fr))]"
      >
        <HostedGameCard
          v-for="session in gameSessionStore.hostedSessions"
          :key="session.id"
          :session="session"
        />
      </div>
    </template>

    <!-- Player history -->
    <template v-else>
      <EmptyState
        v-if="!gameSessionStore.playedSessions.length"
        emoji="🥧"
        :title="t('history.player.empty')"
        :description="t('history.player.emptyDesc')"
      >
        <NuxtLink to="/join">
          <UButton
            color="primary"
            size="lg"
            class="rounded-full px-6 font-bold"
          >
            {{ t('history.player.emptyCta') }}
          </UButton>
        </NuxtLink>
      </EmptyState>
      <div
        v-else
        class="grid gap-4 grid-cols-[repeat(auto-fill,minmax(300px,1fr))]"
      >
        <PlayedGameCard
          v-for="session in gameSessionStore.playedSessions"
          :key="session.id"
          :session="session"
        />
      </div>
    </template>
  </div>
</template>

<script setup lang="ts">
import EmptyState from '~/components/common/EmptyState.vue'
import LoadingSpinner from '~/components/common/LoadingSpinner.vue'
import SegmentedControl from '~/components/common/SegmentedControl.vue'
import HostedGameCard from '~/components/history/HostedGameCard.vue'
import PlayedGameCard from '~/components/history/PlayedGameCard.vue'
import { useGameSessionStore } from '~/stores/gameSession'

definePageMeta({ layout: 'content', middleware: ['auth'] })

const { t } = useI18n()
const gameSessionStore = useGameSessionStore()

type RoleKey = 'host' | 'player'

const activeRole = ref<RoleKey>('host')

const roles = computed(() => [
  { value: 'host' as RoleKey, label: t('history.roles.host') },
  { value: 'player' as RoleKey, label: t('history.roles.player') }
])

onMounted(() => {
  gameSessionStore.fetchHostedSessions()
  gameSessionStore.fetchPlayedSessions()
})
</script>

<script lang="ts">
export default {
  name: 'HistoryPage'
}
</script>

<style scoped lang="scss"></style>
