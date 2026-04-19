---
name: react-typescript
description: >-
  Write React components with TypeScript using discriminated unions, generic
  props, and forwardRef. Use when typing component props, creating generic
  reusable components, forwarding refs with proper types, or fixing TS errors
  in React code. Do not use for Next.js-specific patterns (prefer
  nextjs-app-router) or state management architecture (prefer state-management).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: react-typescript
  maturity: draft
  risk: low
  tags: [react, typescript, components]
---

# Purpose

Write type-safe React components using TypeScript: discriminated union props, generic components, forwardRef, and proper event typing.

# When to use this skill

- typing component props with discriminated unions for variant patterns
- creating generic reusable components (`<List<T>>`, `<Select<T>>`)
- forwarding refs with `forwardRef` and proper generic typing
- fixing TypeScript errors in React components or hooks

# Do not use this skill when

- working with Next.js App Router specifics — prefer `nextjs-app-router`
- designing state architecture — prefer `state-management`
- building forms — prefer `forms-validation`

# Procedure

1. **Define props as types** — use `type` (not `interface`) for component props. Export for reuse.
2. **Use discriminated unions** — for variant props: `type Props = { variant: 'primary'; icon: ReactNode } | { variant: 'ghost' }`.
3. **Make generic components** — `function List<T>({ items, renderItem }: { items: T[]; renderItem: (item: T) => ReactNode })`.
4. **Forward refs correctly** — use `forwardRef<HTMLDivElement, Props>()`. Add `displayName` for DevTools.
5. **Type events** — `onClick: (e: React.MouseEvent<HTMLButtonElement>) => void`. Use `React.ChangeEvent<HTMLInputElement>` for inputs.
6. **Type hooks** — `useState<User | null>(null)`, `useRef<HTMLDivElement>(null)`. Avoid `any`.
7. **Use `ComponentPropsWithoutRef`** — extend native element props: `type Props = ComponentPropsWithoutRef<'button'> & { variant: string }`.
8. **Extract shared types** — put reusable types in `types.ts`. Co-locate component-specific types with the component.

# Discriminated unions

```tsx
type ButtonProps =
  | { variant: 'primary'; loading?: boolean; onClick: () => void }
  | { variant: 'link'; href: string }
  | { variant: 'icon'; icon: ReactNode; 'aria-label': string; onClick: () => void };

function Button(props: ButtonProps) {
  switch (props.variant) {
    case 'primary':
      return <button onClick={props.onClick} disabled={props.loading}>Submit</button>;
    case 'link':
      return <a href={props.href}>Link</a>;
    case 'icon':
      return <button onClick={props.onClick} aria-label={props['aria-label']}>{props.icon}</button>;
  }
}
```

# Generic components

```tsx
type ListProps<T> = {
  items: T[];
  renderItem: (item: T) => ReactNode;
  keyExtractor: (item: T) => string;
};

function List<T>({ items, renderItem, keyExtractor }: ListProps<T>) {
  return <ul>{items.map(item => <li key={keyExtractor(item)}>{renderItem(item)}</li>)}</ul>;
}

// Usage — T is inferred
<List items={users} renderItem={u => <span>{u.name}</span>} keyExtractor={u => u.id} />
```

# forwardRef pattern

```tsx
type InputProps = ComponentPropsWithoutRef<'input'> & { label: string };

const Input = forwardRef<HTMLInputElement, InputProps>(({ label, ...props }, ref) => (
  <div>
    <label>{label}</label>
    <input ref={ref} {...props} />
  </div>
));
Input.displayName = 'Input';
```

# Decision rules

- `type` over `interface` for props — unions require `type`; consistency matters.
- Never use `React.FC` — it adds implicit `children` and breaks generics.
- Discriminated unions over optional booleans — `variant: 'loading'` not `isLoading?: boolean`.
- `ComponentPropsWithoutRef` to extend native props — avoids ref conflicts.
- Keep `children` explicit — `{ children: ReactNode }` not implicit.

# References

- https://react.dev/learn/typescript
- https://www.typescriptlang.org/docs/handbook/2/narrowing.html

# Related skills

- `nextjs-app-router` — Next.js-specific patterns
- `state-management` — typed state management
- `forms-validation` — typed form patterns
