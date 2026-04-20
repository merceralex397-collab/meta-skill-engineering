---
name: accessibility-audit
description: >-
  Audit and fix web accessibility against WCAG 2.2 AA criteria. Use when
  auditing pages for WCAG compliance, fixing axe-core or Lighthouse
  accessibility violations, adding ARIA attributes, or testing keyboard
  navigation and screen reader support. Do not use for visual performance
  (prefer frontend-performance) or form logic without a11y focus (prefer
  forms-validation).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: accessibility-audit
  maturity: draft
  risk: low
  tags: [accessibility, audit, wcag]
---

# Purpose

Audit and fix web accessibility against WCAG 2.2 success criteria using axe-core, Lighthouse, keyboard testing, and manual screen reader verification.

# When to use this skill

- auditing a page or component for WCAG 2.2 AA compliance
- fixing accessibility violations found by axe-core or Lighthouse
- building new components that must meet accessibility standards
- adding ARIA patterns to custom interactive widgets

# Do not use this skill when

- the task is visual performance — prefer `frontend-performance`
- the task is form logic without a11y focus — prefer `forms-validation`
- the task is state management — prefer `state-management`

# Procedure

1. **Run axe-core scan** — install `@axe-core/cli` or use browser extension. Run `npx axe <url>` to get violations list.
2. **Run Lighthouse audit** — `npx lighthouse <url> --only-categories=accessibility --output=json`. Target score >= 95.
3. **Check heading hierarchy** — verify `h1` > `h2` > `h3` — no skipped levels. One `h1` per page.
4. **Test keyboard navigation** — tab through all interactive elements. Verify: logical focus order, visible focus indicator, no keyboard traps.
5. **Verify ARIA usage** — check roles, states, properties follow WAI-ARIA Authoring Practices. Prefer semantic HTML over ARIA.
6. **Check color contrast** — verify ratios: 4.5:1 for normal text, 3:1 for large text (AA). Use DevTools contrast checker.
7. **Test with screen reader** — verify content announced correctly, images have `alt`, dynamic updates use `aria-live`.
8. **Add skip link** — `<a href="#main" class="sr-only focus:not-sr-only">Skip to content</a>` before navigation.

# WCAG 2.2 AA key criteria

| SC | Name | Requirement |
|----|------|-------------|
| 1.1.1 | Non-text Content | All images have `alt` (decorative: `alt=""`) |
| 1.3.1 | Info and Relationships | Structure via semantic HTML, not just styling |
| 2.1.1 | Keyboard | All functionality via keyboard |
| 2.1.2 | No Keyboard Trap | Focus can always move away |
| 2.4.1 | Bypass Blocks | Skip-to-content link |
| 2.4.7 | Focus Visible | Visible focus indicator on all interactive elements |
| 4.1.2 | Name, Role, Value | All UI components have accessible names |

# ARIA patterns for custom widgets

```html
<!-- Disclosure (expand/collapse) -->
<button aria-expanded="false" aria-controls="panel1">Details</button>
<div id="panel1" hidden>Content</div>

<!-- Tab panel -->
<div role="tablist">
  <button role="tab" aria-selected="true" aria-controls="tab1">Tab 1</button>
  <button role="tab" aria-selected="false" aria-controls="tab2">Tab 2</button>
</div>
<div role="tabpanel" id="tab1">Content 1</div>

<!-- Live region for dynamic updates -->
<div aria-live="polite" aria-atomic="true">Status: Loading...</div>
```

# Decision rules

- Prefer semantic HTML (`<button>`, `<nav>`, `<main>`) over ARIA roles — native elements have built-in behavior.
- Never use `tabindex > 0` — it breaks natural tab order. Use `0` or `-1` only.
- `aria-label` for icon-only buttons: `<button aria-label="Close"><svg>...</svg></button>`.
- Focus management on route changes in SPAs — move focus to main content or heading.
- Test with real screen readers (VoiceOver, NVDA) — automated tools catch only ~30% of issues.

# References

- https://www.w3.org/WAI/WCAG22/quickref/
- https://www.w3.org/WAI/ARIA/apg/
- https://github.com/dequelabs/axe-core

# Related skills

- `forms-validation` — accessible form patterns
- `ux-design` — visual hierarchy and usability
- `testing-web` — accessibility testing automation
