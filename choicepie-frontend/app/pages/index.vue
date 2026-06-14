<template>
  <div />
</template>

<script setup lang="ts">
definePageMeta({ layout: 'default' })

await navigateTo('/join', { replace: true })
</script>

<script lang="ts">
export default {
  name: 'IndexPage'
}
</script>

<style scoped lang="scss">
</style>
