<template>
  <UApp>
    <NuxtLayout>
      <NuxtPage :page-key="route => route.fullPath" />
    </NuxtLayout>
  </UApp>
</template>

<script setup lang="ts">
</script>

<script lang="ts">
export default {
  name: 'App'
}
</script>

<style scoped lang="scss">
</style>
