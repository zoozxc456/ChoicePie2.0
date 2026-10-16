<template>
  <LegalDocument
    namespace="privacy"
    :section-keys="SECTION_KEYS"
    last-updated="2026-07-07"
  />
</template>

<script setup lang="ts">
import LegalDocument from '~/components/common/LegalDocument.vue'

definePageMeta({ layout: 'content' })

const SECTION_KEYS = [
  'collection',
  'usage',
  'sharing',
  'cookies',
  'retention',
  'rights',
  'changes',
  'contact'
] as const
</script>

<script lang="ts">
export default {
  name: 'PrivacyPage'
}
</script>

<style scoped lang="scss"></style>
