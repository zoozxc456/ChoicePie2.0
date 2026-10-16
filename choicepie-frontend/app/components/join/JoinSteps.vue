<template>
  <div class="rounded-xl p-4 bg-white border border-neutral-200">
    <p class="text-xs font-semibold mb-3 text-neutral-400">
      {{ t('join.steps.title') }}
    </p>
    <ol class="flex flex-col gap-3">
      <li
        v-for="(step, i) in steps"
        :key="i"
        class="flex items-center gap-3"
      >
        <span
          data-testid="join-step-number"
          class="w-6 h-6 rounded-full text-xs font-bold flex items-center justify-center shrink-0 text-white bg-primary-500"
        >{{ i + 1 }}</span>
        <div>
          <p class="text-sm font-medium">
            {{ step.title }}
          </p>
          <p class="text-xs text-neutral-400">
            {{ step.desc }}
          </p>
        </div>
      </li>
    </ol>
  </div>
</template>

<script setup lang="ts">
const { t } = useI18n()

const steps = computed(() => [
  { title: t('join.steps.step1Title'), desc: t('join.steps.step1Desc') },
  { title: t('join.steps.step2Title'), desc: t('join.steps.step2Desc') },
  { title: t('join.steps.step3Title'), desc: t('join.steps.step3Desc') }
])
</script>

<script lang="ts">
export default {
  name: 'JoinSteps'
}
</script>

<style scoped lang="scss"></style>
