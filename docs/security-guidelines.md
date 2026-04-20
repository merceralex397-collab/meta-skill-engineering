# Security Guidelines

**For:** AI Agents and Developers  
**Purpose:** Security patterns and vulnerability prevention  
**Version:** 1.0

---

## Security-First Principles

**Rule 1:** Never use string concatenation for command arguments.  
**Rule 2:** Always validate file paths before access.  
**Rule 3:** Never use `random` for security-sensitive operations.  
**Rule 4:** Always set timeouts on external operations.  

---

## Process Execution Security

### CWE-78: Command Injection Prevention

**VULNERABLE - Never do this:**
```csharp
// ❌ CRITICAL SECURITY RISK
string arguments = $"--skill \"{skillName}\" --brief \"{brief}\"";
process.StartInfo.Arguments = arguments;
```

**Why this is dangerous:**
- If `skillName` contains `"; rm -rf /"`, it executes arbitrary commands
- String interpolation is injection vulnerability
- No separation between command and data

**SECURE - Always do this:**
```csharp
// ✅ SAFE from command injection
var psi = new ProcessStartInfo
{
    FileName = pythonPath,
    UseShellExecute = false,
    RedirectStandardInput = true,
    RedirectStandardOutput = true,
    RedirectStandardError = true
};

// Use ArgumentList, NOT Arguments string
psi.ArgumentList.Add("--skill");
psi.ArgumentList.Add(skillName);  // Safely escaped
psi.ArgumentList.Add("--brief");
psi.ArgumentList.Add(brief);      // Safely escaped
```

**Verification checklist:**
- [ ] Using `ArgumentList` (not `Arguments`)
- [ ] `UseShellExecute = false`
- [ ] No string interpolation in arguments
- [ ] User input never reaches shell directly

---

## Path Security

### CWE-22: Path Traversal Prevention

**VULNERABLE:**
```csharp
// ❌ DANGEROUS
string path = Path.Combine(baseDir, userInput);
var content = File.ReadAllText(path);  // Can read arbitrary files
```

**SECURE:**
```csharp
// ✅ SAFE
string path = Path.Combine(baseDir, userInput);
string fullPath = Path.GetFullPath(path);
string baseFullPath = Path.GetFullPath(baseDir);

// Verify path is within allowed directory
if (!fullPath.StartsWith(baseFullPath, StringComparison.OrdinalIgnoreCase))
{
    throw new SecurityException("Path traversal attempt detected");
}

var content = File.ReadAllText(fullPath);
```

**Required validation:**
1. Resolve to absolute path
2. Verify path is within allowed base directory
3. Reject path traversal sequences (`../`, `..\`)

---

## Random Number Security

### CWE-338: Weak PRNG

**VULNERABLE:**
```python
# ❌ NEVER for security-sensitive operations
import random
session_id = random.randint(1000, 9999)  # Predictable!
```

**SECURE:**
```python
# ✅ Cryptographically secure
import secrets
session_id = secrets.token_hex(16)  # 256-bit random value
```

**When `random` is acceptable:**
- ML train/test splitting (with documented security note)
- UI animations
- Non-security game mechanics

**When `secrets` is required:**
- Session IDs
- Authentication tokens
- Cryptographic keys
- Password reset tokens

---

## Input Validation

### Always Validate User Input

**Pattern:**
```csharp
public void ExecuteSkill(string skillName, string action)
{
    // 1. Whitelist validation
    if (!IsValidSkillName(skillName))
    {
        throw new ArgumentException("Invalid skill name format");
    }
    
    // 2. Length limits
    if (action?.Length > 1000)
    {
        throw new ArgumentException("Action too long (max 1000 chars)");
    }
    
    // 3. Character filtering
    if (action?.Any(c => char.IsControl(c) && c != '\n' && c != '\r') == true)
    {
        throw new ArgumentException("Action contains invalid characters");
    }
    
    // 4. Execute
    _service.ExecuteSkillAsync(skillName, action);
}
```

### Validation Requirements

| Input Type | Validation Required |
|------------|-------------------|
| File paths | Path traversal check |
| Command arguments | ArgumentList usage |
| Identifiers | Whitelist regex |
| User content | Length limits, character filtering |
| URLs | Scheme validation (http/https only) |

---

## Timeout Protection

### CWE-400: Resource Exhaustion Prevention

**Always set timeouts:**
```csharp
// Process timeout (30 minutes)
using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(30));
await process.WaitForExitAsync(cts.Token);

// Kill if still running
if (!process.HasExited)
{
    process.Kill();
}
```

### Regex Timeout

**Pattern (via RegexCache):**
```csharp
// RegexCache enforces 100ms timeout
var regex = RegexCache.GetOrCreate(
    pattern,
    RegexOptions.Compiled,
    TimeSpan.FromMilliseconds(100)  // Timeout
);
```

**Never:**
```csharp
// ❌ Can hang on catastrophic backtracking
var match = Regex.Match(input, @"(a+)+$");  // No timeout!
```

---

## Temporary Files

### CWE-377: Insecure Temporary Files

**VULNERABLE:**
```python
# ❌ Predictable filename
temp_file = f"/tmp/eval_{random.randint(1000, 9999)}.json"
```

**SECURE:**
```python
# ✅ Unpredictable, secure location
import tempfile
with tempfile.NamedTemporaryFile(
    mode='w',
    suffix='.json',
    delete=False
) as f:
    temp_path = f.name
    json.dump(data, f)
```

**Additional requirements:**
- Set restrictive permissions (0600)
- Delete after use (try/finally)
- Use library functions (don't roll your own)

---

## Data Deserialization

### CWE-502: Deserialization of Untrusted Data

**JSON Deserialization:**
```csharp
// ✅ Safe for trusted internal data
var config = JsonSerializer.Deserialize<AppConfiguration>(json);

// ⚠️ Validate schema for external data
var jsonDoc = JsonDocument.Parse(json);
if (!jsonDoc.RootElement.TryGetProperty("version", out _))
{
    throw new JsonException("Invalid configuration format");
}
```

**Never use BinaryFormatter or similar:**
```csharp
// ❌ NEVER - RCE vulnerability
var formatter = new BinaryFormatter();
var obj = formatter.Deserialize(stream);
```

---

## Secure Coding Checklist

Before claiming work complete, verify:

### Process Execution
- [ ] `ArgumentList` used (not `Arguments` string)
- [ ] `UseShellExecute = false`
- [ ] No shell metacharacters in arguments
- [ ] Timeout configured (max 30 minutes)
- [ ] Process killed if timeout exceeded

### File Operations
- [ ] Path traversal check performed
- [ ] Base directory enforced
- [ ] Symlink traversal prevented
- [ ] File size limits enforced

### Input Handling
- [ ] Whitelist validation for identifiers
- [ ] Length limits enforced
- [ ] Character filtering applied
- [ ] No direct user input in commands

### Randomness
- [ ] `secrets` module for security operations
- [ ] `random` module only for non-security uses
- [ ] Documented which is used and why

### Timeouts
- [ ] All external operations have timeouts
- [ ] Regex operations have 100ms timeout
- [ ] Network operations have connection timeouts
- [ ] Cancellation tokens properly passed

### Error Handling
- [ ] Exceptions logged (not swallowed)
- [ ] Sensitive data not logged
- [ ] Stack traces not exposed to users
- [ ] Error messages are informative but safe

---

## Security Review Questions

For any code change, ask:

1. **Does this accept user input?** → Validate and sanitize
2. **Does this execute external commands?** → Use ArgumentList
3. **Does this access files?** → Check for path traversal
4. **Does this generate IDs/tokens?** → Use secrets module
5. **Does this have network access?** → Set connection timeouts
6. **Does this use regex?** → Set timeout protection
7. **Does this create temp files?** → Use secure library functions

If any answer is "yes" and protection is missing, **DO NOT COMMIT**.

---

## Common Vulnerabilities Reference

| CWE | Issue | Prevention |
|-----|-------|------------|
| CWE-78 | Command injection | Use ArgumentList |
| CWE-22 | Path traversal | Validate paths |
| CWE-338 | Weak PRNG | Use secrets module |
| CWE-377 | Insecure temp files | Use tempfile library |
| CWE-400 | Resource exhaustion | Set timeouts |
| CWE-502 | Unsafe deserialization | Use JsonSerializer safely |
| CWE-312 | Cleartext storage | Encrypt sensitive data |

---

## Related Documentation

- **AGENTS.md:** `../AGENTS.md` - Behavioral guardrails and verification protocols
- **CONTRIBUTING.md:** `../CONTRIBUTING.md` - How to contribute to this repository
- **Workflow:** `docs/workflow.md` - Development workflow patterns
- **Architecture:** `docs/architecture.md` - System architecture and patterns
- **Code Style:** `docs/code-style.md` - Coding conventions
- **Testing:** `docs/testing-guide.md` - Testing requirements and patterns
- **Troubleshooting:** `docs/troubleshooting.md` - Common issues and solutions
- **Evaluation Cadence:** `docs/evaluation-cadence.md` - When to run which tests
