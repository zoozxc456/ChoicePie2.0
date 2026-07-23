<template>
  <div class="relative">
    <UDropdownMenu
      :items="shareActions"
      :content="{ align: 'start' }"
    >
      <button class="h-10 px-4 rounded-full text-[13px] font-semibold border border-neutral-200 bg-white whitespace-nowrap">
        {{ t('libraryDetail.share.action') }}
      </button>
    </UDropdownMenu>
    <span
      v-if="isCopied"
      class="absolute top-full left-1/2 -translate-x-1/2 mt-1 text-[11px] text-white bg-neutral-800 px-2 py-1 rounded-lg whitespace-nowrap"
    >
      {{ t('libraryDetail.share.copied') }}
    </span>
  </div>
</template>

<script setup lang="ts">
import type { DropdownMenuItem } from '@nuxt/ui'

interface Props {
  quizId: string
  quizTitle: string
}

const props = defineProps<Props>()

const { t } = useI18n()
const quizStore = useQuizStore()

const isCopied = ref(false)

const buildShareUrl = (channel: string) =>
  `${window.location.origin}/library/${props.quizId}?ref=${channel}`

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

const handleShareLine = () => {
  const url = `https://social-plugins.line.me/lineit/share?url=${encodeURIComponent(buildShareUrl('line'))}`
  openShareWindow(url)
}

const handleShareFacebook = () => {
  const url = `https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(buildShareUrl('facebook'))}`
  openShareWindow(url)
}

const handleShareX = () => {
  const text = encodeURIComponent(props.quizTitle)
  const url = `https://twitter.com/intent/tweet?text=${text}&url=${encodeURIComponent(buildShareUrl('x'))}`
  openShareWindow(url)
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
    onSelect: handleShareLine
  },
  {
    label: t('libraryDetail.share.facebook'),
    icon: 'i-lucide-thumbs-up',
    onSelect: handleShareFacebook
  },
  {
    label: t('libraryDetail.share.x'),
    icon: 'i-lucide-at-sign',
    onSelect: handleShareX
  }
])
</script>

<script lang="ts">
export default {
  name: 'ShareMenu'
}
</script>

<style scoped lang="scss"></style>
