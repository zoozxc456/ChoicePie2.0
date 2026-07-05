<template>
  <div
    class="min-h-screen"
    style="background: var(--cp-bg);"
  >
    <!-- Top Navbar -->

    <!-- Page Content -->
    <main>
      <slot />
    </main>
  </div>
</template>

<script setup lang="ts">
const route = useRoute()

const isMenuOpen = ref(false)
const dropdownRef = ref<HTMLElement | null>(null)

const closeMenu = () => {
  isMenuOpen.value = false
}

onClickOutside(dropdownRef, closeMenu)
watch(() => route.path, closeMenu)
</script>

<script lang="ts">
export default {
  name: 'DefaultLayout'
}
</script>

<style scoped lang="scss">

</style>
