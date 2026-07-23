<template>
  <div class="flex gap-3">
    <div class="w-8 h-8 rounded-full flex items-center justify-center text-xs font-bold text-white bg-secondary-800 shrink-0">
      {{ comment.userName[0] }}
    </div>
    <div class="flex-1 min-w-0">
      <div class="flex items-center justify-between gap-2">
        <p class="text-[13px] font-semibold">
          {{ comment.userName }}
        </p>
        <div
          v-if="isOwner && !isEditing"
          class="flex items-center gap-2 shrink-0"
        >
          <button
            class="text-xs text-neutral-400 hover:text-neutral-700"
            @click="startEditing"
          >
            {{ t('libraryDetail.comments.edit') }}
          </button>
          <button
            class="text-xs text-neutral-400 hover:text-error-600"
            :disabled="isDeleting"
            @click="$emit('delete', comment.id)"
          >
            {{ t('libraryDetail.comments.delete') }}
          </button>
        </div>
      </div>

      <div
        v-if="isEditing"
        class="flex flex-col gap-2 mt-1"
      >
        <UTextarea
          v-model="editText"
          :rows="2"
          autoresize
        />
        <div class="flex items-center gap-2 self-end">
          <UButton
            size="xs"
            variant="ghost"
            color="neutral"
            :disabled="isUpdating"
            @click="cancelEditing"
          >
            {{ t('libraryDetail.comments.cancel') }}
          </UButton>
          <UButton
            size="xs"
            color="primary"
            :loading="isUpdating"
            :disabled="!editText.trim()"
            @click="submitEdit"
          >
            {{ t('libraryDetail.comments.save') }}
          </UButton>
        </div>
      </div>
      <p
        v-else
        class="text-sm text-neutral-800 whitespace-pre-wrap wrap-break-word"
      >
        {{ comment.text }}
      </p>
    </div>
  </div>
</template>

<script lang="ts" setup>
import type { CommentDto } from '~/types/api'

interface Props {
  comment: CommentDto
  isOwner: boolean
  isUpdating?: boolean
  isDeleting?: boolean
}

const props = defineProps<Props>()
const emit = defineEmits<{
  update: [commentId: string, text: string]
  delete: [commentId: string]
}>()

const { t } = useI18n()

const isEditing = ref(false)
const editText = ref('')

const startEditing = () => {
  editText.value = props.comment.text
  isEditing.value = true
}

const cancelEditing = () => {
  isEditing.value = false
}

const submitEdit = () => {
  if (!editText.value.trim()) return
  emit('update', props.comment.id, editText.value.trim())
}

watch(() => props.isUpdating, (updating, wasUpdating) => {
  if (wasUpdating && !updating) {
    isEditing.value = false
  }
})
</script>

<style scoped lang="scss"></style>
