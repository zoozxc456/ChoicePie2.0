<template>
  <div class="max-w-7xl mx-auto px-6 py-8">
    <h1 class="text-2xl font-black mb-5">
      {{ t('library.title') }}
    </h1>

    <div class="flex gap-2 mb-7 border-b border-neutral-200">
      <button
        class="px-4 py-3 text-sm font-bold border-b-2 -mb-px transition-colors"
        :class="activeTab === 'all'
          ? 'border-primary-500 text-primary-500'
          : 'border-transparent text-neutral-500'"
        @click="activeTab = 'all'"
      >
        {{ t('library.tabs.all') }}
      </button>
      <button
        v-if="auth.isLoggedIn"
        class="px-4 py-3 text-sm font-bold border-b-2 -mb-px transition-colors"
        :class="activeTab === 'mine'
          ? 'border-primary-500 text-primary-500'
          : 'border-transparent text-neutral-500'"
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
