---
name: testing
description: Choose and structure appropriate testing layers for the current stack — unit, integration, e2e, snapshot, or property-based. Trigger on "testing strategy", "add tests", "test pyramid", "test coverage", "flaky tests", "property-based testing", or "test organization". Do not use for review-audit-bridge (reviewing code quality), performance-baseline (benchmarks and load testing), or deployment-pipeline (CI setup).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: testing
  maturity: draft
  risk: low
  tags: [testing]
---

# Purpose
Structure tests according to the testing pyramid: many fast unit tests, fewer integration tests, minimal E2E tests. Avoid the ice cream cone anti-pattern where slow E2E tests dominate. Good test coverage means high confidence in refactoring, not chasing a percentage number.

# When to use this skill
Use when:
- Setting up testing strategy for new project
- Adding tests to existing codebase
- Deciding what type of test to write for a feature
- Test suite is slow or flaky and needs restructuring

Do NOT use when:
- One-off scripts that won't be maintained
- Prototypes explicitly marked as throwaway

# Operating procedure
1. **Apply the testing pyramid**:
   ```
        /\          E2E (Few)
       /  \         - Full user flows
      /    \        - Slow, expensive
     /------\       
    /        \      Integration (Some)
   /          \     - Component boundaries
  /            \    - Database, API calls
 /--------------\   
/                \  Unit (Many)
                    - Pure functions
                    - Fast, isolated
   ```

2. **Unit test design** (fast, isolated, many):
   ```python
   # Test pure functions thoroughly
   def test_calculate_discount():
       assert calculate_discount(100, "SAVE10") == 90
       assert calculate_discount(100, "INVALID") == 100
       assert calculate_discount(0, "SAVE10") == 0
   
   # Mock external dependencies
   def test_user_service_get_user(mocker):
       mock_db = mocker.patch('app.db.get_user')
       mock_db.return_value = {"id": 1, "name": "Alice"}
       
       result = user_service.get_user(1)
       
       assert result.name == "Alice"
       mock_db.assert_called_once_with(1)
   ```

3. **Integration test design** (real dependencies, boundaries):
   ```python
   # Test with real database
   @pytest.fixture
   def db_session():
       engine = create_engine("postgresql://localhost/test_db")
       with engine.connect() as conn:
           yield conn
           conn.rollback()  # Clean up after each test
   
   def test_create_and_retrieve_user(db_session):
       user_repo = UserRepository(db_session)
       
       created = user_repo.create(email="test@example.com")
       retrieved = user_repo.get_by_email("test@example.com")
       
       assert retrieved.id == created.id
   ```

4. **E2E test design** (critical paths only):
   ```python
   # Only test the most critical user journeys
   def test_user_signup_and_purchase_flow(browser):
       # Navigate to signup
       browser.get("/signup")
       browser.fill("email", "new@example.com")
       browser.fill("password", "secure123")
       browser.click("submit")
       
       # Verify logged in
       assert browser.url == "/dashboard"
       
       # Add to cart and checkout
       browser.get("/products/1")
       browser.click("add-to-cart")
       browser.get("/checkout")
       browser.click("complete-purchase")
       
       # Verify order created
       assert "Order confirmed" in browser.text
   ```

5. **Property-based testing** (for edge cases):
   ```python
   from hypothesis import given, strategies as st
   
   @given(st.lists(st.integers()))
   def test_sort_is_idempotent(xs):
       """Sorting twice should give same result as sorting once"""
       assert sorted(sorted(xs)) == sorted(xs)
   
   @given(st.text())
   def test_json_roundtrip(s):
       """Encoding and decoding should preserve data"""
       assert json.loads(json.dumps(s)) == s
   ```

6. **Test organization**:
   ```
   tests/
   ├── unit/                    # Fast, no I/O
   │   ├── test_calculator.py
   │   └── test_validators.py
   ├── integration/             # Real DB, APIs
   │   ├── test_user_repo.py
   │   └── test_payment_service.py
   ├── e2e/                     # Full browser/API flows
   │   └── test_checkout_flow.py
   ├── conftest.py              # Shared fixtures
   └── fixtures/                # Test data
       └── users.json
   ```

7. **Coverage that matters**:
   ```bash
   # Coverage as a tool, not a goal
   pytest --cov=app --cov-report=term-missing
   
   # Focus on:
   # - Critical business logic: 100%
   # - Error handling paths: high
   # - Edge cases: property tests
   # - Boilerplate/glue code: lower priority
   
   # DON'T chase 100% overall - it leads to useless tests
   ```

# Output defaults
```markdown
## Testing Strategy: [Project Name]

### Test Distribution
| Layer | Count | Run Time | Coverage |
|-------|-------|----------|----------|
| Unit | ~500 | <30s | 80%+ |
| Integration | ~50 | <5min | Key boundaries |
| E2E | ~10 | <10min | Critical paths |

### Running Tests
```bash
# Unit tests only (fast feedback)
pytest tests/unit/ -x

# Full test suite
pytest

# With coverage
pytest --cov=app --cov-report=html
```

### CI Configuration
- Unit tests: Run on every push
- Integration: Run on PR, requires services
- E2E: Run before deploy to staging
```

# References
- https://martinfowler.com/articles/practical-test-pyramid.html
- https://martinfowler.com/bliki/TestDouble.html
- https://docs.pytest.org/
- https://hypothesis.readthedocs.io/

# Failure handling
- **Tests too slow**: Move logic from integration to unit tests by extracting pure functions; parallelize test runs
- **Flaky tests**: Quarantine and fix; usually timing, shared state, or external dependencies
- **Low coverage feels bad but tests are pointless**: Delete tests that don't catch bugs; add tests for code that breaks
- **Hard to test code**: Refactor for testability; inject dependencies; extract pure functions
- **Too many mocks**: Sign that integration test would be more valuable; or code needs restructuring
