<template>
  <div class="max-w-7xl mx-auto px-6 py-8">
    <h1 class="text-2xl font-black mb-5 text-cp-text-primary">
      {{ t('library.title') }}
    </h1>

    <div class="inline-flex gap-1 p-1 mb-7 rounded-full bg-cp-surface-muted">
      <button
        v-for="tab in visibleTabs"
        :key="tab"
        class="px-5 py-2.5 text-sm font-bold rounded-full transition-colors"
        :class="activeTab === tab
          ? 'bg-cp-surface text-cp-primary shadow-cp-sm'
          : 'text-cp-text-secondary'"
        @click="activeTab = tab"
      >
        {{ t(`library.tabs.${tab}`) }}
      </button>
    </div>

    <AllQuizzesTab v-if="activeTab === 'all'" />
    <MyQuizzesTab v-else-if="activeTab === 'mine' && auth.isLoggedIn" />
    <FavoritesTab
      v-else-if="activeTab === 'favorites' && auth.isLoggedIn"
      @browse="activeTab = 'all'"
    />
    <FollowingTab
      v-else-if="activeTab === 'following' && auth.isLoggedIn"
      @browse="activeTab = 'all'"
    />
  </div>
</template>

<script setup lang="ts">
import AllQuizzesTab from '~/components/library/AllQuizzesTab.vue'
import FavoritesTab from '~/components/library/FavoritesTab.vue'
import FollowingTab from '~/components/library/FollowingTab.vue'
import MyQuizzesTab from '~/components/library/MyQuizzesTab.vue'
import { useAuthStore } from '~/stores/auth'

definePageMeta({ layout: 'content' })

const { t } = useI18n()
const auth = useAuthStore()
const route = useRoute()
const router = useRouter()

type LibraryTab = 'all' | 'mine' | 'favorites' | 'following'

const MEMBER_TABS: LibraryTab[] = ['mine', 'favorites', 'following']

const visibleTabs = computed<LibraryTab[]>(() => (auth.isLoggedIn ? ['all', ...MEMBER_TABS] : ['all']))

const resolveTab = (): LibraryTab => {
  const tab = route.query.tab as LibraryTab
  return auth.isLoggedIn && MEMBER_TABS.includes(tab) ? tab : 'all'
}

const activeTab = ref<LibraryTab>(resolveTab())

watch(activeTab, (tab) => {
  router.replace({ query: { ...route.query, tab: tab === 'all' ? undefined : tab } })
})

watch(() => route.query.tab, () => {
  activeTab.value = resolveTab()
})
</script>

<script lang="ts">
export default {
  name: 'LibraryPage'
}
</script>

<style scoped lang="scss">
</style>
