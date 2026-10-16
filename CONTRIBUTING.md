# Contributing to ChoicePie

Thanks for your interest in ChoicePie. Please read this guide before opening an issue or a pull request.

## Before you contribute

ChoicePie is **proprietary software** (see [LICENSE](LICENSE)). Contributions are accepted **by invitation only**.

- Bug reports and feature suggestions are welcome as GitHub issues.
- Pull requests from people who haven't been invited may be closed without review. If you'd like to contribute code, open an issue first or email zoozxc456@gmail.com.
- By submitting a contribution, you agree that it may be used, modified, and distributed as part of ChoicePie under the project's license, and that you have the right to submit it.

## Reporting issues

When filing a bug, include:

- What you did, what you expected, and what happened instead
- Which part is affected (frontend / backend / game hub / admin) and the page or endpoint
- Browser and device for frontend issues; logs or the `ApiResponse` error body for backend issues
- Screenshots or a short recording if it's a UI issue

**Do not report security vulnerabilities in public issues.** Follow [SECURITY.md](SECURITY.md) instead.

## Development workflow

The project follows a GitLab Flow–style branching model:

```
feat/<topic>  ──PR──▶  develop  ──PR──▶  master
```

- `develop` is the integration branch. All feature work branches from and merges back into `develop`.
- `master` holds released code. It is only updated by merging `develop` into it.

Steps:

1. Set up the frontend and backend as described in the [README](README.md#local-development).
2. Create a branch from `develop` named `feat/<topic>` (e.g. `feat/host-offline-banner`).
3. Make focused changes and add tests (see below).
4. Run the same checks CI runs, and make sure they pass.
5. Open a pull request against `develop`.
6. Once `develop` is stable, the maintainer opens a pull request from `develop` to `master` to release.

### Checks

```bash
# Frontend
cd choicepie-frontend
pnpm lint
pnpm typecheck
pnpm test

# Backend (integration tests need Docker)
cd choicepie-backend/ChoicePie.Backend
dotnet build ChoicePie.Backend.sln
dotnet test ChoicePie.Backend.sln
```

## Coding guidelines

Each side has detailed conventions — read them before changing code there:

- Frontend: [choicepie-frontend/CLAUDE.md](choicepie-frontend/CLAUDE.md)
- Backend: [choicepie-backend/ChoicePie.Backend/CLAUDE.md](choicepie-backend/ChoicePie.Backend/CLAUDE.md)

The most important rules:

**Frontend**

- Vue 3 `<script setup lang="ts">`, PascalCase component files in feature-scoped folders.
- Pinia stores use setup-store syntax only.
- All user-facing text goes through i18n (`t()`); update both `zh-TW.json` and `en.json`.
- REST calls go through `useApi()` and the `services/<domain>/client.ts` layer, never directly to the backend.
- Render rankings from `entry.rank`, never from the array index.
- Split large pages into per-state child components.

**Backend**

- Respect the Clean Architecture dependency direction: `WebApi → Infrastructure → Application → Domain → Shared.Kernel`. The Domain layer stays framework-free.
- Model with DDD: aggregates enforce their own invariants; repositories exist only for aggregate roots.
- Commands use repositories; queries go through a per-feature Query Service implemented in `Infrastructure`.
- Communicate state changes via domain events instead of calling other modules directly.
- Register services with DI marker interfaces (`IScopedDependency`, etc.), not manual registration.
- All entities derive from `AuditableEntity<TId>` (or `AppendOnlyAuditableEntity`).

## Tests

- New code needs tests.
- Frontend: Vitest unit tests under `choicepie-frontend/tests/nuxt/unit/**/*.spec.ts`.
- Backend: developed with TDD — start from a failing test. Unit tests use NUnit + NSubstitute + ExpectedObjects; API and persistence integration tests use `WebApplicationFactory` + Testcontainers.

## Commits

- Write commit messages in English, using [Conventional Commits](https://www.conventionalcommits.org/) with a scope where it helps:

  ```
  feat(game): show host-offline banner and let bigscreen watch the room
  fix(auth): ...
  test(webapi): ...
  ```

  Common types: `feat`, `fix`, `refactor`, `test`, `docs`, `chore`, `style`, `ci`.
- Keep each commit focused; stage only the files related to that change.
- Do not add `Co-authored-by` trailers.

## Pull requests

- Keep PRs small and focused on one change.
- Describe what changed, why, and how you tested it; link the related issue.
- Include screenshots for UI changes.
- Update the docs (README, `CLAUDE.md`, `docs/`) when behavior or configuration changes.
- CI must pass before a PR can be merged.
