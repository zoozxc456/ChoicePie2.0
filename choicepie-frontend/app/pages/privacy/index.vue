<template>
  <div class="max-w-3xl mx-auto px-6 py-12">
    <div class="text-center mb-12">
      <h1 class="text-3xl font-black mb-3">
        {{ t('privacy.title') }}
      </h1>
      <p class="text-sm text-cp-text-muted">
        {{ t('privacy.updatedAt', { date: lastUpdated }) }}
      </p>
    </div>

    <div class="rounded-2xl p-8 bg-white border border-cp-border flex flex-col gap-8">
      <section
        v-for="section in sections"
        :key="section.key"
      >
        <h2 class="text-lg font-bold mb-3">
          {{ t(`privacy.sections.${section.key}.title`) }}
        </h2>
        <p class="text-sm leading-loose text-cp-text-secondary whitespace-pre-line">
          {{ t(`privacy.sections.${section.key}.body`) }}
        </p>
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
definePageMeta({ layout: 'default' })

const { t } = useI18n()

const lastUpdated = '2026-07-07'

const sections = [
  { key: 'collection' },
  { key: 'usage' },
  { key: 'sharing' },
  { key: 'cookies' },
  { key: 'retention' },
  { key: 'rights' },
  { key: 'changes' },
  { key: 'contact' }
]
</script>

<script lang="ts">
export default {
  name: 'PrivacyPage'
}
</script>

<style scoped lang="scss">
</style>
