<template>
  <div class="flex-1 flex flex-col items-center justify-center">
    <p class="text-2xl font-bold mb-10 text-white/60">
      {{ t('bigscreen.ended.title') }}
    </p>

    <!-- Podium -->
    <div class="flex items-end justify-center gap-6 mb-12">
      <div
        v-for="slot in podium"
        :key="slot.entry.nickname"
        data-testid="podium-slot"
        :data-place="slot.place"
        class="flex flex-col items-center gap-3"
      >
        <UIcon
          name="i-lucide-medal"
          class="text-4xl"
          :class="rankTextClass(slot.place)"
        />
        <p class="text-xl font-bold text-white">
          {{ slot.entry.nickname }}
        </p>
        <p class="font-bold text-cp-primary">
          {{ slot.entry.score.toLocaleString() }}
        </p>
        <div
          data-testid="podium-bar"
          class="w-36 rounded-t-2xl flex items-start justify-center pt-3 text-white text-4xl font-black"
          :class="rankColorClass(slot.place)"
          :style="{ height: PODIUM_HEIGHTS[slot.place] }"
        >
          {{ slot.place }}
        </div>
      </div>
    </div>

    <!-- 未上頒獎台的玩家（含同分並列而擠不上台的） -->
    <div class="flex gap-6 flex-wrap justify-center">
      <div
        v-for="entry in others"
        :key="entry.nickname"
        data-testid="other-player"
        class="text-center"
      >
        <p
          data-testid="other-rank"
          class="text-sm text-white/40"
        >
          {{ t('bigscreen.ended.place', { rank: entry.rank }) }}
        </p>
        <p
          data-testid="other-nickname"
          class="font-bold text-white"
        >
          {{ entry.nickname }}
        </p>
        <p class="text-sm text-cp-primary">
          {{ entry.score.toLocaleString() }}
        </p>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { buildPodium, rankColorClass, rankTextClass, type PodiumPlace } from '~/utils/podium'
import type { RankEntry } from '~/types/other'

interface Props {
  rankings: readonly RankEntry[]
}

const props = defineProps<Props>()

const { t } = useI18n()

const PODIUM_HEIGHTS: Record<PodiumPlace, string> = { 1: '140px', 2: '100px', 3: '70px' }

const podium = computed(() => buildPodium(props.rankings))
const others = computed(() => {
  const onPodium = new Set(podium.value.map(slot => slot.entry))
  return props.rankings.filter(entry => !onPodium.has(entry))
})
</script>

<script lang="ts">
export default {
  name: 'BigscreenEnded'
}
</script>

<style scoped lang="scss"></style>
