<template>
  <div class="w-full bg-[#FBFAF7] px-6 py-20 md:px-16">
    <h2 class="text-2xl text-[#1A1A1A] font-bold text-center mb-14">
      怎麼玩？
    </h2>

    <div class="relative mx-auto max-w-5xl">
      <!-- 連接三步驟的虛線路徑（僅桌面顯示） -->
      <div
        class="pointer-events-none absolute left-0 right-0 top-6 hidden md:block"
        aria-hidden="true"
      >
        <svg
          class="w-full"
          height="4"
          preserveAspectRatio="none"
        >
          <line
            x1="16.5%"
            y1="2"
            x2="83.5%"
            y2="2"
            stroke="var(--cp-primary-border)"
            stroke-width="2"
            stroke-dasharray="6 8"
          />
        </svg>
      </div>

      <div class="relative grid grid-cols-1 gap-10 md:grid-cols-3 md:gap-6">
        <div
          v-for="(step, i) in steps"
          :key="step.num"
          class="reveal-step flex flex-col items-center gap-3 text-center"
          :style="`animation-delay: ${i * 120}ms;`"
        >
          <span class="relative flex h-12 w-12 items-center justify-center rounded-full bg-primary-500 font-extrabold text-white shadow-[0_8px_20px_rgba(248,147,29,0.35)]">
            {{ step.num }}
          </span>
          <h3 class="font-bold text-[#1A1A1A]">
            {{ step.title }}
          </h3>
          <p class="max-w-64 text-sm text-[#626262]">
            {{ step.desc }}
          </p>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
const steps = [
  { num: 1, title: '貼上內容或網址', desc: '貼上一段文章、簡報重點，或直接貼網址連結' },
  { num: 2, title: 'AI 出題', desc: 'AI 在幾秒內生成 4 選 1 問答題組，含詳解' },
  { num: 3, title: '分享房間碼開玩', desc: '大家掃碼或輸入房間碼即可加入，即時搶答' }
]
</script>

<style scoped lang="scss">
@keyframes reveal-up {
  from { opacity: 0; transform: translateY(16px); }
  to { opacity: 1; transform: translateY(0); }
}

.reveal-step {
  animation: reveal-up 0.6s cubic-bezier(0.16, 1, 0.3, 1) both;
}

@media (prefers-reduced-motion: reduce) {
  .reveal-step {
    animation: none;
  }
}
</style>
