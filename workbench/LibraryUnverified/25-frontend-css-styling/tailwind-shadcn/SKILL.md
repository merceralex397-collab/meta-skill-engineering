---
name: tailwind-shadcn
description: >-
  Style components with Tailwind CSS, shadcn/ui primitives, cn() utility, and
  CVA variants. Use when composing Tailwind classes with cn(), creating
  component variants with CVA, customizing shadcn/ui components, or setting
  up CSS variable-based theming. Do not use for design token architecture
  (prefer design-tokens) or accessibility patterns (prefer accessibility-audit).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: tailwind-shadcn
  maturity: draft
  risk: low
  tags: [tailwind, shadcn, cva, styling]
---

# Purpose

Style components using Tailwind CSS utilities, shadcn/ui primitives, the `cn()` merge helper, and CVA for variant-driven styling.

# When to use this skill

- composing Tailwind classes with conditional merging via `cn()` (clsx + tailwind-merge)
- creating component variants with `cva` (class-variance-authority)
- customizing or extending shadcn/ui components
- setting up CSS variable-based theming for light/dark modes

# Do not use this skill when

- designing token architecture from scratch — prefer `design-tokens`
- auditing accessibility — prefer `accessibility-audit`
- the task is UX layout decisions — prefer `ux-design`

# Procedure

1. **Set up cn()** — create `lib/utils.ts` with `cn` function combining `clsx` and `tailwind-merge`.
2. **Use Tailwind utilities** — apply classes directly. Use responsive prefixes (`md:`, `lg:`), state (`hover:`, `focus:`), dark mode (`dark:`).
3. **Create variants with CVA** — define `cva()` with base classes, `variants`, and `defaultVariants`. Export type with `VariantProps`.
4. **Customize shadcn/ui** — components are in your codebase (`components/ui/`). Edit directly — they are not in node_modules.
5. **Theme with CSS variables** — define colors in `globals.css` as HSL values. Reference in `tailwind.config.ts` with `hsl(var(--primary))`.
6. **Add dark mode** — use `class` strategy in Tailwind config. Toggle `.dark` class on `<html>`. Override CSS variables in `.dark {}`.
7. **Extract repeated patterns** — if 3+ components share the same Tailwind class string, extract to a CVA variant or utility class.

# cn() utility

```ts
// lib/utils.ts
import { clsx, type ClassValue } from 'clsx';
import { twMerge } from 'tailwind-merge';

export function cn(...inputs: ClassValue[]) {
  return twMerge(clsx(inputs));
}

// Usage — later classes override earlier ones
cn('px-4 py-2 bg-blue-500', isActive && 'bg-blue-700', className)
```

# CVA variants

```tsx
import { cva, type VariantProps } from 'class-variance-authority';
import { cn } from '@/lib/utils';

const buttonVariants = cva(
  'inline-flex items-center justify-center rounded-md text-sm font-medium transition-colors focus-visible:outline-none focus-visible:ring-2',
  {
    variants: {
      variant: {
        default: 'bg-primary text-primary-foreground hover:bg-primary/90',
        destructive: 'bg-destructive text-destructive-foreground hover:bg-destructive/90',
        outline: 'border border-input bg-background hover:bg-accent',
        ghost: 'hover:bg-accent hover:text-accent-foreground',
      },
      size: {
        default: 'h-10 px-4 py-2',
        sm: 'h-9 rounded-md px-3',
        lg: 'h-11 rounded-md px-8',
        icon: 'h-10 w-10',
      },
    },
    defaultVariants: { variant: 'default', size: 'default' },
  }
);

type ButtonProps = React.ComponentPropsWithoutRef<'button'> & VariantProps<typeof buttonVariants>;

function Button({ className, variant, size, ...props }: ButtonProps) {
  return <button className={cn(buttonVariants({ variant, size }), className)} {...props} />;
}
```

# CSS variable theming

```css
/* globals.css */
@layer base {
  :root {
    --background: 0 0% 100%;
    --foreground: 222.2 84% 4.9%;
    --primary: 221.2 83.2% 53.3%;
    --primary-foreground: 210 40% 98%;
  }
  .dark {
    --background: 222.2 84% 4.9%;
    --foreground: 210 40% 98%;
    --primary: 217.2 91.2% 59.8%;
    --primary-foreground: 222.2 47.4% 11.2%;
  }
}
```

# Decision rules

- Always use `cn()` for conditional classes — prevents Tailwind class conflicts (e.g., `px-2` vs `px-4`).
- shadcn/ui components live in YOUR codebase — customize freely, do not treat as third-party.
- Use CSS variables for colors, not Tailwind color names — enables runtime theming.
- CVA for components with 2+ variant dimensions — overkill for single variant.
- Prefer `focus-visible` over `focus` — avoids focus ring on mouse clicks.

# References

- https://tailwindcss.com/docs
- https://ui.shadcn.com/
- https://cva.style/docs

# Related skills

- `design-tokens` — token architecture feeding Tailwind config
- `react-typescript` — typed component props
- `accessibility-audit` — accessible styling patterns
