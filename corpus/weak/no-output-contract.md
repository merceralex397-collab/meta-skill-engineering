---
name: no-output-contract-example
description: >
  Generates environment-specific configuration files from a shared template
  and a per-environment variable overlay, with secret injection from Vault.
---

# Config File Generator

## Purpose

Produce deployment-ready configuration files for each target environment
(development, staging, production) by merging a shared base template with
environment-specific variable overlays and injecting secrets from HashiCorp
Vault at generation time. Eliminates manual config editing and reduces
drift between environments.

## When to use

- Before deploying a service to a new environment for the first time.
- When a config template or overlay has changed and configs need
  regeneration.
- During CI to verify that generated configs are valid before deployment.

## When NOT to use

- For runtime feature-flag changes — use `feature-flag-manager` instead.
- For secrets rotation without config changes — use `vault-rotation` instead.
- When config changes require a migration (e.g., renaming keys) — use
  `config-migration` first, then regenerate.

## Procedure

1. Load the base template from `config/templates/<service>.yaml.tmpl`.
   Templates use Go `text/template` syntax with a strict set of allowed
   functions: `env`, `default`, `required`, `toJSON`, `indent`.

2. Load the environment overlay from
   `config/overlays/<environment>.yaml`. The overlay is a flat key-value
   map that provides variable values for the template.

3. Validate that every `required` variable in the template has a
   corresponding key in the overlay. Collect all missing keys and abort
   with a diagnostic list if any are absent.

4. Authenticate to Vault using the AppRole credentials stored in the
   environment variables `VAULT_ROLE_ID` and `VAULT_SECRET_ID`.

5. For each template variable prefixed with `vault:`, fetch the secret
   from the Vault path specified after the prefix. Example:
   `vault:secret/data/myservice/db_password` fetches the `db_password`
   field from that Vault path.

6. Render the template with the merged variable set (overlay values +
   Vault secrets). Write the output to
   `config/generated/<environment>/<service>.yaml`.

7. Validate the rendered config against the JSON Schema located at
   `config/schemas/<service>.schema.json`. Report any validation errors
   with field paths and expected types.

8. Run a diff against the previously generated config (if it exists) and
   write a human-readable change summary to stdout. Highlight any changes
   to secret-bearing fields without revealing the secret values.

## Failure handling

- If Vault is unreachable, retry 3 times with 5-second backoff. If still
  unreachable, abort and print which secrets could not be fetched.
- If the overlay contains keys not referenced by the template, log a
  warning for each unused key but do not abort.
- If schema validation fails, do not write the output file. Print the
  validation errors and exit with code 1.

## Next steps

- Deploy generated configs using `deployment-orchestrator`.
- Run `skill-evaluation` to verify config coverage across all declared
  environments.
- Store generation metadata via `skill-provenance` for audit trail.
