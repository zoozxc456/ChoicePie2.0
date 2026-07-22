/**
 * ChoicePie — Nuxt UI v3 Theme 設定
 * 文件：https://ui.nuxt.com/getting-started/theme
 */

export default defineAppConfig({
  ui: {
    colors: {
      primary: 'primary', // 對應 @theme 裡定義的 --color-primary-*
      secondary: 'secondary', // 對應 --color-secondary-*
      success: 'success', // 對應 --color-success-*（cp-success）
      warning: 'warning', // 對應 --color-warning-*（cp-warning）
      error: 'error', // 對應 --color-error-*（cp-danger）
      info: 'info', // 對應 --color-info-*（cp-info）
      neutral: 'neutral' // 對應 --color-neutral-*（cp-text / cp-border 灰階）
    },

    /* ── 全域圓角覆蓋 ── */
    button: {
      defaultVariants: {
        color: 'primary'
      }
    },

    /* ── Input 風格 ── */
    input: {
      slots: {
        base: 'rounded-xl'
      },
      defaultVariants: {
        color: 'primary',
        variant: 'outline'
      }
    },

    /* ── Modal / Slideover ── */
    modal: {
      slots: {
        overlay: 'bg-black/50',
        content: 'rounded-cp-xl shadow-xl'
      }
    },

    /* ── Card ── */
    card: {
      slots: {
        root: 'bg-white border border-neutral-200 rounded-cp-lg shadow-cp-md'
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
