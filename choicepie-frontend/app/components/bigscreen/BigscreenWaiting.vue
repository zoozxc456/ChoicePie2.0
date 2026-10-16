<template>
  <div class="flex-1 flex flex-col items-center justify-center text-center gap-8">
    <div>
      <p class="text-xl mb-4 text-white/60">
        {{ t('bigscreen.waiting.scanQR') }}
      </p>
      <div class="rounded-2xl p-4 inline-block bg-white">
        <img
          :src="qrCodeSrc"
          alt="QR Code"
          width="260"
          height="260"
        >
      </div>
    </div>
    <div>
      <p class="text-lg mb-2 text-white/50">
        {{ t('bigscreen.waiting.orEnterCode') }}
      </p>
      <p
        data-testid="room-code"
        class="text-8xl font-black tracking-[0.2em] text-cp-primary"
      >
        {{ code }}
      </p>
      <p class="text-lg mt-2 text-white/40">
        {{ t('bigscreen.waiting.joinUrl') }}
      </p>
    </div>
    <p
      data-testid="player-count"
      class="text-2xl font-bold text-white/70"
    >
      {{ t('bigscreen.waiting.playerCount', { count: playerCount }) }}
    </p>
  </div>
</template>

<script setup lang="ts">
interface Props {
  code: string
  /** 玩家掃描 QR Code 後開啟的完整加入網址 */
  joinUrl: string
  playerCount: number
}

const props = defineProps<Props>()

const { t } = useI18n()

const qrCodeSrc = computed(() =>
  `https://api.qrserver.com/v1/create-qr-code/?size=260x260&data=${encodeURIComponent(props.joinUrl)}&bgcolor=ffffff&color=2d3748`
)
</script>

<script lang="ts">
export default {
  name: 'BigscreenWaiting'
}
</script>

<style scoped lang="scss"></style>
