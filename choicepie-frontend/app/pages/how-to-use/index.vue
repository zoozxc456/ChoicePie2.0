<template>
  <div class="max-w-4xl mx-auto px-6 py-12">
    <!-- Header -->
    <div class="text-center mb-14">
      <h1 class="text-4xl font-black mb-3">
        {{ t('howToUse.title') }}
      </h1>
      <p
        class="text-lg"
        style="color: var(--cp-text-secondary);"
      >
        {{ t('howToUse.subtitle') }}
      </p>
    </div>

    <!-- Role Toggle -->
    <div class="flex justify-center mb-12">
      <div
        class="flex rounded-xl p-1 gap-1"
        style="background: var(--cp-surface-muted); border: 1px solid var(--cp-border);"
      >
        <button
          v-for="role in roles"
          :key="role.key"
          class="px-6 py-2 rounded-lg text-sm font-semibold transition-all"
          :style="activeRole === role.key
            ? 'background: white; color: var(--cp-primary); box-shadow: var(--cp-shadow-sm);'
            : 'color: var(--cp-text-secondary);'"
          @click="activeRole = role.key"
        >
          {{ role.label }}
        </button>
      </div>
    </div>

    <!-- Host Steps -->
    <template v-if="activeRole === 'host'">
      <div class="flex flex-col gap-6 mb-14">
        <div
          v-for="(step, i) in hostSteps"
          :key="i"
          class="flex gap-6 items-start p-6 rounded-2xl bg-white"
          style="border: 1px solid var(--cp-border);"
        >
          <div
            class="w-12 h-12 rounded-xl flex items-center justify-center text-xl font-black text-white shrink-0"
            style="background: var(--cp-primary);"
          >
            {{ i + 1 }}
          </div>
          <div class="flex-1">
            <h3 class="font-bold text-base mb-1">
              {{ step.title }}
            </h3>
            <p
              class="text-sm leading-relaxed"
              style="color: var(--cp-text-secondary);"
            >
              {{ step.desc }}
            </p>
          </div>
          <div class="text-3xl shrink-0">
            {{ step.icon }}
          </div>
        </div>
      </div>

      <div class="text-center">
        <NuxtLink to="/library/new">
          <UButton
            size="lg"
            color="primary"
          >
            {{ t('howToUse.hostCta') }}
          </UButton>
        </NuxtLink>
      </div>
    </template>

    <!-- Player Steps -->
    <template v-else>
      <div class="flex flex-col gap-6 mb-14">
        <div
          v-for="(step, i) in playerSteps"
          :key="i"
          class="flex gap-6 items-start p-6 rounded-2xl bg-white"
          style="border: 1px solid var(--cp-border);"
        >
          <div
            class="w-12 h-12 rounded-xl flex items-center justify-center text-xl font-black text-white shrink-0"
            style="background: var(--cp-secondary);"
          >
            {{ i + 1 }}
          </div>
          <div class="flex-1">
            <h3 class="font-bold text-base mb-1">
              {{ step.title }}
            </h3>
            <p
              class="text-sm leading-relaxed"
              style="color: var(--cp-text-secondary);"
            >
              {{ step.desc }}
            </p>
          </div>
          <div class="text-3xl shrink-0">
            {{ step.icon }}
          </div>
        </div>
      </div>

      <div class="text-center">
        <NuxtLink to="/join">
          <UButton
            size="lg"
            color="neutral"
            variant="outline"
          >
            {{ t('howToUse.playerCta') }}
          </UButton>
        </NuxtLink>
      </div>
    </template>

    <!-- Scoring -->
    <div class="mt-16 mb-14">
      <h2 class="text-2xl font-bold text-center mb-8">
        {{ t('howToUse.scoring.title') }}
      </h2>
      <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
        <div
          v-for="rule in scoringRules"
          :key="rule.time"
          class="rounded-2xl p-5 text-center bg-white"
          style="border: 1px solid var(--cp-border);"
        >
          <p
            class="text-3xl font-black mb-1"
            style="color: var(--cp-primary);"
          >
            {{ rule.points.toLocaleString() }}
          </p>
          <p
            class="text-xs font-semibold mb-2"
            style="color: var(--cp-text-muted);"
          >
            {{ t('howToUse.scoring.unit') }}
          </p>
          <p class="text-sm font-medium">
            {{ rule.time }}
          </p>
        </div>
      </div>
      <p
        class="text-center text-sm mt-4"
        style="color: var(--cp-text-muted);"
      >
        {{ t('howToUse.scoring.footer') }}
      </p>
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
          class="rounded-xl bg-white overflow-hidden group"
          style="border: 1px solid var(--cp-border);"
        >
          <summary class="flex items-center justify-between px-5 py-4 cursor-pointer font-semibold text-sm select-none list-none">
            {{ faq.q }}
            <UIcon
              name="i-lucide-chevron-down"
              class="transition-transform group-open:rotate-180"
              style="color: var(--cp-text-muted);"
            />
          </summary>
          <div
            class="px-5 pb-4 text-sm leading-relaxed"
            style="color: var(--cp-text-secondary); border-top: 1px solid var(--cp-border);"
          >
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
definePageMeta({ layout: 'default' })

const { t, tm, rt } = useI18n()

const activeRole = ref<'host' | 'player'>('host')

type RoleKey = 'host' | 'player'
const roles = computed(() => [
  { key: 'host' as RoleKey, label: t('howToUse.roles.host') },
  { key: 'player' as RoleKey, label: t('howToUse.roles.player') }
])

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
</script>

<script lang="ts">
export default {
  name: 'HowToUsePage'
}
</script>

<style scoped lang="scss">
</style>
