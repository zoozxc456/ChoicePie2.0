<template>
  <div class="text-center mb-6">
    <div class="text-5xl mb-3">
      🥧
    </div>
    <h1 class="text-xl font-bold mb-1">
      {{ t('room.waiting.title') }}
    </h1>
  </div>

  <div
    class="flex flex-col gap-4 rounded-2xl p-5 bg-white mb-4 border border-neutral-200"
  >
    <div class="text-center">
      <p class="text-xs font-semibold mb-1 text-neutral-400">
        {{ t('room.waiting.yourNickname') }}
      </p>
      <p class="text-2xl font-bold text-primary-500">
        {{ gameStore.myNickname }}
      </p>
    </div>
    <div class="text-center">
      <p class="text-xs font-semibold mb-1 text-neutral-400">
        {{ t('room.waiting.roomCode') }}
      </p>
      <p class="text-xl font-bold tracking-widest text-secondary-800">
        {{ props.code }}
      </p>
    </div>

    <!-- Player list -->
    <div>
      <p class="text-xs font-semibold mb-2 text-neutral-400">
        {{ t('room.waiting.playerCount', { count: gameStore.playerCount }) }}
      </p>
      <div class="flex flex-wrap gap-2">
        <div
          v-for="player in gameStore.players"
          :key="player.id"
          class="player-chip"
          :class="player.nickname === gameStore.myNickname
            ? 'bg-primary-100 border-primary-200'
            : ''"
        >
          <div class="online-dot" />
          <span
            :class="player.nickname === gameStore.myNickname ? 'text-primary-500 font-semibold' : ''"
          >
            {{ player.nickname }}
          </span>
        </div>
      </div>
    </div>

    <div class="flex items-center justify-center gap-2 text-sm text-neutral-400">
      <UIcon
        name="i-lucide-loader-2"
        class="animate-spin"
      />
      {{ t('room.waiting.waitingHost') }}
    </div>
  </div>
</template>

<script lang="ts" setup>
interface Props {
  code: string
}

const { t } = useI18n()
const props = defineProps<Props>()
const gameStore = useGameStore()
</script>

<style scoped lang="scss"></style>
