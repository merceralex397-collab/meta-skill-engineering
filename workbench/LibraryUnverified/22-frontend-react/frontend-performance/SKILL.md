---
name: frontend-performance
description: >-
  Optimize Core Web Vitals (LCP, CLS, INP) and Lighthouse scores. Use when
  diagnosing slow LCP, layout shifts, or interaction delays, optimizing
  bundle size with code splitting, implementing image lazy loading, or
  improving Lighthouse performance score. Do not use for backend API
  performance or accessibility audits (prefer accessibility-audit).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: frontend-performance
  maturity: draft
  risk: low
  tags: [performance, web-vitals, lighthouse]
---

# Purpose

Optimize Core Web Vitals (LCP, CLS, INP) and Lighthouse performance score through bundle optimization, image handling, and rendering strategies.

# When to use this skill

- diagnosing slow LCP, layout shifts (CLS), or interaction delays (INP)
- reducing bundle size with code splitting and tree shaking
- implementing image optimization and lazy loading
- improving Lighthouse performance score

# Do not use this skill when

- the task is backend/API performance — different domain
- the task is accessibility — prefer `accessibility-audit`
- the task is SEO metadata — prefer `seo-structured-data`

# Procedure

1. **Measure baseline** — run Lighthouse in incognito: `npx lighthouse <url> --output=json`. Note LCP, CLS, INP, TBT.
2. **Analyze bundle** — `npx webpack-bundle-analyzer` or `npx vite-bundle-visualizer`. Find largest chunks.
3. **Code split routes** — `React.lazy()` + `Suspense` for route-level splitting. Dynamic `import()` for heavy features.
4. **Optimize images** — use `<img>` with `srcset`, `sizes`, and `loading="lazy"`. Serve WebP/AVIF. Set explicit `width`/`height` to prevent CLS.
5. **Reduce CLS** — reserve space for dynamic content (ads, images, embeds) with `aspect-ratio` or explicit dimensions.
6. **Improve INP** — break long tasks with `requestIdleCallback` or `scheduler.yield()`. Debounce input handlers.
7. **Optimize fonts** — use `font-display: swap`, preload critical fonts: `<link rel="preload" as="font" crossorigin>`.
8. **Verify** — re-run Lighthouse. Test on throttled 3G in DevTools. Check field data in CrUX/PageSpeed Insights.

# Core Web Vitals targets

| Metric | Good | Needs Work | Poor |
|--------|------|------------|------|
| LCP | < 2.5s | 2.5-4s | > 4s |
| CLS | < 0.1 | 0.1-0.25 | > 0.25 |
| INP | < 200ms | 200-500ms | > 500ms |

# Code splitting pattern

```tsx
import { lazy, Suspense } from 'react';

const Dashboard = lazy(() => import('./pages/Dashboard'));
const Settings = lazy(() => import('./pages/Settings'));

function App() {
  return (
    <Suspense fallback={<Spinner />}>
      <Routes>
        <Route path="/" element={<Dashboard />} />
        <Route path="/settings" element={<Settings />} />
      </Routes>
    </Suspense>
  );
}
```

# Image optimization

```html
<img
  src="/img/hero.webp"
  srcset="/img/hero-400.webp 400w, /img/hero-800.webp 800w, /img/hero-1200.webp 1200w"
  sizes="(max-width: 768px) 100vw, 50vw"
  width="1200" height="600"
  loading="lazy"
  decoding="async"
  alt="Hero image"
/>
```

# Decision rules

- Measure before optimizing — Lighthouse + field data (CrUX), not guesses.
- LCP is usually the hero image or largest text block — optimize that first.
- Set explicit dimensions on all images and embeds — prevents CLS.
- Preload only critical resources — over-preloading delays everything.
- Third-party scripts (analytics, chat widgets) are the top CLS/TBT offender — load them after interaction or on `requestIdleCallback`.

# References

- https://web.dev/articles/vitals
- https://developer.chrome.com/docs/lighthouse/

# Related skills

- `accessibility-audit` — a11y alongside performance
- `seo-structured-data` — performance affects SEO
- `analytics-instrumentation` — analytics script impact
