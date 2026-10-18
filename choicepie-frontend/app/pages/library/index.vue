<template>
  <div class="max-w-7xl mx-auto px-6 py-8">
    <h1 class="text-2xl font-black mb-5 text-cp-text-primary">
      {{ t('library.title') }}
    </h1>

    <SegmentedControl
      v-model="activeTab"
      :options="tabOptions"
      shape="pill"
      class="mb-7"
    />

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
import SegmentedControl from '~/components/common/SegmentedControl.vue'
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

const tabOptions = computed(() => visibleTabs.value.map(tab => ({ value: tab, label: t(`library.tabs.${tab}`) })))

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
