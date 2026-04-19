---
name: analytics-instrumentation
description: >-
  Instrument product analytics with GA4, Segment, or PostHog using consistent
  event naming. Use when adding event tracking, defining naming conventions,
  instrumenting funnels, or creating analytics wrapper hooks in React. Do not
  use for E2E testing (prefer testing-web) or SEO metadata (prefer
  seo-structured-data).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: analytics-instrumentation
  maturity: draft
  risk: low
  tags: [analytics, instrumentation, ga4, segment]
---

# Purpose

Instrument product analytics with consistent event naming, type-safe wrappers, and funnel tracking using GA4, Segment, or PostHog.

# When to use this skill

- adding analytics tracking to a web app (page views, custom events, conversions)
- defining event naming conventions or a tracking plan
- instrumenting funnels or feature usage metrics
- wrapping analytics calls in React hooks for reuse

# Do not use this skill when

- the task is E2E testing — prefer `testing-web`
- the task is SEO metadata — prefer `seo-structured-data`
- the task is backend logging/monitoring — different patterns

# Procedure

1. **Identify provider** — check for GA4 (`gtag.js`), Segment (`analytics.js`), PostHog, or custom data layer.
2. **Define naming convention** — use `object_action` pattern in `snake_case`: `signup_completed`, `item_added_to_cart`.
3. **Create analytics wrapper** — thin abstraction over provider for portability. Type the event names and properties.
4. **Implement tracking** — add calls at user action points. Keep tracking code separate from business logic.
5. **Verify events** — use GA4 DebugView, Segment Debugger, or Network tab to confirm events fire with correct properties.
6. **Document tracking plan** — maintain a living doc mapping events to user actions and properties.

# Event naming rules

```
Pattern: <noun>_<past_tense_verb>
  signup_completed, product_viewed, item_added_to_cart, search_performed

Bad: click, pageView123, btnSubmit, homepage

Rules:
- snake_case (GA4) or Title Case (Segment) — be consistent
- Context in properties, not name — item_added_to_cart + {category: "shoes"}
- No PII in event properties — no emails, no passwords
```

# React analytics hook

```tsx
import { useCallback } from 'react';

function trackEvent(name: string, props?: Record<string, unknown>) {
  window.gtag?.('event', name, props);
  window.analytics?.track(name, props);
}

export function useTrackEvent() {
  return useCallback((name: string, props?: Record<string, unknown>) => {
    trackEvent(name, props);
  }, []);
}

// Usage
function SignupButton() {
  const track = useTrackEvent();
  return (
    <button onClick={() => { track('signup_button_clicked', { location: 'hero' }); }}>
      Sign Up
    </button>
  );
}
```

# Funnel tracking

```
Signup Funnel:
  1. signup_page_viewed
  2. signup_form_started (first field interaction)
  3. signup_form_submitted
  4. email_verified
  5. onboarding_completed

Each step includes: funnel_name, step_number, timestamp (auto by provider)
```

# Decision rules

- Track user actions, not implementation events — "signup_completed" not "POST /api/signup succeeded".
- Keep properties flat — avoid nested objects; most analytics tools cannot handle them.
- Separate tracking from business logic — use hooks, middleware, or event listeners.
- Instrument progressively — start with critical funnel events, add engagement events later.
- Verify before shipping — use debug tools to confirm events fire with correct properties.

# References

- https://developers.google.com/analytics/devguides/collection/ga4
- https://segment.com/docs/connections/sources/catalog/libraries/website/javascript/

# Related skills

- `seo-structured-data` — metadata alongside analytics
- `react-typescript` — typed analytics hooks
- `frontend-performance` — analytics impact on performance
