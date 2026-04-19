---
name: security-hardening
description: Apply OWASP Top 10 mitigations as concrete grep-able patterns and code fixes during development and review. Trigger on "harden application", "OWASP review", "security controls", "injection prevention", "XSS prevention", or security audit response. Do not use for security-best-practices (language-specific secure coding guidance), security-threat-model (threat enumeration and abuse paths), or auth-patterns (authentication/authorization implementation).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: security-hardening
  maturity: draft
  risk: low
  tags: [security, hardening]
---

# Purpose
Apply OWASP Top 10 mitigations to code during development and review. Each vulnerability class has specific, checkable defenses—not vague "be careful" advice. This skill translates threat categories into grep-able patterns and concrete fixes.

# When to use this skill
Use when:
- Code review with security focus
- Hardening existing application before launch
- Adding security controls to new feature
- Responding to security audit findings

Do NOT use when:
- Full penetration testing needed (hire specialists)
- Compliance audit (use compliance-specific checklists)
- Infrastructure security (different skill set)

# Operating procedure
1. **A01:2021 - Broken Access Control** (most critical):
   ```python
   # ❌ Direct object reference without authorization check
   @app.get("/users/{user_id}/data")
   def get_user_data(user_id: int):
       return db.get_user_data(user_id)  # Anyone can access any user!
   
   # ✅ Verify authorization
   @app.get("/users/{user_id}/data")
   def get_user_data(user_id: int, current_user: User = Depends(get_current_user)):
       if current_user.id != user_id and not current_user.is_admin:
           raise HTTPException(403, "Not authorized")
       return db.get_user_data(user_id)
   ```

2. **A02:2021 - Cryptographic Failures**:
   ```python
   # ❌ Weak hashing
   password_hash = hashlib.md5(password.encode()).hexdigest()
   
   # ✅ Use bcrypt/argon2 for passwords
   from passlib.hash import argon2
   password_hash = argon2.hash(password)
   
   # ❌ Hardcoded secrets
   API_KEY = "sk-1234567890"
   
   # ✅ Environment variables
   API_KEY = os.environ["API_KEY"]
   ```

3. **A03:2021 - Injection** (SQL, Command, LDAP):
   ```python
   # ❌ SQL injection
   query = f"SELECT * FROM users WHERE email = '{email}'"
   
   # ✅ Parameterized queries
   query = "SELECT * FROM users WHERE email = %s"
   cursor.execute(query, (email,))
   
   # ❌ Command injection
   os.system(f"convert {user_filename} output.png")
   
   # ✅ Use subprocess with list arguments
   subprocess.run(["convert", user_filename, "output.png"], check=True)
   ```

4. **A04:2021 - Insecure Design** (architecture level):
   ```python
   # ❌ No rate limiting on sensitive endpoints
   @app.post("/login")
   def login(credentials: Credentials):
       return authenticate(credentials)
   
   # ✅ Rate limit authentication attempts
   from slowapi import Limiter
   limiter = Limiter(key_func=get_remote_address)
   
   @app.post("/login")
   @limiter.limit("5/minute")
   def login(credentials: Credentials):
       return authenticate(credentials)
   ```

5. **A05:2021 - Security Misconfiguration**:
   ```python
   # ❌ Debug mode in production
   app.run(debug=True)
   
   # ✅ Environment-based configuration
   app.run(debug=os.environ.get("ENV") == "development")
   
   # ❌ Default credentials
   ADMIN_PASSWORD = "admin"
   
   # ✅ Require configuration
   ADMIN_PASSWORD = os.environ["ADMIN_PASSWORD"]
   if not ADMIN_PASSWORD:
       raise RuntimeError("ADMIN_PASSWORD must be set")
   ```

6. **A07:2021 - Cross-Site Scripting (XSS)**:
   ```html
   <!-- ❌ Unescaped user content -->
   <div>{{ user_input | safe }}</div>
   
   <!-- ✅ Auto-escaped (default in most frameworks) -->
   <div>{{ user_input }}</div>
   
   <!-- ✅ Content Security Policy header -->
   Content-Security-Policy: default-src 'self'; script-src 'self'
   ```

7. **Quick security grep patterns**:
   ```bash
   # Find potential SQL injection
   grep -rn "f\"SELECT\|f'SELECT\|\.format.*SELECT" --include="*.py"
   
   # Find hardcoded secrets
   grep -rn "password\s*=\s*['\"]" --include="*.py"
   grep -rn "api_key\s*=\s*['\"]" --include="*.py"
   
   # Find dangerous functions
   grep -rn "eval(\|exec(\|os\.system(" --include="*.py"
   grep -rn "shell=True" --include="*.py"
   
   # Find missing auth decorators
   grep -rn "@app\.\(get\|post\|put\|delete\)" --include="*.py" | grep -v "@requires_auth"
   ```

# Output defaults
```markdown
## Security Review: [Component/Feature]

### Findings
| ID | Category | Severity | Location | Issue | Fix |
|----|----------|----------|----------|-------|-----|
| 1 | A03 Injection | High | api/users.py:45 | SQL string formatting | Use parameterized query |
| 2 | A01 Access Control | Medium | api/orders.py:23 | Missing ownership check | Add user_id verification |

### Mitigations Applied
- [ ] Input validation on all endpoints
- [ ] Parameterized queries for all DB access
- [ ] Authorization checks on resource access
- [ ] Rate limiting on auth endpoints
- [ ] Security headers configured

### Headers to Set
```
Content-Security-Policy: default-src 'self'
X-Content-Type-Options: nosniff
X-Frame-Options: DENY
Strict-Transport-Security: max-age=31536000; includeSubDomains
```
```

# References
- https://owasp.org/www-project-top-ten/
- https://cheatsheetseries.owasp.org/

# Failure handling
- **Finding too many issues**: Prioritize by exploitability and impact; fix critical auth/injection first
- **Can't fix without major refactor**: Document as tech debt with risk assessment; add compensating controls
- **Third-party library vulnerable**: Check if newer version available; if not, evaluate alternatives or add wrapper validation
- **Security vs. usability conflict**: Document the tradeoff explicitly; get business decision rather than silently accepting risk
- **Not sure if issue is exploitable**: Assume it is; fix it anyway—defense in depth
