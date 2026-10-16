<template>
  <div class="flex items-end justify-center gap-4">
    <div
      v-for="slot in podium"
      :key="slot.entry.nickname"
      data-testid="podium-slot"
      :data-place="slot.place"
      class="flex flex-col items-center gap-2"
      :class="{ 'podium-item--animated': animated }"
      :style="animated ? { animationDelay: `${slot.order * 140}ms` } : undefined"
    >
      <div
        data-testid="podium-medal"
        class="rounded-full flex items-center justify-center text-white"
        :class="[sizeClass.medal, rankColorClass(slot.place), slot.place === 1 ? rankGlowClass(1) : '']"
      >
        <UIcon
          name="i-lucide-medal"
          :class="sizeClass.icon"
        />
      </div>
      <p
        data-testid="podium-nickname"
        class="font-bold text-sm truncate max-w-24"
        :class="isHighlighted(slot.entry.nickname) ? 'text-cp-primary' : ''"
      >
        {{ slot.entry.nickname }}{{ isHighlighted(slot.entry.nickname) ? highlightSuffix : '' }}
      </p>
      <p
        data-testid="podium-score"
        class="text-xs text-cp-text-muted"
      >
        {{ slot.entry.score.toLocaleString() }}
      </p>
      <div
        class="flex items-end justify-center"
        :class="sizeClass.bar"
        :style="{ height: sizeClass.heights[slot.place] }"
      >
        <div
          data-testid="podium-bar"
          class="rounded-t-xl h-full flex items-end justify-center pb-2 text-white font-black"
          :class="[
            sizeClass.bar,
            sizeClass.barText,
            rankColorClass(slot.place),
            animated ? 'podium-bar--animated' : '',
            animated && barsGrown ? 'podium-bar--grown' : ''
          ]"
          :style="animated ? { transitionDelay: `${slot.order * 140 + 120}ms` } : undefined"
        >
          {{ slot.place }}
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { buildPodium, rankColorClass, rankGlowClass, type PodiumPlace } from '~/utils/podium'

interface PodiumEntry {
  rank: number
  nickname: string
  score: number
}

interface Props {
  rankings: readonly PodiumEntry[]
  size?: 'sm' | 'md'
  /** 進場動畫：名次彈出 + 柱子長高 */
  animated?: boolean
  highlightNickname?: string
  highlightSuffix?: string
}

const props = withDefaults(defineProps<Props>(), {
  size: 'md',
  animated: false,
  highlightNickname: undefined,
  highlightSuffix: ''
})

const SIZE_CLASSES: Record<NonNullable<Props['size']>, {
  medal: string
  icon: string
  bar: string
  barText: string
  heights: Record<PodiumPlace, string>
}> = {
  sm: { medal: 'w-16 h-16', icon: 'text-2xl', bar: 'w-20', barText: 'text-base', heights: { 1: '70px', 2: '52px', 3: '36px' } },
  md: { medal: 'w-20 h-20', icon: 'text-3xl', bar: 'w-24', barText: 'text-lg', heights: { 1: '80px', 2: '60px', 3: '40px' } }
}

const podium = computed(() => buildPodium(props.rankings))
const sizeClass = computed(() => SIZE_CLASSES[props.size])

const isHighlighted = (nickname: string) => !!props.highlightNickname && nickname === props.highlightNickname

// 柱子高度用 transform: scaleY 做進場動畫；需等瀏覽器先畫出 scaleY(0) 的初始狀態，
// 巢狀兩層 rAF 避免與掛載同一幀導致 transition 被跳過
const barsGrown = ref(false)
onMounted(() => {
  if (!props.animated) return
  requestAnimationFrame(() => {
    requestAnimationFrame(() => {
      barsGrown.value = true
    })
  })
})
</script>

<script lang="ts">
export default {
  name: 'ResultPodium'
}
</script>

<style scoped lang="scss">
@keyframes podium-pop {
  from { opacity: 0; transform: translateY(16px) scale(0.85); }
  to { opacity: 1; transform: translateY(0) scale(1); }
}
.podium-item--animated {
  animation: podium-pop 480ms cubic-bezier(0.34, 1.56, 0.64, 1) both;
}

// 用 transform: scaleY 而非 height 做進場動畫：height transition 在 flex 版面中
// 每一幀都會觸發整行 reflow；scale 走合成層，不觸發版面重算
.podium-bar--animated {
  transform-origin: bottom;
  transform: scaleY(0);
  will-change: transform;
  transition: transform 1s ease-out;
}
.podium-bar--grown {
  transform: scaleY(1);
}

@media (prefers-reduced-motion: reduce) {
  .podium-item--animated {
    animation: none;
  }
  .podium-bar--animated {
    transition: none;
    transform: none;
  }
}
</style>
