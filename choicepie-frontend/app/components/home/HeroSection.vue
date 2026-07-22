<template>
  <div class="relative w-screen min-h-dvh overflow-hidden bg-[linear-gradient(135deg,#1a1a2e_0%,#2d3748_100%)]">
    <div class="mx-auto flex min-h-dvh max-w-7xl flex-col items-center justify-center gap-14 px-6 py-24 lg:flex-row lg:justify-between lg:gap-10 lg:px-16">
      <!-- 左：文字與 CTA -->
      <div class="flex max-w-lg flex-col gap-6 text-center lg:text-left">
        <h1 class="text-4xl font-extrabold leading-[1.2] text-white lg:text-6xl">
          AI 三秒出題，讓每場聚會都變成搶答派對
        </h1>
        <p class="text-lg leading-relaxed text-white/60">
          貼上一段文字或一個網址，ChoicePie 自動生成互動問答遊戲，人人掃碼即可加入
        </p>

        <div class="flex flex-col items-center gap-2 lg:items-start">
          <div class="flex items-center gap-2 rounded-full bg-white/10 p-2 pl-6">
            <UInput
              v-model="gameCode"
              placeholder="輸入遊戲代碼"
              size="xl"
              class="w-56"
              :ui="{ base: 'bg-transparent text-white placeholder-[#FFFFFF80] border-none! shadow-none! ring-0! outline-none! text-lg font-semibold tracking-wide' }"
              @keyup.enter="joinGame"
            />
            <UButton
              color="primary"
              icon="i-lucide-arrow-right"
              size="xl"
              class="rounded-full"
              @click="joinGame"
            />
          </div>
          <p
            v-if="errorMessage"
            class="text-sm font-medium text-error-400"
          >
            {{ errorMessage }}
          </p>
        </div>
      </div>

      <!-- 右：唯一視覺主體，實際搶答畫面縮影 -->
      <div
        class="quiz-card-tilt relative w-full max-w-xl shrink-0 overflow-hidden rounded-2xl bg-white shadow-[0_40px_100px_rgba(0,0,0,0.45)]"
        aria-hidden="true"
      >
        <!-- 瀏覽器頂欄，呼應下方特色區的視覺語言 -->
        <div class="flex items-center gap-2 border-b border-neutral-100 bg-neutral-50 px-4 py-2.5">
          <span class="h-2.5 w-2.5 rounded-full bg-[#ff5f57]" />
          <span class="h-2.5 w-2.5 rounded-full bg-[#febc2e]" />
          <span class="h-2.5 w-2.5 rounded-full bg-[#28c840]" />
          <span class="ml-3 truncate text-xs text-neutral-400">choicepie.app/room/482913</span>
        </div>

        <div class="p-8">
          <div class="mb-5 flex items-center justify-between">
            <span class="text-sm font-semibold text-neutral-400">題目 3 / 10</span>
            <span class="text-2xl font-black tabular-nums text-primary-500">08</span>
          </div>
          <p class="mb-6 text-xl font-bold leading-snug text-neutral-900">
            派對甜點該怎麼配才對味？
          </p>
          <div class="flex flex-col gap-3">
            <div
              v-for="opt in options"
              :key="opt.letter"
              class="flex items-center gap-3 rounded-xl px-4 py-3.5 text-base font-medium"
              :class="opt.correct ? 'bg-success-50 ring-1 ring-success-500' : 'bg-neutral-50'"
            >
              <span
                class="flex h-7 w-7 shrink-0 items-center justify-center rounded-full text-sm font-bold text-white"
                :class="opt.correct ? 'bg-success-500' : 'bg-neutral-300'"
              >{{ opt.letter }}</span>
              <span :class="opt.correct ? 'text-success-800' : 'text-neutral-700'">{{ opt.text }}</span>
              <span
                v-if="opt.correct"
                class="ml-auto text-success-500"
              >✓</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
const gameCode = ref('')
const errorMessage = ref('')

const options = [
  { letter: 'A', text: '紅酒配起司', correct: false },
  { letter: 'B', text: '氣泡水配馬卡龍', correct: true },
  { letter: 'C', text: '啤酒配鹹酥雞', correct: false },
  { letter: 'D', text: '純喝水', correct: false }
]

const joinGame = () => {
  if (gameCode.value.trim() === '') {
    errorMessage.value = '請輸入遊戲代碼'
    return
  }
  errorMessage.value = ''
  navigateTo(`/join/${gameCode.value}`)
}
</script>

<style scoped lang="scss">
.quiz-card-tilt {
  transform: rotate(3deg);
}

@media (min-width: 1024px) {
  .quiz-card-tilt {
    transform: rotate(4deg) translateY(-8px);
  }
}
</style>
