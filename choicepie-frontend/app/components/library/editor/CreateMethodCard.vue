<template>
  <NuxtLink
    :to="to"
    class="cursor-pointer w-80 bg-cp-surface rounded-2xl px-7 py-8 flex flex-col items-center text-center gap-3 border border-cp-border transition-all"
    :class="dimmed ? 'opacity-60' : 'hover:shadow-cp-md hover:border-transparent hover:-translate-y-0.5'"
  >
    <UIcon
      :name="icon"
      class="text-4xl"
      :class="iconClass"
    />
    <p
      data-testid="method-title"
      class="text-lg font-bold text-cp-text-primary"
    >
      {{ title }}
    </p>
    <p
      data-testid="method-description"
      class="text-[13px] text-cp-text-secondary leading-relaxed"
    >
      {{ description }}
    </p>
    <slot name="badge" />
    <div
      data-testid="method-cta"
      class="mt-2 h-10 px-5 rounded-full text-white font-bold text-[13px] flex items-center whitespace-nowrap"
      :class="ctaClass"
    >
      {{ ctaLabel }}
    </div>
  </NuxtLink>
</template>

<script setup lang="ts">
interface Props {
  to: string
  icon: string
  title: string
  description: string
  ctaLabel: string
  iconClass?: string
  ctaClass?: string
  /** 無法使用（例如 AI 額度用完）時淡化並取消 hover 效果 */
  dimmed?: boolean
}

withDefaults(defineProps<Props>(), {
  iconClass: 'text-cp-primary',
  ctaClass: 'bg-cp-primary',
  dimmed: false
})
</script>

<script lang="ts">
export default {
  name: 'CreateMethodCard'
}
</script>

<style scoped lang="scss"></style>
