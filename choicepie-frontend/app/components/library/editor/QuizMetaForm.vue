<template>
  <div class="bg-cp-surface rounded-2xl border border-cp-border p-5 flex flex-col gap-4">
    <div class="flex flex-col gap-2">
      <label class="text-[13px] font-bold text-cp-text-primary">
        {{ t('myQuizzesEdit.titleLabel') }}
      </label>
      <UInput
        v-model="title"
        data-testid="meta-title"
        size="lg"
        class="w-full"
        :ui="{ base: 'h-11 text-sm px-3.5' }"
      />
    </div>
    <div class="flex flex-col gap-2">
      <label class="text-[13px] font-bold text-cp-text-primary">
        {{ t('myQuizzesEdit.descriptionLabel') }}
      </label>
      <textarea
        v-model="description"
        rows="3"
        class="w-full rounded-lg p-3 text-sm resize-none outline-none bg-cp-surface-muted border-[1.5px] border-transparent focus:border-cp-primary"
        :placeholder="t('myQuizzesEdit.descriptionPlaceholder')"
      />
    </div>
    <div class="flex flex-col gap-2">
      <label class="text-[13px] font-bold text-cp-text-primary">
        {{ t('myQuizzesEdit.tagsLabel') }}
      </label>
      <UInput
        v-model="tags"
        data-testid="meta-tags"
        size="lg"
        class="w-full"
        :placeholder="t('myQuizzesEdit.tagsPlaceholder')"
        :ui="{ base: 'h-11 text-sm px-3.5' }"
      />
    </div>
    <UButton
      data-testid="meta-save"
      class="self-start rounded-full font-bold"
      color="primary"
      :loading="saving"
      @click="emit('save')"
    >
      {{ t('myQuizzesEdit.saveMeta') }}
    </UButton>
  </div>
</template>

<script setup lang="ts">
interface Props {
  saving?: boolean
}

withDefaults(defineProps<Props>(), {
  saving: false
})

const emit = defineEmits<{
  save: []
}>()

const title = defineModel<string>('title', { required: true })
const description = defineModel<string>('description', { required: true })
/** 以逗號分隔的標籤字串，由頁面負責拆解 */
const tags = defineModel<string>('tags', { required: true })

const { t } = useI18n()
</script>

<script lang="ts">
export default {
  name: 'QuizMetaForm'
}
</script>

<style scoped lang="scss"></style>
