<template>
  <div>
    <ChoicePieHeader transparent />
    <HeroSection />
    <PlayingInstruction />
    <ProductHighlights />
    <HomeFooter />
  </div>
</template>

<script setup lang="ts">
import ChoicePieHeader from '~/components/common/ChoicePieHeader.vue'
import HeroSection from '~/components/home/HeroSection.vue'
import PlayingInstruction from '~/components/home/PlayingInstruction.vue'
import ProductHighlights from '~/components/home/ProductHighlights.vue'
import HomeFooter from '~/components/home/HomeFooter.vue'

definePageMeta({ layout: 'default' })
</script>

<script lang="ts">
export default {
  name: 'IndexPage'
}
</script>

<style scoped lang="scss"></style>
