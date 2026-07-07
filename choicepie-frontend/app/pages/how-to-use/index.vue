<template>
  <div class="max-w-4xl mx-auto px-6 py-12">
    <!-- Header -->
    <div class="text-center mb-14">
      <h1 class="text-4xl font-black mb-3">
        {{ t('howToUse.title') }}
      </h1>
      <p class="text-lg text-cp-text-secondary">
        {{ t('howToUse.subtitle') }}
      </p>
    </div>

    <!-- Role Toggle -->
    <div class="flex justify-center mb-14">
      <div class="flex rounded-xl p-1 gap-1 bg-cp-surface-muted border border-cp-border">
        <button
          v-for="role in roles"
          :key="role.key"
          class="px-6 py-2 rounded-lg text-sm font-semibold transition-all"
          :class="activeRole === role.key
            ? 'bg-white text-cp-primary shadow-cp-sm'
            : 'text-cp-text-secondary'"
          @click="activeRole = role.key"
        >
          {{ role.label }}
        </button>
      </div>
    </div>

    <!-- Steps timeline -->
    <div class="relative mb-16">
      <!-- 貫穿全程的時間軸線 -->
      <div
        class="absolute left-6.75 top-2 bottom-2 w-0.5 bg-cp-border"
        aria-hidden="true"
      />

      <div class="flex flex-col gap-3">
        <div
          v-for="(step, i) in activeSteps"
          :key="step.title"
          ref="stepRefs"
          class="step-reveal relative flex gap-6"
          :class="{ 'step-reveal--visible': visibleSteps.has(i) }"
          :style="{ transitionDelay: `${i * 90}ms` }"
        >
          <!-- 節點 -->
          <div
            class="relative z-10 shrink-0 rounded-full flex items-center justify-center font-black text-white transition-all"
            :class="[
              i === 0 ? 'w-14 h-14 text-2xl' : 'w-8 h-8 text-sm mt-3',
              roleAccentClass
            ]"
          >
            {{ i + 1 }}
          </div>

          <!-- 內容：第一步放大成主視覺，其餘縮小呈現節奏感 -->
          <div
            v-if="i === 0"
            class="flex-1 rounded-2xl p-7 bg-white border-2 shadow-cp-lg mb-2"
            :class="roleBorderClass"
          >
            <span
              class="inline-block text-xs font-bold px-2.5 py-1 rounded-full mb-3"
              :class="roleBadgeClass"
            >
              {{ t('howToUse.firstStepBadge') }}
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

    <div class="text-center mb-16">
      <NuxtLink :to="activeRole === 'host' ? '/library/new' : '/join'">
        <UButton
          size="lg"
          :color="activeRole === 'host' ? 'primary' : 'neutral'"
          :variant="activeRole === 'host' ? 'solid' : 'outline'"
          class="rounded-full px-7 font-bold"
        >
          {{ activeRole === 'host' ? t('howToUse.hostCta') : t('howToUse.playerCta') }}
        </UButton>
      </NuxtLink>
    </div>

    <!-- Scoring -->
    <div class="mb-16 rounded-3xl p-8 bg-[linear-gradient(135deg,#1a1a2e_0%,#2d3748_100%)] text-white">
      <h2 class="text-2xl font-bold text-center mb-2">
        {{ t('howToUse.scoring.title') }}
      </h2>
      <p class="text-center text-sm text-white/50 mb-8">
        {{ t('howToUse.scoring.footer') }}
      </p>
      <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
        <div
          v-for="(rule, i) in scoringRules"
          :key="rule.time"
          class="rounded-2xl p-5 text-center border transition-transform hover:-translate-y-1"
          :class="i === 0
            ? 'bg-primary-500/15 border-primary-500/40'
            : 'bg-white/5 border-white/10'"
        >
          <p
            class="text-3xl font-black mb-1"
            :class="i === 0 ? 'text-primary-500' : 'text-white'"
          >
            {{ rule.points.toLocaleString() }}
          </p>
          <p class="text-xs font-semibold mb-2 text-white/40">
            {{ t('howToUse.scoring.unit') }}
          </p>
          <p class="text-sm font-medium text-white/70">
            {{ rule.time }}
          </p>
        </div>
      </div>
    </div>

    <!-- FAQ -->
    <div>
      <h2 class="text-2xl font-bold text-center mb-8">
        {{ t('howToUse.faq.title') }}
      </h2>
      <div class="flex flex-col gap-3">
        <details
          v-for="faq in faqs"
          :key="faq.q"
          class="rounded-xl bg-white overflow-hidden group border border-cp-border transition-colors open:border-cp-primary/40"
        >
          <summary class="flex items-center justify-between px-5 py-4 cursor-pointer font-semibold text-sm select-none list-none">
            {{ faq.q }}
            <UIcon
              name="i-lucide-chevron-down"
              class="transition-transform group-open:rotate-180 text-cp-text-muted"
            />
          </summary>
          <div class="px-5 pb-4 text-sm leading-relaxed text-cp-text-secondary border-t border-cp-border">
            <div class="pt-3">
              {{ faq.a }}
            </div>
          </div>
        </details>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'content' })

const { t, tm, rt } = useI18n()

type RoleKey = 'host' | 'player'
const activeRole = ref<RoleKey>('host')

const roles = computed(() => [
  { key: 'host' as RoleKey, label: t('howToUse.roles.host') },
  { key: 'player' as RoleKey, label: t('howToUse.roles.player') }
])

const roleAccentClass = computed(() => (activeRole.value === 'host' ? 'bg-cp-primary' : 'bg-cp-secondary'))
const roleBorderClass = computed(() => (activeRole.value === 'host' ? 'border-cp-primary/30' : 'border-cp-secondary/30'))
const roleBadgeClass = computed(() => (activeRole.value === 'host'
  ? 'bg-cp-primary-light text-cp-primary'
  : 'bg-cp-surface-muted text-cp-secondary'))

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const castArray = (val: unknown): any[] => (Array.isArray(val) ? val : [])

const hostStepIcons = ['\u{1F4DD}', '✨', '\u{1F6AA}', '\u{1F3C1}']
const playerStepIcons = ['\u{1F4F7}', '✏️', '⚡', '\u{1F3C6}']

const hostSteps = computed(() =>
  castArray(tm('howToUse.hostSteps')).map((s, i) => ({
    title: rt(s.title),
    desc: rt(s.desc),
    icon: hostStepIcons[i]
  }))
)

const playerSteps = computed(() =>
  castArray(tm('howToUse.playerSteps')).map((s, i) => ({
    title: rt(s.title),
    desc: rt(s.desc),
    icon: playerStepIcons[i]
  }))
)

const activeSteps = computed(() => (activeRole.value === 'host' ? hostSteps.value : playerSteps.value))

const scoringRules = computed(() =>
  castArray(tm('howToUse.scoring.rules')).map(r => ({
    time: rt(r.time),
    points: r.points as number
  }))
)

const faqs = computed(() =>
  castArray(tm('howToUse.faq.items')).map(f => ({
    q: rt(f.q),
    a: rt(f.a)
  }))
)

// 步驟隨滾動進場：進入可視範圍即標記為 visible，觸發 CSS 進場動畫
const stepRefs = ref<HTMLElement[]>([])
const visibleSteps = ref<Set<number>>(new Set())
let observer: IntersectionObserver | null = null

const observeSteps = () => {
  observer?.disconnect()
  visibleSteps.value = new Set()

  observer = new IntersectionObserver((entries) => {
    entries.forEach((entry) => {
      if (!entry.isIntersecting) return
      const index = stepRefs.value.indexOf(entry.target as HTMLElement)
      if (index !== -1) visibleSteps.value.add(index)
    })
  }, { threshold: 0.2 })

  stepRefs.value.forEach(el => observer!.observe(el))
}

onMounted(() => {
  nextTick(observeSteps)
})

watch(activeRole, () => {
  nextTick(observeSteps)
})

onUnmounted(() => {
  observer?.disconnect()
})
</script>

<script lang="ts">
export default {
  name: 'HowToUsePage'
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
