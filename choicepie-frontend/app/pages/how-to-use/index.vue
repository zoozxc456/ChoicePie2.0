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
      <SegmentedControl
        v-model="activeRole"
        :options="roles"
      />
    </div>

    <StepTimeline
      class="mb-16"
      :steps="activeSteps"
      :first-badge="t('howToUse.firstStepBadge')"
      :accent="activeRole === 'host' ? 'primary' : 'secondary'"
    />

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
    <div class="mb-16 rounded-3xl p-8 bg-cp-secondary text-white">
      <h2 class="text-2xl font-bold text-center mb-2">
        {{ t('howToUse.scoring.title') }}
      </h2>
      <p class="text-center text-sm text-white/50 mb-8">
        {{ t('howToUse.scoring.footer') }}
      </p>
      <ScoringRuleGrid
        :rules="scoringRules"
        :unit-label="t('howToUse.scoring.unit')"
      />
    </div>

    <!-- FAQ -->
    <div>
      <h2 class="text-2xl font-bold text-center mb-8">
        {{ t('howToUse.faq.title') }}
      </h2>
      <FaqList :items="faqs" />
    </div>
  </div>
</template>

<script setup lang="ts">
import FaqList from '~/components/common/FaqList.vue'
import SegmentedControl from '~/components/common/SegmentedControl.vue'
import ScoringRuleGrid from '~/components/howToUse/ScoringRuleGrid.vue'
import StepTimeline from '~/components/howToUse/StepTimeline.vue'

definePageMeta({ layout: 'content' })

const { t, tm, rt } = useI18n()

type RoleKey = 'host' | 'player'
const activeRole = ref<RoleKey>('host')

const roles = computed(() => [
  { value: 'host' as RoleKey, label: t('howToUse.roles.host') },
  { value: 'player' as RoleKey, label: t('howToUse.roles.player') }
])

// eslint-disable-next-line @typescript-eslint/no-explicit-any
const castArray = (val: unknown): any[] => (Array.isArray(val) ? val : [])

const hostSteps = computed(() =>
  castArray(tm('howToUse.hostSteps')).map(s => ({
    title: rt(s.title),
    desc: rt(s.desc)
  }))
)

const playerSteps = computed(() =>
  castArray(tm('howToUse.playerSteps')).map(s => ({
    title: rt(s.title),
    desc: rt(s.desc)
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
</script>

<script lang="ts">
export default {
  name: 'HowToUsePage'
}
</script>

<style scoped lang="scss"></style>
