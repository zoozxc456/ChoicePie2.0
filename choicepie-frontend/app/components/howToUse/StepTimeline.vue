<template>
  <div class="relative">
    <!-- 貫穿全程的時間軸線 -->
    <div
      class="absolute left-6.75 top-2 bottom-2 w-0.5 bg-cp-border"
      aria-hidden="true"
    />

    <div class="flex flex-col gap-3">
      <div
        v-for="(step, i) in steps"
        :key="step.title"
        ref="stepRefs"
        data-testid="step"
        class="step-reveal relative flex gap-6"
        :class="{ 'step-reveal--visible': visibleSteps.has(i) }"
        :style="{ transitionDelay: `${i * 90}ms` }"
      >
        <!-- 節點 -->
        <div
          data-testid="step-number"
          class="relative z-10 shrink-0 rounded-full flex items-center justify-center font-black text-white transition-all"
          :class="[i === 0 ? 'w-14 h-14 text-2xl' : 'w-8 h-8 text-sm mt-3', ACCENT_CLASSES[accent].node]"
        >
          {{ i + 1 }}
        </div>

        <!-- 內容：第一步放大成主視覺，其餘縮小呈現節奏感 -->
        <div
          v-if="i === 0"
          data-testid="step-featured"
          class="flex-1 rounded-2xl p-7 bg-white border-2 shadow-cp-lg mb-2"
          :class="ACCENT_CLASSES[accent].border"
        >
          <span
            class="inline-block text-xs font-bold px-2.5 py-1 rounded-full mb-3"
            :class="ACCENT_CLASSES[accent].badge"
          >
            {{ firstBadge }}
          </span>
          <h3 class="font-black text-xl mb-2">
            {{ step.title }}
          </h3>
          <p class="text-sm leading-relaxed text-cp-text-secondary">
            {{ step.desc }}
          </p>
        </div>
        <div
          v-else
          data-testid="step-compact"
          class="flex-1 rounded-xl px-5 py-3.5 bg-white border border-cp-border"
        >
          <h3 class="font-bold text-sm mb-0.5">
            {{ step.title }}
          </h3>
          <p class="text-xs leading-relaxed text-cp-text-secondary">
            {{ step.desc }}
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
interface TimelineStep {
  title: string
  desc: string
}

interface Props {
  steps: TimelineStep[]
  /** 第一步上方的徽章文字 */
  firstBadge: string
  accent?: 'primary' | 'secondary'
}

const props = withDefaults(defineProps<Props>(), {
  accent: 'primary'
})

const ACCENT_CLASSES = {
  primary: { node: 'bg-cp-primary', border: 'border-cp-primary/30', badge: 'bg-cp-primary-light text-cp-primary' },
  secondary: { node: 'bg-cp-secondary', border: 'border-cp-secondary/30', badge: 'bg-cp-surface-muted text-cp-secondary' }
} as const

// 步驟隨滾動進場：進入可視範圍即標記為 visible，觸發 CSS 進場動畫
const stepRefs = ref<HTMLElement[]>([])
const visibleSteps = ref<Set<number>>(new Set())
let observer: IntersectionObserver | null = null

const observeSteps = () => {
  observer?.disconnect()
  if (typeof IntersectionObserver === 'undefined') {
    visibleSteps.value = new Set(props.steps.map((_, i) => i))
    return
  }
  visibleSteps.value = new Set()
  observer = new IntersectionObserver((entries) => {
    entries.forEach((entry) => {
      if (!entry.isIntersecting) return
      const index = stepRefs.value.indexOf(entry.target as HTMLElement)
      if (index !== -1) visibleSteps.value = new Set(visibleSteps.value).add(index)
    })
  }, { threshold: 0.2 })
  stepRefs.value.forEach(el => observer!.observe(el))
}

onMounted(() => {
  nextTick(observeSteps)
})

watch(() => props.steps, () => {
  nextTick(observeSteps)
})

onUnmounted(() => {
  observer?.disconnect()
})
</script>

<script lang="ts">
export default {
  name: 'StepTimeline'
}
</script>

<style scoped lang="scss">
.step-reveal {
  opacity: 0;
  transform: translateY(18px);
  transition: opacity 520ms ease, transform 520ms cubic-bezier(0.22, 1, 0.36, 1);
}
.step-reveal--visible {
  opacity: 1;
  transform: translateY(0);
}

@media (prefers-reduced-motion: reduce) {
  .step-reveal {
    opacity: 1;
    transform: none;
    transition: none;
  }
}
</style>
