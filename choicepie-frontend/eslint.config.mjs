import { createConfigForNuxt } from '@nuxt/eslint-config/flat'

export default createConfigForNuxt({
  features: {
    stylistic: false,
  },
}).append({
  rules: {
    // Vue
    'vue/multi-word-component-names': 'off',
    'vue/no-v-html': 'warn',

    // TypeScript
    '@typescript-eslint/no-explicit-any': 'warn',
    '@typescript-eslint/no-unused-vars': ['error', { argsIgnorePattern: '^_' }],

    // Common
    'no-console': ['warn', { allow: ['warn', 'error'] }],
  },
})
