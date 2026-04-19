# Criterion Patterns

## UI Flow

```text
Given <starting state>, when <user action>, the interface <observable behavior>.
```

## API Endpoint

```text
Given <request context>, when the client sends <input>, the API returns <status + payload shape> and records <side effect if any>.
```

## Background Job

```text
When <trigger>, the job processes <scope>, emits <observable output>, and surfaces failures through <log/metric/alert>.
```

## Migration

```text
Before enabling <consumer>, the migration creates or transforms <state>. Verification is proven by <query/check>. Rollback or containment is <method>.
```

## Operator Workflow

```text
Given <incident/admin condition>, the operator can <action> and confirm success via <receipt>.
```
