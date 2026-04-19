# Assumption Taxonomy

Use these buckets to keep the audit concrete.

## User or product behavior

Examples:

- users will understand the new flow without onboarding
- admins will perform cleanup manually

## Dependency or integration behavior

Examples:

- the vendor API returns stable payloads
- the auth provider supports the needed token claims

## Operational environment

Examples:

- deploy windows are available
- staging matches production closely enough

## Data quality or measurement

Examples:

- identifiers are unique
- source data is complete enough to migrate

## Resourcing or ownership

Examples:

- another team will review the change this week
- someone owns the dashboard or alert after launch

## Policy, legal, or security approval

Examples:

- the logging change is allowed to store new fields
- the integration can be approved without procurement
