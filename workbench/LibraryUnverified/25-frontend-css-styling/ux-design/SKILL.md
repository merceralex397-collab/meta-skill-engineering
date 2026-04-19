---
name: ux-design
description: >-
  Apply UX design principles — Fitts's Law, Hick's Law, visual hierarchy, and
  interaction patterns. Use when evaluating UI layouts for usability, improving
  click target sizing, reducing choice overload, or establishing visual
  hierarchy with typography and spacing. Do not use for implementation with
  Tailwind (prefer tailwind-shadcn) or accessibility compliance (prefer
  accessibility-audit).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: ux-design
  maturity: draft
  risk: low
  tags: [ux, design, usability, hierarchy]
---

# Purpose

Apply UX design principles — Fitts's Law, Hick's Law, visual hierarchy, and interaction patterns — to create usable interfaces.

# When to use this skill

- evaluating a UI layout for usability issues
- improving click/touch target sizing and spacing
- reducing cognitive load by simplifying choices and information density
- establishing visual hierarchy with typography, color, and whitespace

# Do not use this skill when

- implementing styles with Tailwind/CSS — prefer `tailwind-shadcn`
- testing accessibility compliance — prefer `accessibility-audit`
- building design token systems — prefer `design-tokens`

# Procedure

1. **Audit visual hierarchy** — check that the most important element is visually dominant (size, color, position). Users should see it within 2 seconds.
2. **Apply Fitts's Law** — interactive targets minimum 44x44px (touch) or 24x24px (mouse). Increase size for primary actions.
3. **Apply Hick's Law** — reduce choices per screen. Group related options. Progressive disclosure for complex features.
4. **Check reading flow** — F-pattern for content pages, Z-pattern for landing pages. Left-align text. Limit line length to 60-80 characters.
5. **Validate whitespace** — increase spacing between groups, decrease within groups (proximity principle). Never crowd elements.
6. **Test interaction patterns** — primary action is visually distinct (filled button), secondary is subdued (outline/ghost). Destructive actions require confirmation.
7. **Review mobile** — thumb-zone placement for primary actions (bottom of screen). Stack layouts vertically. Larger touch targets.

# Core principles

| Principle | Rule | Application |
|-----------|------|-------------|
| Fitts's Law | Larger + closer = easier to click | Big primary buttons, avoid tiny icons for key actions |
| Hick's Law | More choices = longer decision time | Max 5-7 items per group, use progressive disclosure |
| Proximity | Related items close together | Group form fields, separate sections with whitespace |
| Contrast | Important elements stand out | Primary action vs secondary, error states vs normal |
| Consistency | Same action = same appearance | All delete buttons are red, all links are underlined |

# Visual hierarchy checklist

```
1. Size — most important element is largest
2. Color — primary action uses brand color; secondary is muted
3. Weight — headings are bold; body text is regular
4. Position — key content above the fold; CTAs in expected locations
5. Whitespace — breathing room around important elements
6. Typography scale — clear h1 > h2 > h3 > body progression
```

# Interaction patterns

```
Primary action:    filled button, brand color, prominent placement
Secondary action:  outline or ghost button, less prominent
Destructive:       red/destructive variant, requires confirmation dialog
Disabled:          reduced opacity, no hover state, aria-disabled
Loading:           spinner or skeleton, disable submission, show progress
Empty state:       illustration + explanation + single CTA to get started
```

# Decision rules

- One primary action per screen — if everything is bold, nothing is.
- 44px minimum touch target on mobile — Apple and Google both recommend this.
- Progressive disclosure — show basic options first, reveal advanced behind "Show more" or accordion.
- Error messages next to the field, not in a banner — reduces user effort to fix.
- Test with real users when possible — 5 users find 80% of usability issues.

# References

- https://lawsofux.com/
- https://www.nngroup.com/articles/ten-usability-heuristics/

# Related skills

- `tailwind-shadcn` — implementing UX decisions in code
- `accessibility-audit` — usability for all users
- `design-tokens` — consistent spacing and typography scales
