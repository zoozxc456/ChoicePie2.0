<template>
  <AnswerFeedback />
  <QuestionExplanation />
  <CurrentRank />

  <p class="text-center text-sm mt-4 text-neutral-400">
    {{ t('room.result.waitingNext') }}
  </p>
</template>

<script lang="ts" setup>
import AnswerFeedback from '~/components/gameRoom/result/AnswerFeedback.vue'
import QuestionExplanation from '~/components/gameRoom/result/QuestionExplanation.vue'
import CurrentRank from '~/components/gameRoom/result/CurrentRank.vue'

const { t } = useI18n()
</script>

<style scoped lang="scss"></style>
