/**
 * ChoicePie — Nuxt UI v3 Theme 設定
 * 文件：https://ui.nuxt.com/getting-started/theme
 */

export default defineAppConfig({
  ui: {
    colors: {
      primary: 'primary', // 對應 @theme 裡定義的 --color-primary-*
      neutral: 'zinc'
    },

    /* ── 全域圓角覆蓋 ── */
    button: {
      defaultVariants: {
        color: 'primary'
      }
    },

    /* ── Input 風格 ── */
    input: {
      defaultVariants: {
        color: 'primary',
        variant: 'outline'
      }
    },

    /* ── Modal / Slideover ── */
    modal: {
      slots: {
        overlay: 'bg-black/50',
        content: 'rounded-[20px] shadow-xl'
      }
    },

    /* ── Card ── */
    card: {
      slots: {
        root: 'bg-white border border-[var(--cp-border)] rounded-[var(--cp-radius-lg)] shadow-[var(--cp-shadow-md)]'
      }
    },

    /* ── Badge ── */
    badge: {
      defaultVariants: {
        color: 'primary',
        variant: 'soft'
      }
    },

    /* ── Toast 通知 ── */
    toast: {
      defaultVariants: {
        color: 'primary'
      }
    }
  }
})
