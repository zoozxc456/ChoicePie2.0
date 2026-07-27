<template>
  <div class="flex items-center justify-between mb-6">
    <div>
      <h1 class="text-2xl font-bold mb-1">
        {{ t('host.waiting.title') }}
      </h1>
      <p class="text-sm text-cp-text-secondary">
        {{ t('host.waiting.subtitle') }}
      </p>
    </div>
    <NuxtLink
      :to="bigscreenUrl"
      target="_blank"
      class="flex items-center gap-2 text-sm font-medium px-4 py-2 rounded-lg border-[1.5px] border-cp-border text-cp-text-secondary"
    >
      {{ t('host.waiting.bigscreen') }}
    </NuxtLink>
  </div>

  <div class="grid grid-cols-[1fr_280px] gap-6">
    <!-- Left: Room info + Players -->
    <div class="flex flex-col gap-4">
      <!-- Room Code -->
      <div class="rounded-2xl p-6 bg-white border border-cp-border">
        <p class="text-xs font-semibold mb-2 text-cp-text-muted">
          {{ t('host.waiting.roomCode') }}
        </p>
        <div class="room-code">
          {{ code }}
        </div>
        <p class="text-xs mt-2 text-cp-text-muted">
          {{ joinUrl }}
        </p>
      </div>

      <!-- Player List -->
      <div class="rounded-2xl p-6 bg-white flex-1 border border-cp-border">
        <div class="flex items-center justify-between mb-4">
          <p class="text-sm font-semibold">
            {{ t('host.waiting.joinedPlayers') }}
          </p>
          <span class="text-lg font-bold text-cp-primary">{{ t('host.waiting.playerCount', { count: gameStore.playerCount }) }}</span>
        </div>

        <TransitionGroup
          name="player-list"
          tag="div"
          class="flex flex-wrap gap-2"
        >
          <div
            v-for="player in gameStore.players"
            :key="player.id"
            class="player-chip"
          >
            <div class="online-dot" />
            {{ player.nickname }}
          </div>
        </TransitionGroup>

        <div
          v-if="gameStore.playerCount === 0"
          class="text-center py-8 text-sm text-cp-text-muted"
        >
          {{ t('host.waiting.noPlayers') }}
        </div>
      </div>

      <!-- Actions -->
      <UButton
        block
        size="lg"
        color="primary"
        :disabled="gameStore.playerCount === 0"
        @click="handleStart"
      >
        {{ t('host.waiting.startGame', { count: gameStore.playerCount }) }}
      </UButton>
      <UButton
        block
        size="md"
        color="neutral"
        variant="ghost"
      >
        {{ t('host.waiting.waitMore') }}
      </UButton>
    </div>

    <!-- Right: QR Code -->
    <div class="flex flex-col items-center justify-start pt-2">
      <div class="rounded-2xl p-5 bg-white w-full flex flex-col items-center border border-cp-border">
        <div class="rounded-xl overflow-hidden mb-3 bg-cp-secondary p-3">
          <img
            :src="qrUrl"
            alt="QR Code"
            width="200"
            height="200"
            class="block rounded"
          >
        </div>
        <p class="text-sm font-semibold mb-1">
          {{ t('host.waiting.scanToJoin') }}
        </p>
        <p class="text-xs text-cp-text-muted">
          choicepie.app/join
        </p>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
interface Props {
  code: string
}

const props = defineProps<Props>()
const { t } = useI18n()
const gameStore = useGameStore()
const gameRoom = useGameRoom()

const origin = useRequestURL().origin
const joinUrl = computed(() => `${origin}/join?code=${props.code}`)
const qrUrl = computed(() =>
  `https://api.qrserver.com/v1/create-qr-code/?size=240x240&data=${encodeURIComponent(joinUrl.value)}&bgcolor=ffffff&color=2d3748`
)

const bigscreenUrl = computed(() => `/bigscreen/${props.code}`)

const handleStart = () => {
  gameRoom.startGame(props.code)
}
</script>

<style scoped lang="scss">
.player-list-enter-active { transition: all 0.3s ease; }
.player-list-enter-from { opacity: 0; transform: scale(0.8); }
</style>
