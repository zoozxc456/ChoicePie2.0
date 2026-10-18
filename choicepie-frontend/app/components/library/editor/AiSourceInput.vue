<template>
  <div class="bg-white border border-neutral-200 rounded-2xl p-5 flex flex-col gap-2">
    <textarea
      v-model="model"
      rows="7"
      class="w-full rounded-xl p-3.5 text-sm resize-none outline-none bg-neutral-100 border-[1.5px] transition-colors"
      :class="model ? 'border-primary-500' : 'border-neutral-200'"
      :placeholder="t('hostNew.ai.contentPlaceholder')"
    />
    <div class="flex items-center justify-between">
      <span
        data-testid="source-hint"
        class="text-xs text-neutral-400"
      >
        {{ isUrlSource(model) ? t('hostNew.ai.urlHint') : t('hostNew.ai.charCount', { count: model.length }) }}
      </span>
      <span
        v-if="isSourceTooShort(model)"
        data-testid="min-chars-hint"
        class="text-xs text-warning-500"
      >
        {{ t('hostNew.ai.minCharsHint') }}
      </span>
    </div>
  </div>
</template>

<script setup lang="ts">
import { isSourceTooShort, isUrlSource } from '~/utils/aiSource'

const model = defineModel<string>({ required: true })

const { t } = useI18n()
</script>

<script lang="ts">
export default {
  name: 'AiSourceInput'
}
</script>

<style scoped lang="scss"></style>
