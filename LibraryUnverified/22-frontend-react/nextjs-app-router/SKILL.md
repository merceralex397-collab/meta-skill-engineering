---
name: nextjs-app-router
description: >-
  Build Next.js App Router apps with RSC, route groups, Server Actions, and
  proper client/server boundaries. Use when creating App Router routes and
  layouts, implementing Server Components vs Client Components, using Server
  Actions for mutations, or configuring route groups and parallel routes.
  Do not use for Pages Router projects or non-Next.js React apps (prefer
  react-typescript).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: nextjs-app-router
  maturity: draft
  risk: low
  tags: [nextjs, app-router, rsc, server-actions]
---

# Purpose

Build Next.js App Router applications with React Server Components, Server Actions, route groups, and correct client/server boundaries.

# When to use this skill

- creating routes, layouts, and pages with App Router conventions
- deciding which components should be Server vs Client Components
- implementing data mutations with Server Actions
- using route groups, parallel routes, or intercepting routes

# Do not use this skill when

- working with Next.js Pages Router — different patterns
- building non-Next.js React apps — prefer `react-typescript`
- doing state management — prefer `state-management`

# Procedure

1. **Create route structure** — `app/page.tsx` (home), `app/dashboard/page.tsx` (route). Use `layout.tsx` for shared UI.
2. **Default to Server Components** — only add `'use client'` when you need hooks, event handlers, or browser APIs.
3. **Fetch data in Server Components** — use `async` server components with direct `fetch()` or DB calls. No `useEffect` for data fetching.
4. **Implement mutations** — create Server Actions with `'use server'` in a separate file. Call from `<form action={myAction}>` or `useTransition`.
5. **Use route groups** — `(marketing)/` and `(app)/` for separate layouts without affecting URL. `(auth)/login/page.tsx`.
6. **Handle loading/error** — add `loading.tsx` for Suspense fallback and `error.tsx` (client component) for error boundaries per route.
7. **Configure metadata** — export `metadata` object or `generateMetadata()` function from `page.tsx` or `layout.tsx`.
8. **Deploy** — verify `next build` succeeds. Check static vs dynamic rendering in build output.

# Server vs Client Components

```
Server Component (default):      Client Component ('use client'):
- async/await for data           - useState, useEffect, useRef
- Direct DB/API access           - onClick, onChange handlers
- Access secrets/env vars        - Browser APIs (localStorage, etc.)
- Zero JS sent to client         - Interactive UI elements
- Cannot use hooks               - Cannot be async
```

# Server Actions

```tsx
// app/actions.ts
'use server';
import { revalidatePath } from 'next/cache';

export async function createPost(formData: FormData) {
  const title = formData.get('title') as string;
  await db.post.create({ data: { title } });
  revalidatePath('/posts');
}

// app/posts/new/page.tsx
import { createPost } from '@/app/actions';

export default function NewPost() {
  return (
    <form action={createPost}>
      <input name="title" required />
      <button type="submit">Create</button>
    </form>
  );
}
```

# Route groups and parallel routes

```
app/
  (marketing)/
    layout.tsx          # marketing layout
    page.tsx            # /
    about/page.tsx      # /about
  (dashboard)/
    layout.tsx          # dashboard layout (with sidebar)
    overview/page.tsx   # /overview
    @modal/(.)edit/[id]/page.tsx  # intercepting route for modal
```

# Decision rules

- Server Components by default — only opt into `'use client'` when required.
- Colocate Server Actions in `actions.ts` files — not inline in components.
- Use `revalidatePath()` or `revalidateTag()` after mutations — do not manually refetch.
- Put `loading.tsx` at route segment level — Next.js wraps the page in `Suspense` automatically.
- Prefer `generateMetadata` over `<Head>` — it supports dynamic metadata with data fetching.

# References

- https://nextjs.org/docs/app
- https://nextjs.org/docs/app/building-your-application/data-fetching/server-actions-and-mutations

# Related skills

- `react-typescript` — component typing patterns
- `state-management` — client-side state alongside RSC
- `testing-web` — testing Server Components and Actions
