<template>
  <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
    <div
      v-for="(rule, i) in rules"
      :key="rule.time"
      data-testid="scoring-rule"
      class="rounded-2xl p-5 text-center border transition-transform hover:-translate-y-1"
      :class="i === 0
        ? 'bg-primary-500/15 border-primary-500/40'
        : 'bg-white/5 border-white/10'"
    >
      <p
        class="text-3xl font-black mb-1"
        :class="i === 0 ? 'text-primary-500' : 'text-white'"
      >
        {{ rule.points.toLocaleString() }}
      </p>
      <p class="text-xs font-semibold mb-2 text-white/40">
        {{ unitLabel }}
      </p>
      <p class="text-sm font-medium text-white/70">
        {{ rule.time }}
      </p>
    </div>
  </div>
</template>

<script setup lang="ts">
interface ScoringRule {
  time: string
  points: number
}

interface Props {
  /** 由快到慢排序；第一條（最高分）會被強調 */
  rules: ScoringRule[]
  unitLabel: string
}

defineProps<Props>()
</script>

<script lang="ts">
export default {
  name: 'ScoringRuleGrid'
}
</script>

<style scoped lang="scss"></style>
