---
name: testing-web
description: >-
  Test React apps with Testing Library, MSW for API mocking, and Vitest. Use
  when writing component tests with Testing Library, mocking API calls with
  MSW, setting up Vitest for a React project, or testing hooks and async
  behavior. Do not use for E2E browser tests (prefer Playwright skills) or
  backend API tests.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: testing-web
  maturity: draft
  risk: low
  tags: [testing, vitest, testing-library, msw]
---

# Purpose

Test React applications using Testing Library for component tests, MSW for API mocking, and Vitest as the test runner.

# When to use this skill

- writing component tests with `@testing-library/react`
- mocking API responses with MSW (Mock Service Worker)
- setting up Vitest for a React/Vite project
- testing hooks, async behavior, and user interactions

# Do not use this skill when

- writing E2E browser tests — prefer Playwright skills
- testing backend APIs — different testing patterns
- the task is accessibility auditing — prefer `accessibility-audit`

# Procedure

1. **Set up Vitest** — `npm install -D vitest @testing-library/react @testing-library/jest-dom @testing-library/user-event jsdom`.
2. **Configure** — in `vitest.config.ts`: `environment: 'jsdom'`, `setupFiles: ['./src/test/setup.ts']`.
3. **Write component test** — render with `render(<Component />)`, query with `screen.getByRole()`, assert with `expect().toBeInTheDocument()`.
4. **Simulate interactions** — use `userEvent.click()`, `userEvent.type()`. Prefer `userEvent` over `fireEvent`.
5. **Set up MSW** — define handlers with `http.get('/api/data', () => HttpResponse.json(...))`. Start server in `beforeAll`.
6. **Test async** — use `await screen.findByText()` for elements that appear after async operations. Use `waitFor` for assertions.
7. **Test hooks** — use `renderHook()` from `@testing-library/react` for custom hooks.
8. **Run** — `npx vitest` (watch mode) or `npx vitest run` (CI mode).

# Component test example

```tsx
import { render, screen } from '@testing-library/react';
import userEvent from '@testing-library/user-event';
import { expect, test } from 'vitest';
import { Counter } from './Counter';

test('increments count on click', async () => {
  const user = userEvent.setup();
  render(<Counter />);

  expect(screen.getByText('Count: 0')).toBeInTheDocument();
  await user.click(screen.getByRole('button', { name: /increment/i }));
  expect(screen.getByText('Count: 1')).toBeInTheDocument();
});
```

# MSW API mocking

```tsx
import { http, HttpResponse } from 'msw';
import { setupServer } from 'msw/node';

const server = setupServer(
  http.get('/api/users', () => HttpResponse.json([
    { id: '1', name: 'Alice' },
    { id: '2', name: 'Bob' },
  ])),
);

beforeAll(() => server.listen());
afterEach(() => server.resetHandlers());
afterAll(() => server.close());

test('displays users from API', async () => {
  render(<UserList />);
  expect(await screen.findByText('Alice')).toBeInTheDocument();
  expect(screen.getByText('Bob')).toBeInTheDocument();
});

test('handles API error', async () => {
  server.use(http.get('/api/users', () => HttpResponse.error()));
  render(<UserList />);
  expect(await screen.findByText(/error/i)).toBeInTheDocument();
});
```

# Decision rules

- Query by role first (`getByRole`), then by label, then by text — matches how users interact.
- Never query by test ID unless no semantic alternative exists.
- Use `userEvent` over `fireEvent` — it simulates full interaction (focus, keydown, keyup, click).
- Mock at the network level (MSW) not at the module level — tests real fetch/axios code.
- `findBy*` for async, `getBy*` for sync, `queryBy*` to assert absence.

# References

- https://testing-library.com/docs/react-testing-library/intro
- https://mswjs.io/docs
- https://vitest.dev/

# Related skills

- `react-typescript` — typed component patterns being tested
- `forms-validation` — testing form validation
- `state-management` — testing state interactions
