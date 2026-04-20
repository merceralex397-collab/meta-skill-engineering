---
name: design-tokens
description: >-
  Define design tokens in W3C DTCG format and generate platform outputs with
  Style Dictionary. Use when creating a token system (colors, spacing, type),
  setting up Style Dictionary transforms, bridging Figma variables to code,
  or implementing dark mode token layers. Do not use for Tailwind utility
  classes (prefer tailwind-shadcn) or brand identity (prefer brand-guidelines).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: design-tokens
  maturity: draft
  risk: low
  tags: [design, tokens, style-dictionary]
---

# Purpose

Define design tokens using W3C DTCG format and generate CSS custom properties, Tailwind config, and platform-specific outputs via Style Dictionary.

# When to use this skill

- creating or reorganizing a token system (colors, spacing, typography, shadows)
- setting up Style Dictionary or similar token transformation pipeline
- bridging Figma variables to code via token files
- implementing dark mode as a semantic token layer

# Do not use this skill when

- working with Tailwind utilities or shadcn/ui — prefer `tailwind-shadcn`
- deciding UX layout — prefer `ux-design`
- defining brand identity — prefer `brand-guidelines`

# Procedure

1. **Audit existing tokens** — check `globals.css`, `tailwind.config`, theme files, or `tokens/` for existing design values.
2. **Structure 3 layers** — primitive (raw values) > semantic (meaning-based aliases) > component (consumed by UI).
3. **Define in DTCG format** — JSON with `$value`, `$type`, `$description` fields. Use `{ref.path}` for aliases.
4. **Set up Style Dictionary** — `npm install style-dictionary`, configure `source` and `platforms` in config.
5. **Generate outputs** — `npx style-dictionary build`. Verify CSS custom properties, Tailwind tokens, iOS/Android values.
6. **Add dark mode** — define `semantic-dark` layer with inverted mappings. Output as `.dark { }` CSS overrides.
7. **Validate** — no orphan tokens, dark mode complete, foreground/background pairs pass WCAG AA contrast.

# Token architecture

```
Component:  button-bg, input-border, card-shadow
     |
Semantic:   color-primary, color-surface, spacing-md
     |
Primitive:  blue-500, gray-100, 16px, 0.5rem
```

**Components reference semantic tokens, never primitives.**

# DTCG format

```json
{
  "color": {
    "primitive": {
      "blue-500": { "$value": "#3b82f6", "$type": "color" },
      "gray-50":  { "$value": "#f9fafb", "$type": "color" },
      "gray-900": { "$value": "#111827", "$type": "color" }
    },
    "semantic": {
      "primary":    { "$value": "{color.primitive.blue-500}", "$type": "color" },
      "background": { "$value": "{color.primitive.gray-50}", "$type": "color" },
      "foreground": { "$value": "{color.primitive.gray-900}", "$type": "color" }
    }
  },
  "spacing": {
    "sm": { "$value": "0.5rem", "$type": "dimension" },
    "md": { "$value": "1rem",   "$type": "dimension" },
    "lg": { "$value": "1.5rem", "$type": "dimension" }
  }
}
```

# Dark mode strategy

```css
:root {
  --color-background: #f9fafb;
  --color-foreground: #111827;
}
.dark {
  --color-background: #111827;
  --color-foreground: #f9fafb;
}
```

# Decision rules

- Single source of truth — tokens live in JSON. CSS/Tailwind values are generated, never hand-maintained.
- Semantic over primitive — components use `--color-primary`, not `--blue-500`.
- Complete dark mode — every semantic token has a dark counterpart.
- Contrast validation — foreground/background pairs meet WCAG AA (4.5:1).
- No magic numbers — if a value appears 2+ times, make it a token.

# References

- https://tr.designtokens.org/format/
- https://amzn.github.io/style-dictionary/

# Related skills

- `tailwind-shadcn` — consuming tokens in Tailwind projects
- `accessibility-audit` — contrast ratio validation
- `ux-design` — visual hierarchy using token scales
