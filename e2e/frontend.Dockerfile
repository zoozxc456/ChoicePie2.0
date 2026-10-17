# 只給 e2e 用的 Nuxt 正式版 build；build context 是 choicepie-frontend/。
FROM node:22-slim AS build
WORKDIR /app
RUN corepack enable
COPY package.json pnpm-lock.yaml ./
# postinstall 的 nuxt prepare 需要完整原始碼，先跳過，等 COPY 完再由 build 一併處理。
RUN pnpm install --frozen-lockfile --ignore-scripts
COPY . .
RUN pnpm build

FROM node:22-slim
WORKDIR /app
COPY --from=build /app/.output ./.output
ENV HOST=0.0.0.0 PORT=3000
EXPOSE 3000
CMD ["node", ".output/server/index.mjs"]
