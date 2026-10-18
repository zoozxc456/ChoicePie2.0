# ChoicePie E2E

Playwright tests that run against a production-like, multi-node stack started with Docker Compose:

- two backend nodes sharing Postgres and Redis (Redis is also the SignalR backplane);
- an nginx load balancer with **no sticky sessions** (round-robin for REST);
- two Nuxt production builds. Pages served by `web-a` (`localhost:3000`) open their SignalR connection to
  `backend-a` (`localhost:8081`), and pages served by `web-b` (`localhost:3001`) open theirs to `backend-b`
  (`localhost:8082`). This guarantees that a host and a player are on different nodes.

## Run locally

```bash
pnpm install
pnpm exec playwright install chromium
pnpm stack:up      # builds the backend and frontend images, waits until healthy
pnpm test
pnpm stack:down    # also removes the database volume
```

After changing frontend or backend code, run `pnpm stack:up` again to rebuild the images.
The Nuxt build runs only inside Docker, so it writes nothing to `choicepie-frontend/.output`.

`docker compose logs lb` shows which entry port each request used and which backend served it.
