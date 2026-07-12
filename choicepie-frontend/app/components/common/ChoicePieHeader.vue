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
        class="hamburger-btn relative flex items-center justify-center w-10 h-10 rounded-full transition-transform active:scale-90"
        :class="[transparent ? 'border-0 bg-white' : 'border border-cp-border bg-white', isMenuOpen ? 'menu-open' : '']"
        @click="isMenuOpen = !isMenuOpen"
      >
        <span class="hamburger-line hamburger-line--top bg-cp-text-secondary" />
        <span class="hamburger-line hamburger-line--mid bg-cp-text-secondary" />
        <span class="hamburger-line hamburger-line--bottom bg-cp-text-secondary" />
      </button>

      <!-- Dropdown -->
      <Transition name="dropdown">
        <div
          v-if="isMenuOpen"
          class="absolute right-0 top-13 rounded-2xl min-w-56 z-50 bg-white border border-cp-border shadow-cp-xl overflow-hidden"
        >
          <template v-if="auth.isLoggedIn">
            <div class="flex items-center gap-2.5 px-3.5 py-3 bg-[linear-gradient(135deg,#fff8f0_0%,#fff3e0_100%)]">
              <span class="flex items-center justify-center w-9 h-9 rounded-full text-sm font-bold text-white bg-cp-primary ring-2 ring-white shadow-cp-sm">
                {{ (auth.user?.name ?? '?').charAt(0).toUpperCase() }}
              </span>
              <span class="text-sm font-bold text-cp-text-primary truncate">
                {{ auth.user?.name }}
              </span>
            </div>
            <div class="py-1">
              <NuxtLink
                to="/library/new"
                class="dropdown-item dropdown-item--primary"
                @click="closeMenu"
              ><UIcon
                name="i-lucide-sparkles"
                class="dropdown-item__icon"
              />{{ t('nav.createGame') }}</NuxtLink>
              <NuxtLink
                to="/library/mine"
                class="dropdown-item dropdown-item--info"
                @click="closeMenu"
              ><UIcon
                name="i-lucide-library"
                class="dropdown-item__icon"
              />{{ t('nav.myLibrary') }}</NuxtLink>
              <NuxtLink
                to="/history"
                class="dropdown-item dropdown-item--info"
                @click="closeMenu"
              ><UIcon
                name="i-lucide-bar-chart-3"
                class="dropdown-item__icon"
              />{{ t('nav.history') }}</NuxtLink>
            </div>
            <hr class="my-1 border-cp-border">
            <div class="py-1">
              <button
                class="dropdown-item dropdown-item--danger text-left"
                @click="auth.logout"
              >
                <UIcon
                  name="i-lucide-log-out"
                  class="dropdown-item__icon"
                />{{ t('nav.logout') }}
              </button>
            </div>
          </template>
          <template v-else>
            <div class="pt-2 pb-1">
              <NuxtLink
                to="/login"
                class="dropdown-item dropdown-item--primary"
                @click="closeMenu"
              ><UIcon
                name="i-lucide-user"
                class="dropdown-item__icon"
              />{{ t('nav.login') }}</NuxtLink>
              <NuxtLink
                to="/library/new"
                class="dropdown-item dropdown-item--info"
                @click="closeMenu"
              ><UIcon
                name="i-lucide-sparkles"
                class="dropdown-item__icon"
              />{{ t('nav.createGame') }}</NuxtLink>
            </div>
          </template>
          <hr class="my-1 border-cp-border">
          <div class="pt-1 pb-2">
            <NuxtLink
              to="/how-to-use"
              class="dropdown-item dropdown-item--warning"
              @click="closeMenu"
            ><UIcon
              name="i-lucide-help-circle"
              class="dropdown-item__icon"
            />{{ t('nav.howToUse') }}
            </NuxtLink>
            <NuxtLink
              to="/about"
              class="dropdown-item dropdown-item--success"
              @click="closeMenu"
            ><UIcon
              name="i-lucide-book-open"
              class="dropdown-item__icon"
            />{{ t('nav.about') }}
            </NuxtLink>
          </div>
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
/* 漢堡選單 → X 變形動畫 */
.hamburger-line {
  position: absolute;
  top: 50%;
  left: 50%;
  display: block;
  width: 16px;
  height: 2px;
  border-radius: 9999px;
  transform: translate(-50%, -50%);
  transition: transform 250ms cubic-bezier(0.34, 1.56, 0.64, 1), opacity 150ms ease;
}
.hamburger-line--top {
  transform: translate(-50%, calc(-50% - 5px));
}
.hamburger-line--bottom {
  transform: translate(-50%, calc(-50% + 5px));
}
.hamburger-btn.menu-open .hamburger-line--top {
  transform: translate(-50%, -50%) rotate(45deg);
}
.hamburger-btn.menu-open .hamburger-line--mid {
  opacity: 0;
  transform: translate(-50%, -50%) scaleX(0);
}
.hamburger-btn.menu-open .hamburger-line--bottom {
  transform: translate(-50%, -50%) rotate(-45deg);
}

/* Dropdown 選項：每個功能配一種品牌色系 hover 回饋 */
.dropdown-item {
  display: flex;
  align-items: center;
  gap: 10px;
  width: calc(100% - 12px);
  margin: 0 6px;
  padding: 9px 10px;
  border: none;
  border-radius: 10px;
  background: none;
  font-size: 13px;
  font-weight: 600;
  font-family: inherit;
  color: var(--cp-text-primary);
  cursor: pointer;
  transition: background var(--cp-duration-fast), transform var(--cp-duration-fast), color var(--cp-duration-fast);
  text-decoration: none;
}

.dropdown-item__icon {
  flex-shrink: 0;
  width: 16px;
  height: 16px;
}

.dropdown-item:hover {
  transform: translateX(2px);
}

.dropdown-item--primary:hover {
  background: var(--cp-primary-light);
  color: var(--cp-primary-hover);
}
.dropdown-item--info:hover {
  background: var(--cp-info-bg);
  color: var(--cp-info);
}
.dropdown-item--warning:hover {
  background: var(--cp-warning-bg);
  color: var(--cp-warning);
}
.dropdown-item--success:hover {
  background: var(--cp-success-bg);
  color: #2e7d32;
}
.dropdown-item--danger {
  color: var(--cp-danger);
}
.dropdown-item--danger:hover {
  background: var(--cp-danger-bg);
}

/* Dropdown 進場動畫：從 logo 附近彈出，帶一點彈性 */
.dropdown-enter-active {
  transition: opacity 220ms ease, transform 220ms cubic-bezier(0.34, 1.56, 0.64, 1);
}
.dropdown-leave-active {
  transition: opacity 120ms ease, transform 120ms ease;
}

.dropdown-enter-from,
.dropdown-leave-to {
  opacity: 0;
  transform: translateY(-8px) scale(0.95);
}
</style>
