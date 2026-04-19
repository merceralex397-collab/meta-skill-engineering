---
name: error-handling
description: Designs exception hierarchies, user-facing error formats, PII redaction, and structured logging conventions. Trigger on 'error handling strategy', 'exception hierarchy', 'structured logging', 'API error format', 'error boundary'. DO NOT USE for security-specific input validation (use security-hardening), post-incident analysis (use incident-postmortem), or observability dashboards (use workflow-observability).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: error-handling
  maturity: draft
  risk: low
  tags: [error, handling]
---

# Purpose
Design error handling strategy that distinguishes recoverable from unrecoverable errors, provides appropriate information to users vs. operators, and enables effective debugging without leaking sensitive data. Rust's `Result<T, E>` and Python's exception hierarchy offer the mental models.

# When to use this skill
Use when:
- Designing error handling for a new service or module
- Current error handling is inconsistent or leaks stack traces to users
- Adding structured logging/observability to existing code
- Defining API error response formats

Do NOT use when:
- Writing throwaway scripts (just let them crash)
- Error handling already well-established and documented in codebase

# Operating procedure
1. **Classify error types**:
   ```
   Recoverable (Result/Expected)     Unrecoverable (Panic/Exception)
   ─────────────────────────────     ────────────────────────────────
   - User input validation errors    - Out of memory
   - Resource not found              - Invariant violations (bugs)
   - Rate limit exceeded             - Corrupted state
   - Network timeout (retry)         - Missing critical config at startup
   ```

2. **Define error type hierarchy** (example in Python):
   ```python
   class AppError(Exception):
       """Base for all application errors"""
       def __init__(self, message: str, code: str, details: dict = None):
           self.message = message
           self.code = code
           self.details = details or {}
   
   class ValidationError(AppError):
       """User input invalid - return 400"""
   
   class NotFoundError(AppError):
       """Resource doesn't exist - return 404"""
   
   class ServiceError(AppError):
       """Internal failure - return 500, log full details"""
   ```

3. **Rust-style Result pattern** (when applicable):
   ```rust
   // Prefer Result over panic for expected failures
   fn parse_config(path: &str) -> Result<Config, ConfigError> {
       let content = fs::read_to_string(path)
           .map_err(|e| ConfigError::IoError(e))?;
       toml::from_str(&content)
           .map_err(|e| ConfigError::ParseError(e))
   }
   ```

4. **User-facing vs. internal errors**:
   ```python
   # INTERNAL: Full context for debugging
   logger.error("Database query failed", extra={
       "query": sql,
       "params": redact_pii(params),
       "error": str(e),
       "trace_id": request.trace_id
   })
   
   # USER-FACING: Safe, actionable, no internals
   return {"error": "Unable to process request", "code": "SERVICE_ERROR", "request_id": trace_id}
   ```

5. **Error propagation rules**:
   - Add context when crossing boundaries: `raise ServiceError("User lookup failed") from original_error`
   - Use `__cause__` / `from` in Python, `.context()` in Rust, wrapped errors in Go
   - Never swallow errors silently; at minimum log them

6. **Structured error logging**:
   ```python
   # Include: error type, message, trace_id, relevant IDs, stack trace
   # Exclude: passwords, tokens, PII, full request bodies
   logger.exception("Payment processing failed", extra={
       "error_code": "PAYMENT_DECLINED",
       "user_id": user.id,  # OK: internal ID
       "amount": amount,
       "trace_id": trace_id,
       # NOT: card_number, user.email, full_address
   })
   ```

# Output defaults
```markdown
## Error Handling Design

### Error Hierarchy
- `AppError` (base)
  - `ValidationError` → 400
  - `AuthError` → 401/403
  - `NotFoundError` → 404
  - `ConflictError` → 409
  - `ServiceError` → 500

### User Response Format
```json
{
  "error": "Human-readable message",
  "code": "MACHINE_READABLE_CODE",
  "request_id": "abc-123"
}
```

### Logging Standards
- All errors logged with trace_id
- Stack traces for 5xx only
- PII fields redacted: [list]
```

# References
- https://docs.python.org/3/library/exceptions.html
- https://doc.rust-lang.org/book/ch09-00-error-handling.html

# Failure handling
- **Stack traces leaking to users**: Add error boundary/middleware that catches all exceptions and returns safe response
- **Silent failures**: Grep for bare `except:` or `catch {}` blocks; require explicit error handling
- **Too much noise in logs**: Distinguish expected errors (4xx) from unexpected (5xx); only alert on unexpected
- **Missing context in errors**: Use chained exceptions; every `raise` or `return Err` should add the current operation context
