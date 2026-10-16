<template>
  <div>
    <div
      v-for="(entry, i) in rankings"
      :key="entry.nickname"
      data-testid="rank-row"
      class="rank-row relative flex items-center gap-4 px-5 py-3 overflow-hidden"
      :class="[
        i < rankings.length - 1 ? 'border-b border-cp-border' : '',
        isHighlighted(entry.nickname) ? 'bg-cp-primary-light' : ''
      ]"
      :style="{ animationDelay: `${i * 45}ms` }"
    >
      <div
        data-testid="rank-bar"
        class="rank-row__bar absolute inset-y-0 left-0"
        :class="[rankColorClass(entry.rank), barsGrown ? 'rank-row__bar--grown' : '']"
        :style="{
          width: `${scorePercent(entry.score)}%`,
          transitionDelay: `${i * 45 + 200}ms`
        }"
      />
      <span
        data-testid="rank-badge"
        class="relative w-8 h-8 rounded-full flex items-center justify-center text-xs font-black text-white shrink-0"
        :class="rankColorClass(entry.rank)"
      >
        <UIcon
          v-if="entry.rank <= 3"
          name="i-lucide-medal"
          class="text-sm"
        />
        <template v-else>{{ entry.rank }}</template>
      </span>
      <span
        class="relative flex-1 font-medium text-sm"
        :class="isHighlighted(entry.nickname) ? 'text-cp-primary font-bold' : ''"
      >
        {{ entry.nickname }}{{ isHighlighted(entry.nickname) ? highlightSuffix : '' }}
      </span>
      <span class="relative font-bold tabular-nums text-sm text-cp-primary">
        {{ entry.score.toLocaleString() }}
      </span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { rankColorClass } from '~/utils/podium'

interface RankingEntry {
  rank: number
  nickname: string
  score: number
}

interface Props {
  rankings: readonly RankingEntry[]
  highlightNickname?: string
  highlightSuffix?: string
}

const props = withDefaults(defineProps<Props>(), {
  highlightNickname: undefined,
  highlightSuffix: ''
})

const MIN_BAR_PERCENT = 6

// 以最高分為 100%，不假設陣列已排序
const topScore = computed(() => Math.max(1, ...props.rankings.map(entry => entry.score)))
const scorePercent = (score: number) => Math.max(Math.round((score / topScore.value) * 100), MIN_BAR_PERCENT)

const isHighlighted = (nickname: string) => !!props.highlightNickname && nickname === props.highlightNickname

// 分數條用 transform: scaleX 做進場動畫；巢狀兩層 rAF 確保瀏覽器先畫出 scaleX(0) 的初始狀態
const barsGrown = ref(false)
onMounted(() => {
  requestAnimationFrame(() => {
    requestAnimationFrame(() => {
      barsGrown.value = true
    })
  })
})
</script>

<script lang="ts">
export default {
  name: 'RankingBarList'
}
</script>

<style scoped lang="scss">
@keyframes row-slide-in {
  from { opacity: 0; transform: translateX(-10px); }
  to { opacity: 1; transform: translateX(0); }
}
.rank-row {
  animation: row-slide-in 380ms ease both;
}

.rank-row__bar {
  opacity: 0.12;
  transform-origin: left;
  transform: scaleX(0);
  will-change: transform;
  transition: transform 1s ease-out;
}
.rank-row__bar--grown {
  transform: scaleX(1);
}

@media (prefers-reduced-motion: reduce) {
  .rank-row {
    animation: none;
  }
  .rank-row__bar {
    transition: none;
    transform: none;
  }
}
</style>
