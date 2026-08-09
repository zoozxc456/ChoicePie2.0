<template>
  <div class="max-w-7xl mx-auto px-6 py-8">
    <h1 class="text-2xl font-black mb-5 text-cp-text-primary">
      {{ t('library.title') }}
    </h1>

    <div class="inline-flex gap-1 p-1 mb-7 rounded-full bg-cp-surface-muted">
      <button
        class="px-5 py-2.5 text-sm font-bold rounded-full transition-colors"
        :class="activeTab === 'all'
          ? 'bg-cp-surface text-cp-primary shadow-cp-sm'
          : 'text-cp-text-secondary'"
        @click="activeTab = 'all'"
      >
        {{ t('library.tabs.all') }}
      </button>
      <button
        v-if="auth.isLoggedIn"
        class="px-5 py-2.5 text-sm font-bold rounded-full transition-colors"
        :class="activeTab === 'mine'
          ? 'bg-cp-surface text-cp-primary shadow-cp-sm'
          : 'text-cp-text-secondary'"
        @click="activeTab = 'mine'"
      >
        {{ t('library.tabs.mine') }}
      </button>
    </div>

    <AllQuizzesTab v-if="activeTab === 'all'" />
    <MyQuizzesTab v-else-if="activeTab === 'mine' && auth.isLoggedIn" />
  </div>
</template>

<script setup lang="ts">
import AllQuizzesTab from '~/components/library/AllQuizzesTab.vue'
import MyQuizzesTab from '~/components/library/MyQuizzesTab.vue'
import { useAuthStore } from '~/stores/auth'

definePageMeta({ layout: 'content' })

const { t } = useI18n()
const auth = useAuthStore()
const route = useRoute()
const router = useRouter()

type LibraryTab = 'all' | 'mine'

const resolveTab = (): LibraryTab => (route.query.tab === 'mine' && auth.isLoggedIn ? 'mine' : 'all')

const activeTab = ref<LibraryTab>(resolveTab())

watch(activeTab, (tab) => {
  router.replace({ query: { ...route.query, tab: tab === 'mine' ? 'mine' : undefined } })
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
