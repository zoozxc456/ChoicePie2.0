<template>
  <button
    type="button"
    :aria-pressed="isFollowing"
    class="px-5 rounded-full font-semibold border whitespace-nowrap cursor-pointer disabled:opacity-60"
    :class="[
      size === 'sm' ? 'h-8 text-[13px]' : 'h-9 text-sm',
      isFollowing
        ? 'border-cp-border bg-cp-surface text-cp-text-primary'
        : 'border-transparent bg-cp-primary text-white'
    ]"
    :disabled="loading"
    @click="emit('toggle')"
  >
    {{ isFollowing ? t('libraryDetail.creator.following') : t('libraryDetail.creator.follow') }}
  </button>
</template>

<script setup lang="ts">
interface Props {
  isFollowing: boolean
  loading?: boolean
  size?: 'sm' | 'md'
}

withDefaults(defineProps<Props>(), {
  loading: false,
  size: 'md'
})

const emit = defineEmits<{
  toggle: []
}>()

const { t } = useI18n()
</script>

<script lang="ts">
export default {
  name: 'FollowButton'
}
</script>

<style scoped lang="scss"></style>
