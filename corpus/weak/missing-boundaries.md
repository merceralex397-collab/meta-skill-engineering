---
name: missing-boundaries-example
description: >
  Converts OpenAPI 3.x specifications into typed client SDKs for TypeScript
  and Python, including request/response models and authentication helpers.
---

# OpenAPI SDK Generator

## Purpose

Automate the generation of typed HTTP client libraries from OpenAPI 3.x
specifications. The generated SDKs include request/response model types,
endpoint methods, authentication configuration, and retry logic so that
consuming services do not hand-write HTTP calls.

## When to use

- A team has a finalized OpenAPI 3.x spec and needs a TypeScript or Python
  client SDK.
- An existing SDK has drifted from the spec and needs regeneration.
- During CI, to verify that the generated SDK compiles cleanly against the
  current spec version.

## Procedure

1. Parse the OpenAPI spec with `@readme/openapi-parser` and resolve all
   `$ref` pointers into a fully dereferenced document.
2. Validate the resolved spec against the OpenAPI 3.x JSON Schema; abort
   with a diagnostic list if validation fails.
3. Extract every path/operation pair and build an intermediate representation
   (IR) containing method name, URL template, parameters, request body
   schema, and response schemas.
4. For each language target, load the corresponding Handlebars template set
   from `templates/{lang}/`.
5. Render model files: one file per schema component, using the language's
   naming conventions (PascalCase for TS interfaces, snake_case for Python
   dataclasses).
6. Render client files: one method per operation, grouped by the first tag.
   Include JSDoc or docstring with the operation summary.
7. Generate an authentication module that reads credentials from environment
   variables and attaches them per the spec's `securitySchemes`.
8. Write all output to `generated/{lang}/` and run the language's formatter
   (`prettier` for TS, `black` for Python).
9. Compile / type-check the output (`tsc --noEmit` for TS, `mypy --strict`
   for Python) and report any errors.

## Output contract

- `generated/typescript/` — Fully typed TS client, compiles with
  `tsc --strict --noEmit`.
- `generated/python/` — Python 3.11+ package, passes `mypy --strict`.
- `generation-report.md` — Summary listing endpoint count, model count,
  warnings, and any skipped operations.

## Failure handling

- If the spec fails validation, emit the full error list and stop. Do not
  generate partial output from an invalid spec.
- If a schema uses `anyOf`/`oneOf` in a way the templates cannot represent,
  log a warning and generate a union type with a TODO comment.
- If the formatter or type-checker fails, preserve the raw output and
  include the error log in `generation-report.md`.

## Next steps

- Run generated SDKs through `skill-testing-harness` to verify runtime
  behavior against a mock server.
- Feed `generation-report.md` into `skill-evaluation` to check for
  coverage gaps.
