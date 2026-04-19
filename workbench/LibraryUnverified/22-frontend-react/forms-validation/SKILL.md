---
name: forms-validation
description: >-
  Build forms with React Hook Form + Zod schema validation, accessible error
  display, and submission handling. Use when building or fixing forms with
  client-side validation, integrating Zod schemas with React Hook Form,
  handling async submission errors, or making forms keyboard and screen-reader
  accessible. Do not use for backend validation logic or state management
  without forms (prefer state-management).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: forms-validation
  maturity: draft
  risk: low
  tags: [forms, validation, react-hook-form, zod]
---

# Purpose

Build accessible, validated forms using React Hook Form with Zod schema validation, proper error display, and submission handling.

# When to use this skill

- building forms with client-side validation in React
- integrating Zod schemas with React Hook Form via `@hookform/resolvers`
- handling async submission with loading states and server errors
- making forms accessible (error announcements, label associations)

# Do not use this skill when

- writing backend validation — different patterns apply
- managing global state without forms — prefer `state-management`
- the task is visual styling — prefer `tailwind-shadcn`

# Procedure

1. **Define Zod schema** — create schema with `z.object()`, add constraints: `z.string().min(1).email()`, `z.number().positive()`.
2. **Set up React Hook Form** — `useForm({ resolver: zodResolver(schema), defaultValues })`.
3. **Build form fields** — use `register()` for uncontrolled or `Controller` for controlled components. Always set `id` and `htmlFor`.
4. **Display errors** — show `formState.errors[field]?.message` next to fields. Use `aria-describedby` linking error to input.
5. **Handle submission** — pass async handler to `handleSubmit()`. Set loading state, catch server errors, display in form.
6. **Add accessibility** — associate `<label htmlFor>` with inputs, announce errors with `aria-live="polite"`, set `aria-invalid` on errored fields.
7. **Test** — verify validation triggers on blur/submit, errors display and clear correctly, submission works with valid data.

# Zod + React Hook Form

```tsx
import { z } from 'zod';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';

const schema = z.object({
  email: z.string().min(1, 'Required').email('Invalid email'),
  password: z.string().min(8, 'At least 8 characters'),
  age: z.coerce.number().min(18, 'Must be 18+').optional(),
});

type FormData = z.infer<typeof schema>;

function SignupForm() {
  const { register, handleSubmit, formState: { errors, isSubmitting } } = useForm<FormData>({
    resolver: zodResolver(schema),
  });

  const onSubmit = async (data: FormData) => {
    await api.signup(data);
  };

  return (
    <form onSubmit={handleSubmit(onSubmit)} noValidate>
      <div>
        <label htmlFor="email">Email</label>
        <input id="email" type="email" aria-invalid={!!errors.email}
          aria-describedby={errors.email ? 'email-err' : undefined}
          {...register('email')} />
        {errors.email && <p id="email-err" role="alert">{errors.email.message}</p>}
      </div>
      <div>
        <label htmlFor="password">Password</label>
        <input id="password" type="password" aria-invalid={!!errors.password}
          aria-describedby={errors.password ? 'pw-err' : undefined}
          {...register('password')} />
        {errors.password && <p id="pw-err" role="alert">{errors.password.message}</p>}
      </div>
      <button type="submit" disabled={isSubmitting}>
        {isSubmitting ? 'Submitting...' : 'Sign Up'}
      </button>
    </form>
  );
}
```

# Decision rules

- Use `noValidate` on `<form>` — let Zod handle validation, not the browser.
- Validate on blur for individual fields, on submit for the whole form — `mode: 'onBlur'`.
- Use `z.coerce.number()` for numeric inputs — HTML inputs always return strings.
- Share Zod schemas between client and server for consistent validation.
- Always associate errors with inputs via `aria-describedby` and `aria-invalid`.

# References

- https://react-hook-form.com/
- https://zod.dev/
- https://github.com/react-hook-form/resolvers

# Related skills

- `react-typescript` — typed component patterns
- `accessibility-audit` — form accessibility testing
- `state-management` — form state vs app state boundaries
