---
name: vue
description: >-
  Build Vue 3 apps with Composition API, script setup, and Pinia state
  management. Use when writing Vue 3 components with <script setup>, managing
  state with Pinia stores, implementing Vue Router navigation guards, or
  migrating from Options API to Composition API. Do not use for React apps
  (prefer react-typescript) or Svelte apps (prefer svelte).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: vue
  maturity: draft
  risk: low
  tags: [vue, composition-api, pinia, vue-router]
---

# Purpose

Build Vue 3 applications using Composition API with `<script setup>`, Pinia for state management, and Vue Router for navigation.

# When to use this skill

- writing Vue 3 components with `<script setup>` syntax
- managing state with Pinia stores (replacing Vuex)
- implementing Vue Router with navigation guards and dynamic routes
- migrating from Options API to Composition API

# Do not use this skill when

- building React apps — prefer `react-typescript`
- building Svelte apps — prefer `svelte`
- the task is Tailwind styling — prefer `tailwind-shadcn`

# Procedure

1. **Create component** — use `<script setup lang="ts">` for auto-imports and less boilerplate.
2. **Declare reactive state** — `const count = ref(0)` for primitives, `const state = reactive({})` for objects.
3. **Computed values** — `const doubled = computed(() => count.value * 2)`. Auto-tracks dependencies.
4. **Watch effects** — `watch(source, callback)` for explicit watching, `watchEffect()` for auto-tracked effects.
5. **Set up Pinia** — create store with `defineStore('name', () => { ... })` using setup syntax.
6. **Configure Router** — define routes with `createRouter()`. Add `beforeEach` guard for auth checks.
7. **Composables** — extract reusable logic into `use*` functions: `useFetch`, `useAuth`, `useDebounce`.
8. **Type props** — `defineProps<{ title: string; count?: number }>()`. Use `withDefaults` for default values.

# Script setup component

```vue
<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';

const props = withDefaults(defineProps<{
  title: string;
  initialCount?: number;
}>(), { initialCount: 0 });

const emit = defineEmits<{
  (e: 'update', value: number): void;
}>();

const count = ref(props.initialCount);
const doubled = computed(() => count.value * 2);

function increment() {
  count.value++;
  emit('update', count.value);
}

onMounted(() => { console.log('Mounted'); });
</script>

<template>
  <div>
    <h2>{{ title }}</h2>
    <button @click="increment">{{ count }} ({{ doubled }})</button>
  </div>
</template>
```

# Pinia store

```ts
// stores/auth.ts
import { defineStore } from 'pinia';
import { ref, computed } from 'vue';

export const useAuthStore = defineStore('auth', () => {
  const user = ref<User | null>(null);
  const isLoggedIn = computed(() => user.value !== null);

  async function login(email: string, password: string) {
    user.value = await api.login(email, password);
  }

  function logout() {
    user.value = null;
  }

  return { user, isLoggedIn, login, logout };
});
```

# Vue Router with guards

```ts
const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: () => import('./pages/Home.vue') },
    { path: '/dashboard', component: () => import('./pages/Dashboard.vue'), meta: { requiresAuth: true } },
  ],
});

router.beforeEach((to) => {
  const auth = useAuthStore();
  if (to.meta.requiresAuth && !auth.isLoggedIn) {
    return { path: '/login', query: { redirect: to.fullPath } };
  }
});
```

# Decision rules

- Always use `<script setup>` — less boilerplate, better type inference, auto-imports.
- `ref` for primitives, `reactive` for objects — but prefer `ref` for consistency (unwrap with `.value`).
- Pinia setup syntax over options syntax — mirrors Composition API patterns.
- Lazy-load route components — `() => import('./Page.vue')` for code splitting.
- Extract shared logic into composables (`use*` functions) — do not duplicate across components.

# References

- https://vuejs.org/guide/introduction.html
- https://pinia.vuejs.org/
- https://router.vuejs.org/

# Related skills

- `svelte` — alternative frontend framework
- `react-typescript` — React patterns for comparison
- `tailwind-shadcn` — styling Vue components with Tailwind
