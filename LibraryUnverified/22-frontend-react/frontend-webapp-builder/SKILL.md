---
name: frontend-webapp-builder
description: "Scaffold, architect, and configure frontend web applications from scratch. Trigger: 'create a new frontend app', 'scaffold a React/Vue/Svelte/SolidJS project', 'set up a web app with Vite', 'initialize frontend project', 'configure bundler and tooling'. Do NOT use for backend API work, mobile-native apps, static site generators without client-side interactivity, or framework-specific deep patterns already covered by solidjs-patterns or tauri-solidjs."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: frontend-webapp-builder
  maturity: draft
  risk: low
  tags: [frontend, scaffolding, vite, react, vue, svelte, solidjs, typescript, webapp]
---

# Purpose

Guide the scaffolding and architectural setup of frontend web applications — from framework selection through build optimization — producing a production-ready project structure with consistent tooling, testing, and component conventions.

# When to use this skill

Use this skill when:

- the user needs to create a new frontend web application from scratch
- a project requires framework selection advice with justification (React vs Vue vs SolidJS vs Svelte)
- the task involves configuring Vite, TypeScript, ESLint, Prettier, or other frontend tooling
- component architecture decisions need to be made (directory layout, design system organization)
- state management, routing, or build optimization strategy needs to be defined
- an existing project needs restructuring to follow modern frontend conventions

# Do not use this skill when

- the task is about SolidJS-specific reactive patterns — use `solidjs-patterns` instead
- the task is about a Tauri desktop app with a SolidJS frontend — use `tauri-solidjs` instead
- the work is purely backend (API routes, database queries, server configuration)
- the project is a static marketing site with no client-side interactivity
- the task is about an existing app's runtime bugs rather than architecture or scaffolding

# Operating procedure

## Step 1 — Clarify requirements and constraints

Gather before making any decisions:
- **App type**: SPA, SSR, or hybrid (SSR with client hydration)?
- **Scale expectations**: solo project, small team, or large org with multiple teams?
- **Existing ecosystem**: is there a backend already? What language/framework?
- **Performance targets**: time-to-interactive budget, bundle size constraints?
- **Team familiarity**: what frameworks does the team already know?

## Step 2 — Select framework using decision tree

Apply this decision tree in order:

1. **Team already has strong expertise in a framework** → use that framework (switching cost > marginal benefit).
2. **Need the largest ecosystem and hiring pool** → React (with Vite, not CRA).
3. **Incremental adoption into existing server-rendered app** → Vue (progressive integration via single-file components).
4. **Performance is the primary constraint (real-time dashboards, data-heavy UIs)** → SolidJS (fine-grained reactivity, no virtual DOM overhead).
5. **Smallest bundle size and compiler-first approach** → Svelte (compiles to vanilla JS, minimal runtime).
6. **SSR/SEO is critical with React ecosystem** → Next.js. With Vue → Nuxt. With Svelte → SvelteKit. With SolidJS → SolidStart.

If none of the above clearly applies, default to **React + Vite + TypeScript** as the safest general-purpose choice.

## Step 3 — Scaffold the project

Use Vite as the default bundler for all frameworks:

```bash
# React
npm create vite@latest my-app -- --template react-ts

# Vue
npm create vite@latest my-app -- --template vue-ts

# SolidJS
npx degit solidjs/templates/ts my-app

# Svelte
npm create vite@latest my-app -- --template svelte-ts
```

Immediately after scaffolding:

1. **TypeScript** — enable `strict: true` in `tsconfig.json`. Set `noUncheckedIndexedAccess: true`.
2. **ESLint** — install framework-specific config (e.g., `eslint-plugin-react-hooks`, `eslint-plugin-vue`, `eslint-plugin-solid`).
3. **Prettier** — add `.prettierrc` with `semi: true`, `singleQuote: true`, `trailingComma: "all"`. Add `prettier-plugin-tailwindcss` if using Tailwind.
4. **Git hooks** — set up `lint-staged` + `husky` for pre-commit linting.
5. **EditorConfig** — add `.editorconfig` for consistent formatting across editors.

## Step 4 — Define directory structure

Use the co-location pattern with atomic design principles:

```
src/
├── components/
│   ├── atoms/          # Button, Input, Icon, Badge
│   ├── molecules/      # SearchBar, FormField, NavItem
│   ├── organisms/      # Header, Sidebar, DataTable
│   ├── templates/      # PageLayout, DashboardLayout
│   └── pages/          # HomePage, SettingsPage
├── hooks/              # (or composables/ for Vue, primitives/ for SolidJS)
├── stores/             # State management (only if needed)
├── services/           # API client, auth service
├── utils/              # Pure utility functions
├── types/              # Shared TypeScript types/interfaces
├── assets/             # Static assets (images, fonts)
├── styles/             # Global styles, theme tokens
└── test/               # Test utilities, mocks, fixtures
```

Each component directory uses co-location:

```
components/atoms/Button/
├── Button.tsx
├── Button.module.css    # or Button.styles.ts for CSS-in-JS
├── Button.test.tsx
├── Button.stories.tsx   # optional: Storybook
└── index.ts             # barrel export
```

## Step 5 — State management (progressive complexity)

Follow the escalation ladder — do not jump to external stores prematurely:

1. **Local component state** — `useState` (React), `ref`/`reactive` (Vue), `createSignal` (SolidJS), `let` with `$:` (Svelte). Use this for UI-only state (form inputs, toggles, modals).
2. **Lifted state / prop drilling** — lift state to nearest common ancestor. Acceptable for 2–3 levels of depth.
3. **Context / Signals** — `createContext` (React/SolidJS), `provide`/`inject` (Vue), Svelte stores. Use for app-wide state accessed by many components (theme, auth user, locale).
4. **External store** — only when context becomes unwieldy:
   - React → Zustand (simple) or Redux Toolkit (complex with middleware needs)
   - Vue → Pinia
   - SolidJS → built-in `createStore`
   - Svelte → built-in writable stores

**Decision rule**: if you can describe the state's scope in one sentence and it fits in one component subtree, you don't need a store.

## Step 6 — Routing configuration

- **File-based routing** (Next.js, Nuxt, SvelteKit, SolidStart) — prefer this for new projects; convention over configuration.
- **Config-based routing** (React Router, Vue Router, @solidjs/router) — use when you need explicit control or aren't using a meta-framework.

Must-have patterns:
- **Layout routes**: shared chrome (header, sidebar) defined once, nested in route tree.
- **Protected routes**: auth guard as a layout wrapper, redirect to login if unauthenticated.
- **Code splitting**: every route loaded via `lazy()` (React), `defineAsyncComponent` (Vue), or dynamic `import()`.
- **Error boundaries**: per-route error boundaries so one route's crash doesn't take down the app.
- **Loading states**: Suspense boundaries at route level for skeleton/spinner display during lazy loading.

## Step 7 — Build optimization

Configure and verify:

1. **Tree shaking** — ensure `sideEffects: false` in `package.json` where applicable; use named imports, not namespace imports.
2. **Code splitting** — route-level splitting is mandatory; component-level splitting for heavy components (charts, editors, maps).
3. **Asset optimization** — use `vite-plugin-imagemin` or equivalent; inline SVGs as components; use `woff2` for fonts.
4. **Bundle analysis** — run `npx vite-bundle-visualizer` after build; set budget: <200KB JS for initial load (gzipped).
5. **Lighthouse targets** — Performance ≥ 90, Accessibility ≥ 90, Best Practices ≥ 90, SEO ≥ 90.
6. **Caching strategy** — content-hashed filenames for assets (Vite default); configure `Cache-Control` headers for static assets.

## Step 8 — Testing strategy

Three tiers, each with a clear purpose:

| Tier | Tool | What to test | Coverage target |
|------|------|-------------|----------------|
| Unit | Vitest | Pure functions, hooks, utilities, store logic | High (>80%) |
| Component | Vitest + Testing Library | Component rendering, user interactions, accessibility | Medium (critical paths) |
| E2E | Playwright (preferred) or Cypress | Full user flows: login, checkout, form submission | Low (happy paths + critical error paths) |

Rules:
- Test behavior, not implementation details.
- Use `screen.getByRole()` over `getByTestId()` — tests should mirror how users interact.
- Mock API calls at the network level (`msw`) not at the module level.
- Run component tests in jsdom (fast) not in a real browser (slow) unless testing browser-specific behavior.

# Decision rules

- **Framework choice is sticky** — once a team is productive in a framework, the cost of switching almost never justifies marginal performance gains. Only recommend switching for greenfield projects.
- **TypeScript is non-negotiable** — every new frontend project must use TypeScript with strict mode enabled. The long-term maintenance cost of untyped code always exceeds the short-term velocity gain.
- **Vite over Webpack** — for new projects, Vite is the default bundler. Only use Webpack if there's an existing Webpack config with custom loaders that can't be migrated.
- **Co-location over separation** — keep component files (code, styles, tests, types) together. Never organize by file type (all CSS in one dir, all tests in another).
- **Progressive state complexity** — start with the simplest state solution. Escalate only when the current approach creates measurable friction (prop drilling >3 levels, state updates from >3 unrelated components).
- **Performance budgets are constraints, not aspirations** — set them in CI. Fail the build if the JS bundle exceeds the budget.

# Output requirements

Produce a structured deliverable with these sections:

1. **Project Scaffold** — framework choice with justification, `package.json` scripts, directory tree, tooling config files (tsconfig, eslint, prettier).
2. **Component Hierarchy** — atomic design classification of planned components, co-location structure, barrel export conventions.
3. **State Architecture** — diagram of state ownership (which component owns which state), escalation plan for when local state outgrows its scope.
4. **Routing Map** — route tree with layout nesting, protected routes marked, code-splitting points identified.
5. **Build Configuration** — Vite config with plugins, bundle budgets, environment variable handling, production vs development differences.
6. **Test Setup** — Vitest config, Testing Library setup, MSW handlers for API mocking, Playwright config for E2E, CI integration commands.

# Anti-patterns

- **Premature state management**: reaching for Redux/Zustand before trying local state and context. Adds indirection without solving a real problem.
- **Mega-components**: components exceeding 200 lines that handle multiple responsibilities. Split by extracting sub-components and custom hooks.
- **No TypeScript**: starting a project in plain JavaScript "to move fast." The time saved is repaid with interest in debugging and onboarding costs within weeks.
- **Missing error boundaries**: a single unhandled error crashes the entire app. Every route and every async data-fetching component needs an error boundary.
- **No loading states**: users see blank screens during data fetches or route transitions. Every Suspense boundary needs a fallback.
- **Barrel-file explosion**: re-exporting everything from a single `index.ts` at the root of `components/` defeats tree shaking. Keep barrel exports at the component directory level.
- **CSS-in-JS at scale without extraction**: runtime CSS-in-JS (styled-components, Emotion) adds to JS bundle and hurts performance. Prefer CSS Modules, Tailwind, or compile-time solutions (vanilla-extract, Panda CSS).
- **Testing implementation details**: testing that `useState` was called with a specific value instead of testing that the UI shows the correct output. Tests break on refactors without catching real bugs.

# Related skills

- `solidjs-patterns` — deep SolidJS reactivity patterns and idioms
- `tauri-solidjs` — Tauri desktop apps with SolidJS frontends
- `fastapi-patterns` — backend API that this frontend might consume

# Failure handling

- If framework requirements are ambiguous, present the decision tree and ask the user to confirm constraints before proceeding.
- If the project has an existing build system (Webpack, Parcel) that can't easily be replaced, adapt the scaffold to work within it rather than forcing a migration.
- If performance budgets can't be met with the chosen framework, surface the tradeoff explicitly and recommend either a different framework or a hybrid approach (e.g., islands architecture).
- If the team has no testing culture, start with E2E tests for critical paths only — trying to enforce full unit test coverage on an untested codebase will fail.
