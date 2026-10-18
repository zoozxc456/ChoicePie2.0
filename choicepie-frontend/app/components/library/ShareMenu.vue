<template>
  <div class="relative">
    <UDropdownMenu
      :items="shareActions"
      :content="{ align: 'start' }"
    >
      <button class="h-10 px-4 rounded-full text-[13px] font-semibold bg-cp-surface-muted whitespace-nowrap cursor-pointer">
        {{ t('libraryDetail.share.action') }}
      </button>
    </UDropdownMenu>
    <span
      v-if="isCopied"
      class="absolute top-full left-1/2 -translate-x-1/2 mt-1 text-[11px] text-white bg-cp-secondary px-2 py-1 rounded-lg whitespace-nowrap"
    >
      {{ t('libraryDetail.share.copied') }}
    </span>
  </div>
</template>

<script setup lang="ts">
import type { DropdownMenuItem } from '@nuxt/ui'
import { buildQuizShareUrl, buildSocialShareUrl, type SocialShareChannel } from '~/utils/shareUrl'

interface Props {
  quizId: string
  quizTitle: string
}

const props = defineProps<Props>()

const { t } = useI18n()
const quizStore = useQuizStore()

const isCopied = ref(false)

const buildShareUrl = (channel: string) =>
  buildQuizShareUrl(window.location.origin, props.quizId, channel)

const recordShare = () => {
  quizStore.recordShare(props.quizId)
}

const handleCopyLink = async () => {
  try {
    await navigator.clipboard.writeText(buildShareUrl('copy'))
    isCopied.value = true
    setTimeout(() => {
      isCopied.value = false
    }, 2000)
  } catch {
    // 剪貼簿權限被拒絕時不阻擋使用者，僅不顯示已複製提示
  }
  recordShare()
}

const openShareWindow = (url: string) => {
  window.open(url, '_blank', 'noopener,noreferrer,width=600,height=500')
  recordShare()
}

const shareTo = (channel: SocialShareChannel) => {
  openShareWindow(buildSocialShareUrl(channel, buildShareUrl(channel), props.quizTitle))
}

const shareActions = computed<DropdownMenuItem[]>(() => [
  {
    label: t('libraryDetail.share.copyLink'),
    icon: 'i-lucide-link',
    onSelect: handleCopyLink
  },
  {
    label: t('libraryDetail.share.line'),
    icon: 'i-lucide-message-circle',
    onSelect: () => shareTo('line')
  },
  {
    label: t('libraryDetail.share.facebook'),
    icon: 'i-lucide-thumbs-up',
    onSelect: () => shareTo('facebook')
  },
  {
    label: t('libraryDetail.share.x'),
    icon: 'i-lucide-at-sign',
    onSelect: () => shareTo('x')
  }
])
</script>

<script lang="ts">
export default {
  name: 'ShareMenu'
}
</script>

<style scoped lang="scss"></style>
