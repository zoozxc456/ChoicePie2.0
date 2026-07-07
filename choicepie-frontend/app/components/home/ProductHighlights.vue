<template>
  <div
    ref="wrapRef"
    class="relative"
  >
    <div
      v-for="(feature, i) in features"
      :key="feature.title"
      class="feature-card sticky top-0 flex min-h-dvh flex-col items-center justify-center overflow-hidden px-6 py-16 lg:px-16"
      :class="feature.bgClass"
      :style="i < features.length - 1 ? `z-index: ${i + 1};` : `z-index: ${features.length};`"
    >
      <div class="mx-auto flex w-full max-w-7xl flex-col items-center gap-14 lg:flex-row lg:justify-between lg:gap-10">
        <!-- 文字 -->
        <div
          class="flex max-w-md flex-col gap-3 text-center lg:text-left"
          :class="feature.textClass"
        >
          <span class="text-xs font-bold uppercase tracking-[0.14em] opacity-60">
            {{ feature.eyebrow }}
          </span>
          <h3 class="text-4xl font-extrabold leading-[1.1] md:text-5xl">
            {{ feature.title }}
          </h3>
          <p class="text-base leading-relaxed opacity-70">
            {{ feature.desc }}
          </p>
        </div>

        <!-- 唯一視覺主體：大尺寸瀏覽器視窗風格 mockup -->
        <div class="w-full max-w-2xl shrink-0">
          <div class="overflow-hidden rounded-2xl bg-white shadow-[0_40px_100px_rgba(0,0,0,0.4)]">
            <!-- 瀏覽器頂欄，強化「這是真實畫面」的說服力 -->
            <div class="flex items-center gap-2 border-b border-neutral-100 bg-neutral-50 px-4 py-2.5">
              <span class="h-2.5 w-2.5 rounded-full bg-[#ff5f57]" />
              <span class="h-2.5 w-2.5 rounded-full bg-[#febc2e]" />
              <span class="h-2.5 w-2.5 rounded-full bg-[#28c840]" />
              <span class="ml-3 truncate text-xs text-neutral-400">choicepie.app</span>
            </div>

            <!-- AI 秒速出題：文字轉題目 -->
            <div
              v-if="feature.key === 'ai'"
              class="grid grid-cols-1 gap-0 sm:grid-cols-2"
            >
              <div class="flex flex-col gap-3 border-b border-neutral-100 p-6 sm:border-b-0 sm:border-r">
                <span class="text-xs font-semibold text-neutral-400">貼上素材</span>
                <div class="flex-1 rounded-xl bg-neutral-50 p-4 font-mono text-xs leading-relaxed text-neutral-500">
                  「派對甜點該怎麼配才對味？氣泡水配馬卡龍是絕配，酸甜氣泡能中和甜膩，紅酒配起司則偏傳統...」
                </div>
              </div>
              <div class="flex flex-col gap-3 p-6">
                <span class="text-xs font-semibold text-primary-500">✨ AI 自動生成</span>
                <p class="text-base font-bold text-neutral-900">
                  派對甜點該怎麼配才對味？
                </p>
                <div class="flex flex-1 flex-col justify-center gap-2.5">
                  <div
                    v-for="opt in aiOptions"
                    :key="opt.letter"
                    class="flex items-center gap-3 rounded-xl px-4 py-3 text-sm font-medium"
                    :class="opt.correct ? 'bg-success-50 ring-1 ring-success-500 text-success-800' : 'bg-neutral-50 text-neutral-700'"
                  >
                    <span
                      class="flex h-6 w-6 shrink-0 items-center justify-center rounded-full text-xs font-bold text-white"
                      :class="opt.correct ? 'bg-success-500' : 'bg-neutral-300'"
                    >{{ opt.letter }}</span>
                    {{ opt.text }}
                    <span
                      v-if="opt.correct"
                      class="ml-auto text-success-500"
                    >✓</span>
                  </div>
                </div>
              </div>
            </div>

            <!-- 免安裝加入：房間碼 + QR -->
            <div
              v-else-if="feature.key === 'join'"
              class="grid grid-cols-1 gap-8 p-10 sm:grid-cols-[auto_1fr] sm:items-center"
            >
              <div class="mx-auto flex h-48 w-48 items-center justify-center rounded-2xl bg-neutral-900 font-mono text-xs text-white/70">
                QR CODE
              </div>
              <div class="text-center sm:text-left">
                <p class="text-xs font-semibold text-neutral-400">
                  房間碼
                </p>
                <p class="text-6xl font-black tracking-widest text-primary-500">
                  482913
                </p>
                <p class="mt-3 text-sm text-neutral-500">
                  掃碼或輸入房間碼，3 秒內加入搶答
                </p>
              </div>
            </div>

            <!-- 即時排名回饋 -->
            <div
              v-else
              class="p-6"
            >
              <div
                v-for="row in rankRows"
                :key="row.rank"
                class="flex items-center gap-4 px-3 py-4"
                :class="row.rank < rankRows.length ? 'border-b border-neutral-100' : ''"
              >
                <span
                  class="flex h-9 w-9 shrink-0 items-center justify-center rounded-full text-sm font-bold text-white"
                  :class="row.badgeClass"
                >{{ row.rank }}</span>
                <span class="flex-1 text-base font-semibold text-neutral-800">{{ row.name }}</span>
                <span class="text-base font-bold tabular-nums text-primary-500">{{ row.score }}</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script lang="ts" setup>
import { gsap } from 'gsap'
import { ScrollTrigger } from 'gsap/ScrollTrigger'

const features = [
  {
    key: 'ai',
    eyebrow: 'AI 出題引擎',
    title: '別再自己想題目了',
    desc: '貼上一篇文章、簡報重點，或直接丟一個網址連結，ChoicePie 就能自動整理成一組 4 選 1 問答題，連詳解都幫你寫好。',
    bgClass: 'bg-[linear-gradient(135deg,#1a1a2e_0%,#2d3748_100%)]',
    textClass: 'text-white'
  },
  {
    key: 'join',
    eyebrow: '零門檻加入',
    title: '沒有人會被卡在下載畫面',
    desc: '玩家不用下載任何 APP、不用註冊帳號。掃一下 QR Code，或輸入六位數房間碼，馬上就能加入搶答，聚會現場零等待。',
    bgClass: 'bg-primary-500',
    textClass: 'text-white'
  },
  {
    key: 'rank',
    eyebrow: '即時互動回饋',
    title: '讓全場為了排名尖叫',
    desc: '每題結束的瞬間，正確答案、詳解和最新排名同步公布。看著名次即時洗牌，搶答的緊張感和成就感都在這一刻爆發。',
    bgClass: 'bg-[#FBFAF7]',
    textClass: 'text-[#1A1A1A]'
  }
]

const aiOptions = [
  { letter: 'A', text: '紅酒配起司', correct: false },
  { letter: 'B', text: '氣泡水配馬卡龍', correct: true },
  { letter: 'C', text: '啤酒配鹹酥雞', correct: false }
]

const rankRows = [
  { rank: 1, name: '小美', score: 320, badgeClass: 'bg-primary-500' },
  { rank: 2, name: 'Leo', score: 280, badgeClass: 'bg-info-500' },
  { rank: 3, name: 'Ivy', score: 240, badgeClass: 'bg-warning-500' }
]

const wrapRef = ref<HTMLElement | null>(null)
let ctx: gsap.Context | null = null

onMounted(() => {
  if (window.matchMedia('(prefers-reduced-motion: reduce)').matches) return
  if (!wrapRef.value) return

  gsap.registerPlugin(ScrollTrigger)

  ctx = gsap.context(() => {
    const cards = gsap.utils.toArray<HTMLElement>('.feature-card')
    cards.forEach((card, i) => {
      if (i === cards.length - 1) return
      gsap.to(card, {
        scale: 0.92,
        opacity: 0.4,
        ease: 'none',
        scrollTrigger: {
          trigger: cards[i + 1],
          start: 'top bottom',
          end: 'top top',
          scrub: true
        }
      })
    })
  }, wrapRef.value)
})

onUnmounted(() => {
  ctx?.revert()
})
</script>

<style scoped lang="scss">
.feature-card {
  will-change: transform, opacity;
}
</style>
