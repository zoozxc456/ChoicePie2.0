<template>
  <div>
    <!-- Hero -->
    <div class="max-w-4xl mx-auto px-6 pt-16 pb-14 text-center">
      <span class="inline-block text-base font-bold tracking-wide mb-4 text-cp-primary">
        {{ t('about.hero.eyebrow') }}
      </span>
      <h1 class="text-3xl md:text-4xl font-black leading-tight mb-4">
        {{ t('about.hero.title') }}
      </h1>
      <p class="text-lg text-cp-text-secondary">
        {{ t('about.hero.desc') }}
      </p>
    </div>

    <!-- Story: 沉浸式深色區塊，與首頁 Hero/ProductHighlights 的深色語彙呼應 -->
    <div class="bg-cp-secondary text-white">
      <div class="max-w-3xl mx-auto px-6 py-16">
        <p class="text-xs font-bold tracking-wider mb-5 text-white/40">
          {{ t('about.story.label') }}
        </p>
        <p class="text-xl md:text-2xl font-bold leading-relaxed mb-6">
          {{ t('about.story.p1') }}
        </p>
        <p class="text-base leading-relaxed text-white/70 mb-6">
          {{ t('about.story.p2') }}
        </p>
        <p class="text-base leading-relaxed text-white/70 border-t border-white/10 pt-6">
          {{ t('about.story.p3') }}
        </p>
      </div>
    </div>

    <!-- Scenarios：兩個具體情境卡片，取代原本抽象的條列原則 -->
    <div class="max-w-4xl mx-auto px-6 py-16">
      <h2 class="text-xl font-bold text-center mb-8">
        {{ t('about.scenarios.title') }}
      </h2>
      <div class="grid grid-cols-1 md:grid-cols-2 gap-5">
        <div
          v-for="scenario in scenarios"
          :key="scenario.key"
          class="rounded-2xl p-6 bg-white border border-cp-border"
        >
          <UIcon
            :name="scenario.icon"
            class="text-3xl text-cp-primary mb-4"
          />
          <h3 class="font-bold text-base mb-2 leading-snug">
            {{ t(`about.scenarios.${scenario.key}.title`) }}
          </h3>
          <p class="text-sm leading-relaxed text-cp-text-secondary">
            {{ t(`about.scenarios.${scenario.key}.desc`) }}
          </p>
        </div>
      </div>
    </div>

    <!-- Facts：具體、可驗證的產品事實，取代抽象美德詞 -->
    <div class="max-w-4xl mx-auto px-6 py-16">
      <h2 class="text-xl font-bold text-center mb-8">
        {{ t('about.facts.title') }}
      </h2>
      <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <div
          v-for="fact in facts"
          :key="fact.key"
          class="flex gap-4 rounded-xl p-5 bg-white border border-cp-border"
        >
          <UIcon
            :name="fact.icon"
            class="shrink-0 text-2xl text-cp-primary mt-0.5"
          />
          <div>
            <h3 class="font-bold text-sm mb-1">
              {{ t(`about.facts.${fact.key}.title`) }}
            </h3>
            <p class="text-sm leading-relaxed text-cp-text-secondary">
              {{ t(`about.facts.${fact.key}.desc`) }}
            </p>
          </div>
        </div>
      </div>
    </div>

    <!-- CTA -->
    <div class="max-w-3xl mx-auto px-6 pt-4 pb-20 text-center">
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
definePageMeta({ layout: 'content' })

const { t } = useI18n()

const scenarios = [
  { key: 'history', icon: 'i-lucide-book-open' },
  { key: 'exam', icon: 'i-lucide-target' }
]

const facts = [
  { key: 'freeToCreate', icon: 'i-lucide-pencil-line' },
  { key: 'livePlayers', icon: 'i-lucide-users' },
  { key: 'hostPaced', icon: 'i-lucide-timer' },
  { key: 'soloReview', icon: 'i-lucide-repeat' }
]
</script>

<script lang="ts">
export default {
  name: 'AboutPage'
}
</script>

<style scoped lang="scss">
</style>
