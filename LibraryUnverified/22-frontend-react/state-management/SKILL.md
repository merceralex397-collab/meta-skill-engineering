---
name: state-management
description: >-
  Manage React app state with Zustand, TanStack Query, and URL state. Use when
  choosing between local/global/server state, setting up Zustand stores,
  integrating TanStack Query for server cache, or moving state to URL params.
  Do not use for form-specific validation (prefer forms-validation) or
  Next.js data fetching (prefer nextjs-app-router).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: state-management
  maturity: draft
  risk: low
  tags: [state, zustand, tanstack-query, react]
---

# Purpose

Manage React application state using Zustand for client state, TanStack Query for server cache, and URL state for shareable UI state.

# When to use this skill

- choosing between local, global, server, and URL state
- setting up Zustand stores with slices and selectors
- integrating TanStack Query for data fetching and caching
- moving filter/sort/pagination state to URL params

# Do not use this skill when

- building forms with validation — prefer `forms-validation`
- doing Next.js Server Component data fetching — prefer `nextjs-app-router`
- the task is component typing — prefer `react-typescript`

# Procedure

1. **Classify state** — categorize every piece of state: local (component), global (Zustand), server (TanStack Query), URL (search params).
2. **Start local** — use `useState`/`useReducer` first. Only lift to global when 3+ components need it.
3. **Set up Zustand** — create store with `create()`. Use selectors to prevent unnecessary re-renders.
4. **Add TanStack Query** — `useQuery({ queryKey, queryFn })` for reads, `useMutation` for writes with `onSuccess: () => queryClient.invalidateQueries()`.
5. **Use URL state** — put filters, pagination, and sort in URL params. Use `useSearchParams()` (React Router) or `nuqs` (Next.js).
6. **Combine patterns** — Zustand for UI state (sidebar open, theme), TanStack Query for server data, URL for shareable state.
7. **Test** — verify state updates trigger correct re-renders. Test Zustand stores independently of components.

# State categories

| Category | Tool | Examples |
|----------|------|---------|
| Local | `useState` | Form inputs, toggle, hover |
| Server cache | TanStack Query | API data, paginated lists |
| Global client | Zustand | Auth user, theme, sidebar |
| URL | `useSearchParams` | Filters, sort, page number |

# Zustand store

```tsx
import { create } from 'zustand';

type AuthStore = {
  user: User | null;
  setUser: (user: User | null) => void;
  logout: () => void;
};

const useAuthStore = create<AuthStore>((set) => ({
  user: null,
  setUser: (user) => set({ user }),
  logout: () => set({ user: null }),
}));

// Selector — only re-renders when user changes
const user = useAuthStore((s) => s.user);
```

# TanStack Query

```tsx
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';

function usePosts() {
  return useQuery({ queryKey: ['posts'], queryFn: () => api.getPosts() });
}

function useCreatePost() {
  const qc = useQueryClient();
  return useMutation({
    mutationFn: (data: NewPost) => api.createPost(data),
    onSuccess: () => qc.invalidateQueries({ queryKey: ['posts'] }),
  });
}
```

# Decision rules

- **Default to local** — most state belongs in the component that uses it.
- **Server data is not global state** — use TanStack Query, not Zustand, for API responses.
- Use Zustand selectors — `useStore(s => s.field)` not `useStore()` to avoid full re-renders.
- URL state for anything a user might bookmark or share — filters, tabs, pagination.
- Never duplicate server data in Zustand — TanStack Query is the cache; Zustand is for UI state.

# References

- https://zustand-demo.pmnd.rs/
- https://tanstack.com/query/latest

# Related skills

- `react-typescript` — typed state patterns
- `forms-validation` — form-specific state
- `nextjs-app-router` — server-side data fetching
