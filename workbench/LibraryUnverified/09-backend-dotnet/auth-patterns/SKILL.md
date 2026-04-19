---
name: auth-patterns
description: Implements authentication, session, token, and authorization patterns for the current stack. Trigger on 'add auth', 'JWT', 'OAuth', 'login endpoint', 'session management', 'API key auth'. DO NOT USE for OWASP hardening checklists (use security-hardening), threat modeling (use security-threat-model), or secret rotation/storage (use security-best-practices).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: auth-patterns
  maturity: draft
  risk: low
  tags: [auth, patterns]
---

# Purpose
Implement authentication and authorization correctly by choosing the right pattern for the use case. JWT for stateless APIs, OAuth2 for third-party access, session tokens for web apps. Understand the tradeoffs and common security mistakes for each approach.

# When to use this skill
Use when:
- Adding authentication to a new service
- Integrating OAuth2/social login
- Designing API authentication strategy
- Reviewing auth implementation for security issues

Do NOT use when:
- Internal service-to-service calls with mTLS (different pattern)
- Public read-only APIs (may not need auth)

# Operating procedure
1. **Choose auth mechanism by use case**:
   ```
   Use Case                          → Pattern
   ──────────────────────────────────────────────────
   Web app with sessions             → Session cookies + CSRF token
   SPA calling own API               → HttpOnly cookies or short-lived JWT
   Third-party API access            → OAuth 2.0 Authorization Code flow
   Mobile app                        → OAuth 2.0 + PKCE
   Server-to-server                  → Client Credentials or API keys
   Microservices internal            → JWT (from gateway) or mTLS
   ```

2. **JWT implementation** (per RFC 7519):
   ```python
   # Structure: header.payload.signature
   # Header
   {"alg": "RS256", "typ": "JWT"}  # Use RS256, not HS256 for distributed systems
   
   # Payload - standard claims
   {
       "iss": "https://auth.myapp.com",  # Issuer
       "sub": "user_123",                 # Subject (user ID)
       "aud": "https://api.myapp.com",    # Audience
       "exp": 1704067200,                 # Expiration (REQUIRED)
       "iat": 1704063600,                 # Issued at
       "jti": "unique-token-id"           # JWT ID (for revocation)
   }
   ```

3. **JWT validation checklist**:
   ```python
   def validate_jwt(token: str) -> Claims:
       # 1. Verify signature with public key
       # 2. Check exp > current_time
       # 3. Check iss matches expected issuer
       # 4. Check aud contains this service
       # 5. Check token not in revocation list (if using jti)
       
       claims = jwt.decode(
           token,
           public_key,
           algorithms=["RS256"],  # Whitelist algorithms!
           audience="https://api.myapp.com",
           issuer="https://auth.myapp.com"
       )
       return claims
   ```

4. **OAuth 2.0 Authorization Code flow** (per RFC 6749):
   ```
   1. App redirects user to authorization server:
      GET /authorize?
        response_type=code&
        client_id=CLIENT_ID&
        redirect_uri=https://app.com/callback&
        scope=read:profile&
        state=RANDOM_STATE  # CSRF protection
   
   2. User authenticates, grants consent
   
   3. Authorization server redirects back:
      GET /callback?code=AUTH_CODE&state=RANDOM_STATE
   
   4. App exchanges code for tokens (server-side):
      POST /token
        grant_type=authorization_code&
        code=AUTH_CODE&
        redirect_uri=https://app.com/callback&
        client_id=CLIENT_ID&
        client_secret=CLIENT_SECRET
   
   5. Response contains access_token (and optionally refresh_token)
   ```

5. **Common mistakes to avoid**:
   ```python
   # ❌ Storing JWT in localStorage (XSS vulnerable)
   localStorage.setItem('token', jwt)
   
   # ✅ Use HttpOnly cookie
   response.set_cookie('token', jwt, httponly=True, secure=True, samesite='Lax')
   
   # ❌ Long-lived access tokens
   exp = now + timedelta(days=30)
   
   # ✅ Short access tokens + refresh tokens
   access_exp = now + timedelta(minutes=15)
   refresh_exp = now + timedelta(days=7)
   
   # ❌ Accepting any algorithm
   jwt.decode(token, key, algorithms=jwt.get_unverified_header(token)['alg'])
   
   # ✅ Whitelist specific algorithms
   jwt.decode(token, key, algorithms=["RS256"])
   ```

6. **API key pattern** (for server-to-server):
   ```python
   # Generate secure keys
   api_key = secrets.token_urlsafe(32)  # 256 bits
   
   # Store hashed in database
   key_hash = hashlib.sha256(api_key.encode()).hexdigest()
   
   # Validate on request
   def validate_api_key(request):
       key = request.headers.get('X-API-Key')
       key_hash = hashlib.sha256(key.encode()).hexdigest()
       return db.api_keys.find_one({'hash': key_hash, 'active': True})
   ```

# Output defaults
```markdown
## Authentication Design

### Pattern
- Mechanism: [JWT/OAuth2/Session/API Key]
- Token storage: [HttpOnly cookie/Header]
- Token lifetime: Access [X min], Refresh [Y days]

### Endpoints
- POST /auth/login → Returns tokens
- POST /auth/refresh → Exchanges refresh for new access
- POST /auth/logout → Invalidates refresh token

### JWT Claims
```json
{
  "iss": "[issuer]",
  "sub": "[user_id]",
  "exp": "[timestamp]",
  "roles": ["user", "admin"]
}
```

### Authorization
- Role-based: [roles and permissions]
- Resource-based: [ownership checks]
```

# References
- OAuth 2.0 (RFC 6749): https://datatracker.ietf.org/doc/html/rfc6749
- JWT (RFC 7519): https://datatracker.ietf.org/doc/html/rfc7519
- https://jwt.io/introduction

# Failure handling
- **JWT expired but user active**: Implement transparent refresh; return 401 with refresh hint
- **Refresh token stolen**: Implement refresh token rotation (new refresh token each use); detect reuse
- **Algorithm confusion attack**: Never accept `alg: none`; always whitelist specific algorithms
- **CSRF on cookie-based auth**: Use SameSite=Lax/Strict; add CSRF token for state-changing requests
- **Token revocation needed**: Store jti in Redis with TTL; check on each request (adds latency)
