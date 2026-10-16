<template>
  <section
    class="w-full rounded-2xl bg-white shadow-cp-lg p-7 flex flex-col gap-4"
    :class="danger ? 'border border-error-200' : ''"
  >
    <div>
      <h2
        class="text-lg font-extrabold"
        :class="danger ? 'text-error-600' : 'text-cp-text-primary'"
      >
        {{ title }}
      </h2>
      <p
        v-if="description"
        class="text-sm text-neutral-500 mt-1"
      >
        {{ description }}
      </p>
    </div>
    <slot />
  </section>
</template>

<script setup lang="ts">
interface Props {
  title: string
  description?: string
  danger?: boolean
}

withDefaults(defineProps<Props>(), {
  description: undefined,
  danger: false
})
</script>

<style scoped lang="scss"></style>
