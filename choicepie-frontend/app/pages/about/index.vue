<template>
  <div class="max-w-3xl mx-auto px-6 py-12">
    <!-- Hero -->
    <div class="text-center mb-16">
      <div class="text-7xl mb-5">
        🥧
      </div>
      <h1 class="text-4xl font-black mb-4">
        {{ t('about.hero.title') }}
      </h1>
      <p class="text-lg leading-relaxed text-cp-text-secondary max-w-130 mx-auto">
        {{ t('about.hero.desc') }}
      </p>
    </div>

    <!-- Mission -->
    <div class="rounded-2xl p-8 mb-8 text-center bg-[linear-gradient(135deg,#1a1a2e,#2d3748)] text-white">
      <p class="text-xs font-bold tracking-wider mb-3 text-white/50">
        {{ t('about.mission.label') }}
      </p>
      <p class="text-2xl font-bold leading-relaxed">
        {{ t('about.mission.text') }}
      </p>
    </div>

    <!-- Story -->
    <div class="mb-12">
      <h2 class="text-xl font-bold mb-5">
        {{ t('about.story.title') }}
      </h2>
      <div class="rounded-2xl p-6 bg-white text-sm border border-cp-border text-cp-text-secondary leading-loose">
        <p class="mb-4">
          {{ t('about.story.p1') }}
        </p>
        <p class="mb-4">
          {{ t('about.story.p2') }}
        </p>
        <p class="mb-4">
          {{ t('about.story.p3') }}
        </p>
        <p>
          {{ t('about.story.p4') }}
        </p>
      </div>
    </div>

    <!-- Values -->
    <div class="mb-12">
      <h2 class="text-xl font-bold mb-5">
        {{ t('about.values.title') }}
      </h2>
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <div
          v-for="value in values"
          :key="value.key"
          class="rounded-2xl p-5 bg-white border border-cp-border"
        >
          <div class="text-3xl mb-3">
            {{ value.icon }}
          </div>
          <h3 class="font-bold mb-2">
            {{ t(`about.values.${value.key}.title`) }}
          </h3>
          <p class="text-sm leading-relaxed text-cp-text-secondary">
            {{ t(`about.values.${value.key}.desc`) }}
          </p>
        </div>
      </div>
    </div>

    <!-- Stats -->
    <div class="rounded-2xl p-8 mb-12 bg-cp-primary-light border border-cp-primary-border">
      <h2 class="text-xl font-bold mb-6 text-center">
        {{ t('about.stats.title') }}
      </h2>
      <div class="grid grid-cols-3 gap-6 text-center">
        <div
          v-for="stat in stats"
          :key="stat.key"
        >
          <p class="text-3xl font-black mb-1 text-cp-primary">
            {{ t(`about.stats.${stat.key}.value`) }}
          </p>
          <p class="text-sm text-cp-text-secondary">
            {{ t(`about.stats.${stat.key}.label`) }}
          </p>
        </div>
      </div>
    </div>

    <!-- CTA -->
    <div class="text-center">
      <h2 class="text-2xl font-bold mb-3">
        {{ t('about.cta.title') }}
      </h2>
      <p class="mb-6 text-cp-text-secondary">
        {{ t('about.cta.subtitle') }}
      </p>
      <div class="flex justify-center gap-3">
        <NuxtLink to="/library/new">
          <UButton
            size="lg"
            color="primary"
          >
            {{ t('about.cta.createGame') }}
          </UButton>
        </NuxtLink>
        <NuxtLink to="/how-to-use">
          <UButton
            size="lg"
            color="neutral"
            variant="outline"
          >
            {{ t('about.cta.howToUse') }}
          </UButton>
        </NuxtLink>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'default' })

const { t } = useI18n()

const values = [
  { key: 'fast', icon: '⚡' },
  { key: 'fun', icon: '🎮' },
  { key: 'effective', icon: '🧠' }
]

const stats = [
  { key: 'stage' },
  { key: 'timeline' },
  { key: 'team' }
]
</script>

<script lang="ts">
export default {
  name: 'AboutPage'
}
</script>

<style scoped lang="scss">
</style>
