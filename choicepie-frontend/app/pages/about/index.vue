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
    <div class="bg-[linear-gradient(135deg,#1a1a2e_0%,#2d3748_100%)] text-white">
      <div class="max-w-3xl mx-auto px-6 py-16">
        <p class="text-xs font-bold tracking-wider mb-5 text-white/40">
          {{ t('about.story.label') }}
        </p>
        <p class="text-xl md:text-2xl font-bold leading-relaxed mb-6">
          {{ t('about.story.p1') }}
        </p>
        <p class="text-base leading-relaxed text-white/70">
          {{ t('about.story.p2') }}
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
          <div class="text-3xl mb-4">
            {{ t(`about.scenarios.${scenario.key}.icon`) }}
          </div>
          <h3 class="font-bold text-base mb-2 leading-snug">
            {{ t(`about.scenarios.${scenario.key}.title`) }}
          </h3>
          <p class="text-sm leading-relaxed text-cp-text-secondary">
            {{ t(`about.scenarios.${scenario.key}.desc`) }}
          </p>
        </div>
      </div>
    </div>

    <!-- Belief：品牌宣言，大字級無邊框，成為視覺焦點而非又一個方塊 -->
    <div class="bg-cp-primary-light">
      <div class="max-w-3xl mx-auto px-6 py-16 text-center">
        <p class="text-2xl md:text-3xl font-black leading-snug text-cp-primary mb-3">
          {{ t('about.belief.text') }}
        </p>
        <p class="text-base text-cp-text-secondary">
          {{ t('about.belief.subtext') }}
        </p>
      </div>
    </div>

    <!-- VMV：願景與使命並列對照，取代單一 mission 方塊 -->
    <div class="max-w-4xl mx-auto px-6 py-16">
      <div class="grid grid-cols-1 md:grid-cols-2 gap-5">
        <div class="rounded-2xl p-7 bg-white border-2 border-cp-primary/25">
          <p class="text-xs font-bold tracking-wider mb-3 text-cp-primary">
            {{ t('about.vmv.vision.label') }}
          </p>
          <p class="text-base leading-relaxed font-semibold">
            {{ t('about.vmv.vision.text') }}
          </p>
        </div>
        <div class="rounded-2xl p-7 bg-white border-2 border-cp-secondary/20">
          <p class="text-xs font-bold tracking-wider mb-3 text-cp-secondary">
            {{ t('about.vmv.mission.label') }}
          </p>
          <p class="text-base leading-relaxed font-semibold">
            {{ t('about.vmv.mission.text') }}
          </p>
        </div>
      </div>
    </div>

    <!-- Values -->
    <div class="max-w-4xl mx-auto px-6 py-16">
      <h2 class="text-xl font-bold text-center mb-8">
        {{ t('about.values.title') }}
      </h2>
      <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
        <div
          v-for="value in values"
          :key="value.key"
          class="flex gap-4 rounded-xl p-5 bg-white border border-cp-border"
        >
          <UIcon
            :name="value.icon"
            class="shrink-0 text-2xl text-cp-primary mt-0.5"
          />
          <div>
            <h3 class="font-bold text-sm mb-1">
              {{ t(`about.values.${value.key}.title`) }}
            </h3>
            <p class="text-sm leading-relaxed text-cp-text-secondary">
              {{ t(`about.values.${value.key}.desc`) }}
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
  { key: 'history' },
  { key: 'exam' }
]

const values = [
  { key: 'userCentric', icon: 'i-lucide-heart-handshake' },
  { key: 'funLearning', icon: 'i-lucide-party-popper' },
  { key: 'createShare', icon: 'i-lucide-sparkles' },
  { key: 'growth', icon: 'i-lucide-trending-up' }
]
</script>

<script lang="ts">
export default {
  name: 'AboutPage'
}
</script>

<style scoped lang="scss">
</style>
