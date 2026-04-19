---
name: svelte
description: >-
  Build Svelte 5 apps with runes ($state, $derived, $effect) and SvelteKit
  routing. Use when creating Svelte 5 components with runes, setting up
  SvelteKit routes and load functions, migrating from Svelte 4 stores to
  runes, or implementing SvelteKit form actions. Do not use for React apps
  (prefer react-typescript) or Vue apps (prefer vue).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: svelte
  maturity: draft
  risk: low
  tags: [svelte, sveltekit, runes]
---

# Purpose

Build Svelte 5 applications using runes ($state, $derived, $effect) for reactivity and SvelteKit for routing, SSR, and form actions.

# When to use this skill

- creating Svelte 5 components with runes-based reactivity
- setting up SvelteKit routes with `+page.svelte`, `+layout.svelte`, and load functions
- migrating from Svelte 4 stores to Svelte 5 runes
- implementing SvelteKit form actions for mutations

# Do not use this skill when

- building React apps — prefer `react-typescript`
- building Vue apps — prefer `vue`
- the task is state management patterns (React-specific) — prefer `state-management`

# Procedure

1. **Create component** — `<script>` block with runes for state, HTML template, optional `<style>` block.
2. **Use `$state`** — declare reactive state: `let count = $state(0)`. Replaces Svelte 4's `let count = 0` reactive declarations.
3. **Use `$derived`** — computed values: `let doubled = $derived(count * 2)`. Replaces `$:` reactive statements.
4. **Use `$effect`** — side effects: `$effect(() => { console.log(count); })`. Runs when dependencies change.
5. **Set up SvelteKit routes** — `src/routes/+page.svelte` (page), `+page.server.ts` (server load), `+layout.svelte` (layout).
6. **Load data** — export `load` function from `+page.server.ts`. Returns data accessible via `data` prop in page.
7. **Handle forms** — export `actions` from `+page.server.ts`. Use `<form method="POST" use:enhance>`.
8. **Deploy** — choose adapter: `adapter-auto`, `adapter-node`, `adapter-static`, `adapter-vercel`.

# Svelte 5 runes

```svelte
<script>
  let count = $state(0);
  let doubled = $derived(count * 2);

  $effect(() => {
    document.title = `Count: ${count}`;
  });

  function increment() {
    count++;
  }
</script>

<button onclick={increment}>
  Count: {count} (doubled: {doubled})
</button>
```

# SvelteKit routing

```
src/routes/
  +layout.svelte           # root layout
  +page.svelte             # /
  blog/
    +page.svelte           # /blog
    +page.server.ts        # server load function
    [slug]/
      +page.svelte         # /blog/:slug
      +page.server.ts      # dynamic load
```

# SvelteKit form actions

```ts
// +page.server.ts
export const actions = {
  create: async ({ request }) => {
    const data = await request.formData();
    const title = data.get('title');
    await db.post.create({ data: { title } });
    return { success: true };
  },
};
```

```svelte
<!-- +page.svelte -->
<script>
  import { enhance } from '$app/forms';
</script>

<form method="POST" action="?/create" use:enhance>
  <input name="title" required />
  <button type="submit">Create</button>
</form>
```

# Decision rules

- Use `$state` for mutable reactive values — not bare `let` (Svelte 5 requires explicit runes).
- Use `$derived` over `$effect` for computed values — effects are for side effects only.
- Keep load functions in `+page.server.ts` for data that needs secrets or DB access.
- Use `use:enhance` on forms — enables progressive enhancement without full page reload.
- Prefer SvelteKit form actions over API routes for mutations — built-in CSRF protection.

# References

- https://svelte.dev/docs/svelte/overview
- https://svelte.dev/docs/kit/introduction

# Related skills

- `vue` — alternative frontend framework
- `react-typescript` — React patterns for comparison
- `tailwind-shadcn` — styling Svelte components with Tailwind
