---
name: solidjs-patterns
description: "Write idiomatic SolidJS code using fine-grained reactivity, control flow components, stores, and resources. Trigger: 'SolidJS component', 'createSignal', 'createStore', 'SolidJS routing', 'reactive state in Solid', 'Solid control flow', '<For>', '<Show>', 'SolidJS Suspense', 'convert React to SolidJS'. Do NOT use for general frontend scaffolding (use frontend-webapp-builder), Tauri-specific integration (use tauri-solidjs), or other frameworks (React, Vue, Svelte)."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: solidjs-patterns
  maturity: draft
  risk: low
  tags: [solidjs, reactivity, signals, stores, fine-grained, frontend, typescript, jsx]
---

# Purpose

Guide the writing of idiomatic SolidJS code — leveraging its fine-grained reactivity model, control flow primitives, store system, and resource management. This skill prevents the most common mistake: writing SolidJS as if it were React.

# When to use this skill

Use this skill when:

- writing new SolidJS components, stores, or reactive logic
- debugging reactivity issues (values not updating, effects running unexpectedly)
- converting React components to SolidJS
- choosing between signals, stores, memos, and resources for a given use case
- implementing data fetching with `createResource` and Suspense
- setting up routing with `@solidjs/router`
- optimizing SolidJS app performance or understanding why "it's already fast"

# Do not use this skill when

- the task is general frontend project scaffolding (framework selection, bundler config) — use `frontend-webapp-builder`
- the task involves Tauri desktop app integration with SolidJS — use `tauri-solidjs`
- the user is working with React, Vue, or Svelte — those are different reactivity models
- the task is purely CSS/styling with no reactive logic involved

# Operating procedure

## Step 1 — Understand the reactivity model

SolidJS uses **fine-grained reactivity**, not virtual DOM diffing. This is the fundamental difference from React:

- **React**: re-runs the entire component function on state change, diffs the virtual DOM, patches the real DOM.
- **SolidJS**: component functions run **once**. Only the specific DOM nodes that depend on changed signals update. There is no re-render.

This means:
- Component functions are setup functions, not render functions.
- Code at the top level of a component runs once during creation, never again.
- Only code inside reactive contexts (`createEffect`, `createMemo`, JSX expressions) tracks and re-executes.

```tsx
// This console.log runs ONCE, not on every count change
function Counter() {
  const [count, setCount] = createSignal(0);
  console.log("Component setup — runs once");

  // This JSX expression re-evaluates count() reactively
  return <button onClick={() => setCount(c => c + 1)}>
    Count: {count()}
  </button>;
}
```

## Step 2 — Use signals, memos, and effects correctly

### Signals — reactive state atoms

```tsx
import { createSignal } from "solid-js";

const [value, setValue] = createSignal(initialValue);

// Read: call the getter function (NOT just reference it)
console.log(value());  // ✅ Correct — reads and tracks
console.log(value);    // ❌ Wrong — references the getter function itself

// Write: call the setter
setValue(newValue);           // Replace
setValue(prev => prev + 1);  // Update based on previous
```

### Memos — derived reactive values (cached)

Use `createMemo` when a derived value is expensive to compute or used in multiple places:

```tsx
import { createMemo } from "solid-js";

const doubled = createMemo(() => count() * 2);
// doubled() is cached — recomputes only when count() changes
```

**Decision rule**: if a derivation is just `signal() + 1` used in one place, inline it in JSX. If it's expensive or used in 3+ places, use `createMemo`.

### Effects — side effects that react to state changes

```tsx
import { createEffect } from "solid-js";

createEffect(() => {
  // Runs whenever any signal read inside changes
  console.log("Count changed to:", count());
  // Use for: DOM manipulation, logging, localStorage sync, external API calls
});
```

**Critical rule**: never create signals inside effects. Signals are state; effects are reactions to state. Creating state inside a reaction creates infinite loops or orphaned state.

## Step 3 — Handle props correctly

**Never destructure props** — this is the #1 SolidJS mistake by React developers.

```tsx
// ❌ WRONG — breaks reactivity. Values are captured at call time, never update.
function Greeting({ name, age }) {
  return <p>Hello {name}, age {age}</p>;
}

// ✅ CORRECT — access props as properties, reactivity preserved
function Greeting(props) {
  return <p>Hello {props.name}, age {props.age}</p>;
}
```

Use `mergeProps` for defaults:

```tsx
import { mergeProps } from "solid-js";

function Button(props) {
  const merged = mergeProps({ variant: "primary", size: "md" }, props);
  return <button class={`btn-${merged.variant} btn-${merged.size}`}>
    {merged.children}
  </button>;
}
```

Use `splitProps` to separate groups:

```tsx
import { splitProps } from "solid-js";

function Input(props) {
  const [local, inputProps] = splitProps(props, ["label", "error"]);
  return (
    <div>
      <label>{local.label}</label>
      <input {...inputProps} />
      <Show when={local.error}><span class="error">{local.error}</span></Show>
    </div>
  );
}
```

## Step 4 — Use control flow components (not JS expressions)

SolidJS provides control flow components that preserve reactivity. Using JavaScript ternaries or `Array.map` breaks the reactive model.

### `<Show>` — conditional rendering

```tsx
import { Show } from "solid-js";

// ✅ Correct — reactive, preserves DOM nodes when condition is stable
<Show when={isLoggedIn()} fallback={<LoginForm />}>
  <Dashboard />
</Show>

// ❌ Wrong — ternary recreates DOM nodes on every change
{isLoggedIn() ? <Dashboard /> : <LoginForm />}
```

### `<For>` — reactive list rendering

```tsx
import { For } from "solid-js";

// ✅ Correct — keyed by reference, only updates changed items
<For each={items()}>
  {(item, index) => <li>{index()}: {item.name}</li>}
</For>

// ❌ Wrong — Array.map recreates all DOM nodes on any list change
{items().map((item, i) => <li>{i}: {item.name}</li>)}
```

Use `<Index>` when items are primitives (strings, numbers) and the array position is the stable identity:

```tsx
import { Index } from "solid-js";

<Index each={names()}>
  {(name, index) => <li>{index}: {name()}</li>}  {/* note: name is a signal */}
</Index>
```

### `<Switch>` / `<Match>` — multi-branch conditional

```tsx
import { Switch, Match } from "solid-js";

<Switch fallback={<p>Unknown status</p>}>
  <Match when={status() === "loading"}><Spinner /></Match>
  <Match when={status() === "error"}><ErrorDisplay /></Match>
  <Match when={status() === "success"}><DataView /></Match>
</Switch>
```

### `<Dynamic>` — dynamic component selection

```tsx
import { Dynamic } from "solid-js/web";

const components = { home: HomePage, about: AboutPage, settings: SettingsPage };

<Dynamic component={components[currentPage()]} />
```

## Step 5 — Manage complex state with stores

Use `createStore` for nested/complex state (objects, arrays of objects):

```tsx
import { createStore } from "solid-js/store";

const [state, setState] = createStore({
  user: { name: "Alice", settings: { theme: "dark" } },
  todos: [
    { id: 1, text: "Learn SolidJS", done: false },
    { id: 2, text: "Build app", done: false },
  ],
});

// Path-based updates — only affected DOM nodes re-render
setState("user", "settings", "theme", "light");
setState("todos", todo => todo.id === 1, "done", true);

// Batch complex updates with produce (Immer-like API)
import { produce } from "solid-js/store";

setState(produce(s => {
  s.todos.push({ id: 3, text: "Deploy", done: false });
  s.user.name = "Bob";
}));

// Replace store data from server with reconcile (structural diffing)
import { reconcile } from "solid-js/store";

const serverData = await fetchTodos();
setState("todos", reconcile(serverData));
```

**Decision rule for signals vs stores**:
- Single value, flat data → `createSignal`
- Nested object, array of objects, frequently partially updated → `createStore`

## Step 6 — Fetch data with resources

`createResource` is SolidJS's built-in async data primitive:

```tsx
import { createResource, Suspense, ErrorBoundary } from "solid-js";

const fetchUser = async (id: string) => {
  const res = await fetch(`/api/users/${id}`);
  if (!res.ok) throw new Error(`Failed to fetch user ${id}`);
  return res.json();
};

function UserProfile() {
  const [userId] = createSignal("123");
  const [user, { refetch, mutate }] = createResource(userId, fetchUser);

  return (
    <ErrorBoundary fallback={(err) => <p>Error: {err.message}</p>}>
      <Suspense fallback={<Spinner />}>
        <h1>{user()?.name}</h1>
        <button onClick={refetch}>Refresh</button>
      </Suspense>
    </ErrorBoundary>
  );
}
```

Key patterns:
- **First argument** (source signal) triggers refetch when it changes.
- **`mutate`** updates the cached value optimistically without refetching.
- **`refetch`** forces a new fetch.
- Always wrap resource consumers in `<Suspense>` and `<ErrorBoundary>`.

## Step 7 — Set up routing

Use `@solidjs/router`:

```tsx
import { Router, Route, A } from "@solidjs/router";
import { lazy } from "solid-js";

const Home = lazy(() => import("./pages/Home"));
const About = lazy(() => import("./pages/About"));
const UserProfile = lazy(() => import("./pages/UserProfile"));

function App() {
  return (
    <Router>
      <Route path="/" component={Home} />
      <Route path="/about" component={About} />
      <Route path="/users/:id" component={UserProfile} />
    </Router>
  );
}
```

Route data loading:

```tsx
import { useParams } from "@solidjs/router";

function UserProfile() {
  const params = useParams<{ id: string }>();
  const [user] = createResource(() => params.id, fetchUser);
  // params.id is reactive — changing the route refetches automatically
}
```

## Step 8 — Use context for dependency injection

```tsx
import { createContext, useContext } from "solid-js";

interface AuthContextType {
  user: () => User | null;
  login: (creds: Credentials) => Promise<void>;
  logout: () => void;
}

const AuthContext = createContext<AuthContextType>();

export function AuthProvider(props) {
  const [user, setUser] = createSignal<User | null>(null);
  const value: AuthContextType = {
    user,
    login: async (creds) => { /* ... */ },
    logout: () => setUser(null),
  };
  return (
    <AuthContext.Provider value={value}>
      {props.children}
    </AuthContext.Provider>
  );
}

export function useAuth() {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used within AuthProvider");
  return ctx;
}
```

## Step 9 — Performance considerations

SolidJS is fast by default because there are no re-renders. Optimization is mostly about not fighting the framework:

- **`lazy()`** for code splitting — same as React.lazy but for SolidJS components.
- **`startTransition`** for non-urgent updates — keeps the UI responsive during heavy state changes.
- **`batch()`** — groups multiple signal updates into one reactive flush (usually automatic, but useful in async callbacks).
- **`on()`** — explicitly declare which signals an effect depends on (prevents over-tracking).

```tsx
import { on, createEffect } from "solid-js";

// Only reacts to count changes, NOT to name changes even if name() is read
createEffect(on(count, (value) => {
  console.log("Count is now:", value, "Name is:", name());
}));
```

# Decision rules

- **Never destructure props** — this is non-negotiable. Use `mergeProps` for defaults, `splitProps` for separation.
- **Use control flow components, not JS expressions** — `<Show>` over ternaries, `<For>` over `Array.map`. The built-in components maintain reactive tracking.
- **Signals for flat state, stores for nested state** — if you're putting an object in a signal and spreading it to update, switch to `createStore`.
- **Component functions are constructors, not render functions** — any code you want to re-run must be inside a reactive context (effect, memo, JSX expression).
- **Wrap every `createResource` in Suspense + ErrorBoundary** — unhandled async errors and loading states crash the app silently.
- **Use `reconcile()` for server data, `produce()` for local mutations** — don't replace entire store state for partial updates.
- **Prefer `on()` for effects with specific dependencies** — prevents accidental over-tracking when an effect reads signals it shouldn't react to.

# Output requirements

Produce a structured deliverable with these sections:

1. **Component Definition** — component code with proper props handling (no destructuring), typed props interface, `mergeProps`/`splitProps` usage where applicable.
2. **Store Architecture** — store shape definition, update patterns (path-based, `produce`, `reconcile`), which components own vs consume each store.
3. **Route Configuration** — route tree with lazy-loaded components, route data functions, parameter handling, layout nesting.
4. **Resource Pattern** — `createResource` setup with source signals, Suspense/ErrorBoundary wrapping, optimistic updates via `mutate`, refetch triggers.
5. **Reactivity Map** — diagram of signal → memo → effect chains showing what depends on what and which DOM nodes update when each signal changes.

# Anti-patterns

- **Destructuring props**: `function Comp({ name })` breaks reactivity permanently. The value is captured once at component creation and never updates. Always access `props.name`.
- **Using `Array.map` instead of `<For>`**: recreates all DOM nodes on every list change instead of surgically updating only changed items. Causes layout thrashing and lost component state.
- **Creating signals inside effects**: `createEffect(() => { const [x, setX] = createSignal(0); })` creates a new signal on every effect run, leaking memory and causing unpredictable behavior.
- **Treating SolidJS like React**: expecting component functions to re-run, using `useMemo`/`useCallback` equivalents (unnecessary — there are no re-renders to optimize away), reaching for `React.memo` patterns.
- **Storing derived values in signals**: `createEffect(() => setDoubled(count() * 2))` should be `const doubled = createMemo(() => count() * 2)`. Signals are for source state; memos are for derived state.
- **Forgetting that signal getters are functions**: writing `{count}` in JSX instead of `{count()}` renders the function reference, not the value.
- **Over-nesting Suspense boundaries**: one Suspense per resource causes layout shift cascades. Group related resources under one Suspense boundary for cohesive loading states.
- **Mutating store state directly**: `state.user.name = "Bob"` bypasses reactivity. Always use `setState` or `produce`.

# Related skills

- `frontend-webapp-builder` — project scaffolding and framework selection (use when starting a new project)
- `tauri-solidjs` — Tauri desktop app integration with SolidJS
- `fastapi-patterns` — backend API patterns (SolidJS frontend + FastAPI backend is a common pairing)

# Failure handling

- If reactivity is broken (values not updating), check for prop destructuring first — it's the cause >50% of the time.
- If effects run in infinite loops, check for signals being created or set inside effects. Use `on()` to explicitly scope dependencies.
- If converting from React, don't translate line by line. Rewrite from the component's purpose: what signals does it own, what does it derive, what side effects does it perform.
- If store updates aren't reflected in the UI, ensure you're using path-based `setState` or `produce` — not direct mutation of the store object.
- If the team is more comfortable with React patterns, consider whether SolidJS is the right choice for the project — skill misalignment costs more than framework performance gains.
