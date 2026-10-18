<template>
  <div class="max-w-3xl mx-auto px-6 py-12">
    <div class="text-center mb-12">
      <h1 class="text-3xl font-black mb-3">
        {{ t(`${namespace}.title`) }}
      </h1>
      <p
        data-testid="last-updated"
        class="text-sm text-cp-text-muted"
      >
        {{ t(`${namespace}.updatedAt`, { date: lastUpdated }) }}
      </p>
    </div>

    <div class="rounded-2xl p-8 bg-white border border-cp-border flex flex-col gap-8">
      <section
        v-for="key in sectionKeys"
        :key="key"
      >
        <h2 class="text-lg font-bold mb-3">
          {{ t(`${namespace}.sections.${key}.title`) }}
        </h2>
        <p class="text-sm leading-loose text-cp-text-secondary whitespace-pre-line">
          {{ t(`${namespace}.sections.${key}.body`) }}
        </p>
      </section>
    </div>
  </div>
</template>

<script setup lang="ts">
interface Props {
  /** i18n 命名空間，需包含 title / updatedAt / sections.<key>.{title,body} */
  namespace: 'privacy' | 'terms'
  sectionKeys: readonly string[]
  lastUpdated: string
}

defineProps<Props>()

const { t } = useI18n()
</script>

<script lang="ts">
export default {
  name: 'LegalDocument'
}
</script>

<style scoped lang="scss"></style>
