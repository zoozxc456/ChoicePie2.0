<template>
  <div class="flex flex-col min-h-screen p-8">
    <HostOfflineBanner large />

    <!-- Header -->
    <div class="flex items-center justify-between mb-6">
      <div class="flex items-center gap-3">
        <span class="text-2xl font-bold text-cp-primary">🥧 ChoicePie</span>
        <span class="px-3 py-1 rounded-full text-sm font-bold tracking-widest bg-white/10">
          {{ code }}
        </span>
      </div>
      <div class="flex items-center gap-2 text-sm text-white/50">
        <span>{{ t('common.players', { count: gameStore.playerCount }) }}</span>
      </div>
    </div>

    <p
      v-if="watchFailed"
      class="flex-1 flex items-center justify-center text-2xl text-white/60 text-center"
    >
      {{ t('bigscreen.unavailable') }}
    </p>
    <BigscreenWaiting
      v-else-if="gameStore.phase === 'waiting' || gameStore.phase === 'idle'"
      :code="code"
      :join-url="joinUrl"
      :player-count="gameStore.playerCount"
    />
    <BigscreenQuestion v-else-if="gameStore.phase === 'question' || gameStore.phase === 'result'" />
    <BigscreenEnded
      v-else-if="gameStore.phase === 'ended'"
      :rankings="gameStore.rankings"
    />
  </div>
</template>

<script setup lang="ts">
import BigscreenEnded from '~/components/bigscreen/BigscreenEnded.vue'
import BigscreenQuestion from '~/components/bigscreen/BigscreenQuestion.vue'
import BigscreenWaiting from '~/components/bigscreen/BigscreenWaiting.vue'
import HostOfflineBanner from '~/components/gameRoom/HostOfflineBanner.vue'

definePageMeta({ layout: 'big-screen' })

const { t } = useI18n()
const route = useRoute()
const code = computed(() => (route.params.code as string).toUpperCase())
const origin = useRequestURL().origin
const joinUrl = computed(() => `${origin}/join?code=${code.value}`)

const gameStore = useGameStore()
const gameRoom = useGameRoom()

const watchFailed = ref(false)

onMounted(async () => {
  try {
    // 大螢幕是匿名旁觀者：連線後必須 WatchRoom 才會進入房間群組、收到廣播與目前階段快照
    await gameRoom.watchRoom(code.value)
  } catch {
    watchFailed.value = true
  }
})

onUnmounted(() => gameRoom.disconnect())
</script>

<script lang="ts">
export default {
  name: 'BigscreenPage'
}
</script>

<style scoped lang="scss"></style>
