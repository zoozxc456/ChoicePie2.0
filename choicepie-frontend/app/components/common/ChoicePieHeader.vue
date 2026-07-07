<template>
  <header
    class="fixed top-0 left-0 right-0 z-50 flex items-center gap-3 px-6 h-16"
    :class="transparent ? 'bg-transparent' : 'bg-white border-b border-cp-border'"
  >
    <!-- Logo -->
    <NuxtLink
      to="/"
      class="flex items-center gap-2 mr-auto text-lg font-bold"
      :class="transparent ? 'text-white' : 'text-cp-primary'"
    >
      🥧 ChoicePie
    </NuxtLink>

    <!-- 加入遊戲 CTA -->
    <NuxtLink to="/join">
      <UButton
        color="primary"
        size="md"
        class="rounded-full px-4 py-2 text-sm font-bold"
      >{{ t('nav.join') }}</UButton>
    </NuxtLink>

    <!-- 漢堡選單 -->
    <div
      ref="dropdownRef"
      class="relative"
    >
      <button
        class="flex flex-col items-center justify-center w-9 h-9 rounded-full gap-1"
        :class="transparent ? 'border-0 bg-white' : 'border border-cp-border'"
        @click="isMenuOpen = !isMenuOpen"
      >
        <span class="block w-4 h-px rounded bg-cp-text-secondary" />
        <span class="block w-4 h-px rounded bg-cp-text-secondary" />
        <span class="block w-4 h-px rounded bg-cp-text-secondary" />
      </button>

      <!-- Dropdown -->
      <Transition name="dropdown">
        <div
          v-if="isMenuOpen"
          class="absolute right-0 top-11 rounded-xl py-1.5 min-w-44 z-50 bg-white border border-cp-border shadow-cp-lg"
        >
          <template v-if="auth.isLoggedIn">
            <div class="px-3 py-2 text-xs font-semibold text-cp-text-muted">
              {{ auth.user?.name }}
            </div>
            <hr class="my-1 border-cp-border">
            <NuxtLink
              to="/library/new"
              class="dropdown-item"
              @click="closeMenu"
            >✨ {{ t('nav.createGame') }}</NuxtLink>
            <NuxtLink
              to="/history"
              class="dropdown-item"
              @click="closeMenu"
            >📊 {{ t('nav.history') }}</NuxtLink>
            <hr class="my-1 border-cp-border">
            <button
              class="dropdown-item w-full text-left text-cp-danger"
              @click="auth.logout"
            >
              {{ t('nav.logout') }}
            </button>
          </template>
          <template v-else>
            <NuxtLink
              to="/login"
              class="dropdown-item"
              @click="closeMenu"
            >👤 {{ t('nav.login') }}</NuxtLink>
            <NuxtLink
              to="/library/new"
              class="dropdown-item"
              @click="closeMenu"
            >✨ {{ t('nav.createGame') }}</NuxtLink>
          </template>
          <hr class="my-1 border-cp-border">
          <NuxtLink
            to="/how-to-use"
            class="dropdown-item"
            @click="closeMenu"
          >❓ {{ t('nav.howToUse') }}
          </NuxtLink>
          <NuxtLink
            to="/about"
            class="dropdown-item"
            @click="closeMenu"
          >📖 {{ t('nav.about') }}
          </NuxtLink>
        </div>
      </Transition>
    </div>
  </header>
</template>

<script setup lang="ts">
withDefaults(defineProps<{ transparent?: boolean }>(), { transparent: false })

const { t } = useI18n()
const auth = useAuthStore()
const route = useRoute()

const isMenuOpen = ref(false)
const dropdownRef = ref<HTMLElement | null>(null)

const closeMenu = () => {
  isMenuOpen.value = false
}

onClickOutside(dropdownRef, closeMenu)
watch(() => route.path, closeMenu)
</script>

<style scoped lang="scss">
.dropdown-item {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  border-radius: 8px;
  font-size: 13px;
  color: var(--cp-text-primary);
  cursor: pointer;
  transition: background var(--cp-duration-fast);
  text-decoration: none;
}

.dropdown-item:hover {
  background: var(--cp-surface-muted);
}

/* Dropdown animation */
.dropdown-enter-active,
.dropdown-leave-active {
  transition: opacity var(--cp-duration-fast), transform var(--cp-duration-fast);
}

.dropdown-enter-from,
.dropdown-leave-to {
  opacity: 0;
  transform: translateY(-4px);
}
</style>
