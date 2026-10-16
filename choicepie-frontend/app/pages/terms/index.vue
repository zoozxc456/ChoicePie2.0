<template>
  <LegalDocument
    namespace="terms"
    :section-keys="SECTION_KEYS"
    last-updated="2026-07-07"
  />
</template>

<script setup lang="ts">
import LegalDocument from '~/components/common/LegalDocument.vue'

definePageMeta({ layout: 'content' })

const SECTION_KEYS = [
  'acceptance',
  'accounts',
  'content',
  'conduct',
  'termination',
  'disclaimer',
  'changes',
  'contact'
] as const
</script>

<script lang="ts">
export default {
  name: 'TermsPage'
}
</script>

<style scoped lang="scss"></style>
