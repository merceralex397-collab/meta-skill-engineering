---
name: external-api-client
description: Standardizes retries, backoff, timeout, idempotency, and circuit-breaker decisions when consuming third-party APIs. Trigger on "call external API", "retry logic", "rate limiting", "circuit breaker", "HTTP client wrapper". Do NOT use for api-schema (designing APIs), auth-patterns (auth implementation), or mcp-protocol (MCP servers).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: external-api-client
  maturity: draft
  risk: low
  tags: [external, api, client]
---

# Purpose
Build reliable API clients that handle real-world network conditions: transient failures, rate limits, auth token refresh, and timeouts. Following patterns from production APIs like Anthropic's demonstrates what mature clients look like.

# When to use this skill
Use when:
- Integrating a new third-party API (payment, AI, cloud services)
- Existing API client has reliability issues (random failures, rate limit errors)
- Building a wrapper/SDK around an external service
- API calls are in critical paths (checkout, data sync)

Do NOT use when:
- Calling internal services with guaranteed SLAs (use simpler client)
- One-off scripts where manual retry is acceptable

# Operating procedure
1. **Set appropriate timeouts** (never use defaults blindly):
   ```python
   # Anthropic-style: different timeouts for different operations
   client = httpx.Client(
       timeout=httpx.Timeout(
           connect=5.0,      # Connection establishment
           read=60.0,        # Waiting for response (LLM calls are slow!)
           write=10.0,       # Sending request body
           pool=5.0          # Waiting for connection from pool
       )
   )
   ```

2. **Implement exponential backoff with jitter**:
   ```python
   import random
   import time
   
   def retry_with_backoff(fn, max_retries=3, base_delay=1.0):
       for attempt in range(max_retries):
           try:
               return fn()
           except (RateLimitError, ServiceUnavailableError) as e:
               if attempt == max_retries - 1:
                   raise
               delay = base_delay * (2 ** attempt) + random.uniform(0, 1)
               time.sleep(delay)
   ```

3. **Handle rate limits explicitly**:
   ```python
   def call_api(request):
       response = client.post("/v1/messages", json=request)
       
       if response.status_code == 429:
           retry_after = int(response.headers.get("retry-after", 60))
           raise RateLimitError(f"Rate limited, retry after {retry_after}s")
       
       if response.status_code >= 500:
           raise ServiceUnavailableError("Service temporarily unavailable")
       
       response.raise_for_status()
       return response.json()
   ```

4. **Implement auth token refresh**:
   ```python
   class APIClient:
       def __init__(self, credentials):
           self._credentials = credentials
           self._token = None
           self._token_expires_at = 0
       
       def _ensure_token(self):
           if time.time() >= self._token_expires_at - 60:  # Refresh 1 min early
               self._token = self._refresh_token()
               self._token_expires_at = time.time() + 3600
       
       def request(self, method, path, **kwargs):
           self._ensure_token()
           headers = {"Authorization": f"Bearer {self._token}"}
           return self._client.request(method, path, headers=headers, **kwargs)
   ```

5. **Use circuit breaker for cascading failure protection**:
   ```python
   # After N consecutive failures, stop calling for cooldown period
   class CircuitBreaker:
       def __init__(self, failure_threshold=5, cooldown=30):
           self.failures = 0
           self.last_failure_time = 0
           self.threshold = failure_threshold
           self.cooldown = cooldown
       
       def call(self, fn):
           if self.failures >= self.threshold:
               if time.time() - self.last_failure_time < self.cooldown:
                   raise CircuitOpenError("Circuit breaker open")
               self.failures = 0  # Reset for retry
           
           try:
               result = fn()
               self.failures = 0
               return result
           except Exception:
               self.failures += 1
               self.last_failure_time = time.time()
               raise
   ```

6. **Ensure idempotency for retryable operations**:
   ```python
   # Use idempotency keys for non-idempotent operations
   def create_payment(amount, idempotency_key=None):
       key = idempotency_key or str(uuid.uuid4())
       response = client.post("/payments", 
           json={"amount": amount},
           headers={"Idempotency-Key": key}
       )
       return response.json()
   ```

# Output defaults
```python
class ExternalAPIClient:
    """
    Client for [Service Name] API
    
    Features:
    - Exponential backoff with jitter (max 3 retries)
    - Rate limit handling with Retry-After header
    - Automatic token refresh
    - Circuit breaker (5 failures = 30s cooldown)
    - Configurable timeouts (connect: 5s, read: 60s)
    """
    
    RETRYABLE_STATUS_CODES = {429, 500, 502, 503, 504}
```

# References
- https://docs.anthropic.com/en/api/getting-started
- https://docs.anthropic.com/en/api/rate-limits

# Failure handling
- **Infinite retry loops**: Always set max_retries; log when exhausted
- **Rate limit storms**: Implement client-side rate limiting; don't rely only on 429 responses
- **Auth token refresh race**: Use mutex/lock when multiple threads share token
- **Timeout too short for operation**: Research API's expected latency; AI APIs often need 60s+ read timeout
- **Missing idempotency key on retry**: Always generate key BEFORE first attempt, reuse on retries
