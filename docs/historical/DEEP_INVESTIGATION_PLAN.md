# 🔍 DEEP INVESTIGATION PLAN
## Meta-Skill-Engineering Project: Comprehensive Analysis & Remediation Strategy

---

## EXECUTIVE SUMMARY

Based on the 20-agent parallel scan and 3x explore agent analysis, this project has **174 unique issues** across 50+ files. The investigation requires a **4-phase, 170-240 hour remediation plan** with strategic tool deployment and agent delegation.

---

## PHASE 0: ENVIRONMENT SETUP & TOOL DEPLOYMENT
**Duration: 4-6 hours**

### Available Tools (Current)
| Tool | Version | Status | Use Case |
|------|---------|--------|----------|
| dotnet | 10.0.104 | ✅ Available | Build, test, analyze |
| python3 | 3.12.3 | ✅ Available | Script validation, tooling |
| jq | 1.7.1 | ✅ Available | JSON processing |
| git | latest | ✅ Available | Version control |

### Tools to Install (Priority Order)

#### P0 - Critical for Analysis
| Tool | Installation | Purpose | Agent Type |
|------|-------------|---------|------------|
| **shellcheck** | `sudo apt-get install shellcheck` | Bash script static analysis | General agent |
| **yamllint** | `pip3 install yamllint` | YAML validation (GitHub Actions, SKILL.md) | General agent |
| **markdownlint** | `npm install -g markdownlint-cli` | SKILL.md structural validation | General agent |
| **dotnet-format** | `dotnet tool install -g dotnet-format` | C# code formatting analysis | General agent |
| **dotnet-roslynator** | `dotnet tool install -g roslynator.dotnet.cli` | C# code analysis | General agent |

#### P1 - High Value for Deep Analysis
| Tool | Installation | Purpose | Agent Type |
|------|-------------|---------|------------|
| **bandit** | `pip3 install bandit` | Python security scanner | General agent |
| **pylint** | `pip3 install pylint` | Python code quality | General agent |
| **mypy** | `pip3 install mypy` | Python type checking | General agent |
| **python-semgrep** | `pip3 install semgrep` | Static analysis for security patterns | General agent |
| **roslyn-analyzers** | Project-level NuGet | C# security/quality analysis | General agent |

#### P2 - Enhanced Analysis
| Tool | Installation | Purpose |
|------|-------------|---------|
| **psalm** | `composer require --dev psalm` | Static analysis (if PHP added) |
| **trivy** | `curl -sfL https://raw.githubusercontent.com/aquasecurity/trivy/main/contrib/install.sh` | Container/file scanning |
| **grype** | `curl -sSfL https://raw.githubusercontent.com/anchore/grype/main/install.sh` | Vulnerability scanner |

### Virtual Environment Setup
```bash
# Python virtual environment for tooling
python3 -m venv /home/rowan/Meta-Skill-Engineering/.venv
source /home/rowan/Meta-Skill-Engineering/.venv/bin/activate
pip install bandit pylint mypy yamllint semgrep
```

### Roslyn Analyzer Configuration
Create `/.editorconfig` and enable all security analyzers:
```ini
[*.cs]
dotnet_diagnostic.CS8019.severity = error  # Unused usings
dotnet_diagnostic.CA3001.severity = error  # Command injection
dotnet_diagnostic.CA3002.severity = error  # XSS
dotnet_diagnostic.CA3003.severity = error  # File path injection
```

---

## PHASE 1: FOUNDATION REMEDIATION (Tier 1-2)
**Duration: 40-60 hours | Priority: P0**

### 1.1 Build System Stabilization (20 hours)
**Agent Delegation:** 5 parallel general agents + 1 explore agent

```yaml
Task: fix-build-errors
Type: General agents (5 parallel)
Files:
  - Views/SkillSelectionDialog.xaml.cs
  - Views/SettingsDialog.xaml.cs
  - Views/RunDetailsDialog.xaml.cs
  - Views/CreateSkillDialog.xaml.cs
  - Views/BenchmarkDialog.xaml.cs
  - Views/PipelineDialog.xaml.cs
  - Services/DialogService.cs
  - Helpers/AnalyticsCalculator.cs
  - Services/PythonRuntimeService.cs
Commands:
  - dotnet build (to verify each fix)
  - dotnet format (to standardize)
```

**Detailed Fix Plan:**

| Error Category | Files | Fix Strategy | Estimated Hours |
|----------------|-------|--------------|-----------------|
| Using aliases (12 errors) | 9 files | Add `using X = System.Windows.X;` | 2 |
| Property renames (17 errors) | 5 files | Replace with Utc suffix versions | 3 |
| ScottPlot API (12 errors) | AnalyticsDialog | Rewrite chart rendering | 10 |
| Constructor mismatch | PipelineDialog | Fix DI injection | 2 |
| Type conversion | PythonRuntimeService | Fix signature | 1 |
| Duplicate enum | AnalyticsCalculator | Remove duplicate | 1 |
| **Verification** | All | `dotnet build` passes | 1 |

**Interdependencies:** This must complete before Phase 1.2 (cannot test non-compiling code)

### 1.2 Critical Security Patches (15 hours)
**Agent Delegation:** 3 parallel security specialist agents

```yaml
Task: security-remediation
Type: General agents (with security focus)
Priority: P0 (blocks production)
Tools: bandit, semgrep, custom security analyzers
```

| CWE | File | Lines | Fix Strategy | Agent Command |
|-----|------|-------|--------------|---------------|
| CWE-78 | PipelineDialog.xaml.cs | 103-106 | Replace Arguments string with ArgumentList | `sed -i 's/Arguments = /ArgumentList.Add()/'` + validation |
| CWE-78 | scripts/run-evals.sh | 100,142 | Quote variables, validate inputs | shellcheck + manual review |
| CWE-22 | RunDetailsDialog.xaml.cs | 41 | Add path validation | Add `Path.GetFullPath()` check |
| CWE-338 | skill creator/.../run_loop.py | 11,24-34 | Replace random with secrets | `sed -i 's/import random/import secrets/'` |

### 1.3 Infrastructure Completion (5 hours)
**Agent Delegation:** 2 general agents

```yaml
Task: infrastructure-completion
Commands:
  - mkdir -p /home/rowan/Meta-Skill-Engineering/pre-commit-check
  - mkdir -p /home/rowan/Meta-Skill-Engineering/nightly-full-test
  - ln -s ../../scripts/pre-commit-check.sh .git/hooks/pre-commit
  - touch /home/rowan/Meta-Skill-Engineering/skill-orchestrator/references/conditional-branching-rules.md
  - touch /home/rowan/Meta-Skill-Engineering/CONTRIBUTING.md
  - touch /home/rowan/Meta-Skill-Engineering/LICENSE
```

---

## PHASE 2: ARCHITECTURE REMEDIATION (Tier 2-3)
**Duration: 60-90 hours | Priority: P1**

### 2.1 MVVM Extraction (25 hours)
**Agent Delegation:** 4 parallel general agents for each ViewModel

| ViewModel | Source File | Lines to Extract | New File |
|-----------|-------------|------------------|----------|
| SettingsViewModel | SettingsDialog.xaml.cs | 48-359 | /ViewModels/SettingsViewModel.cs |
| RunDetailsViewModel | RunDetailsDialog.xaml.cs | 41-198 | /ViewModels/RunDetailsViewModel.cs |
| PipelineViewModel | PipelineDialog.xaml.cs | 28-214 | /ViewModels/PipelineViewModel.cs |
| BenchmarkViewModel | BenchmarkDialog.xaml.cs | 11-166 | /ViewModels/BenchmarkViewModel.cs |

```yaml
Task: extract-viewmodels
Type: General agents (4 parallel)
Steps:
  1. Identify all INPC properties in code-behind
  2. Extract to new ViewModel class
  3. Move business logic (I/O, JSON, process) to ViewModel
  4. Update XAML DataContext bindings
  5. Leave only event forwarding in code-behind
  6. Add unit tests for new ViewModel
Verification:
  - Code-behind lines reduced by >50%
  - ViewModel testable with mocks
  - No business logic remaining in View
```

### 2.2 Type Consolidation (10 hours)
**Agent Delegation:** 2 general agents

```yaml
Task: consolidate-types
Type: General agents
Files:
  - Models/ApplicationModels.cs (keep these)
  - Helpers/AnalyticsCalculator.cs (remove duplicates)
  - ViewModels/AnalyticsViewModel.cs (remove duplicates)
Changes:
  - Remove TrendDirection from AnalyticsCalculator (line 208)
  - Remove AlertItem from AnalyticsCalculator (line 176)
  - Remove AlertSeverity from AnalyticsCalculator (line 197)
  - Remove AlertItem from AnalyticsViewModel (line 312)
  - Remove AlertSeverity from AnalyticsViewModel (line 326)
  - Update all references to use Models namespace
Verification:
  - dotnet build (no duplicate type errors)
  - All tests pass
```

### 2.3 Dependency Injection Hardening (15 hours)
**Agent Delegation:** 3 general agents

```yaml
Task: di-hardening
Type: General agents
Files:
  - Views/PipelineDialog.xaml.cs
  - ViewModels/MainViewModel.cs
  - Services/PythonRuntimeService.cs
Changes:
  - PipelineDialog: Inject IPythonRuntimeService via constructor
  - MainViewModel: Replace Environment.GetEnvironmentVariable with IEnvironmentProvider
  - Create IProcessService abstraction for testability
Verification:
  - All services resolved from container
  - No new PythonRuntimeService() calls
  - Unit tests can mock all dependencies
```

### 2.4 Async/Await Best Practices (10 hours)
**Agent Delegation:** 2 general agents

```yaml
Task: async-cleanup
Type: General agents
Commands:
  - find . -name "*.cs" -exec grep -l "async void" {} \;
  - find . -name "*.cs" -exec grep -L "ConfigureAwait" {} \;
Changes:
  - Add ConfigureAwait(false) to all service layer calls
  - Add CancellationToken to 4 missing service methods
  - Replace WaitForExit with async equivalent
Verification:
  - No async void (except event handlers)
  - All async calls have ConfigureAwait
```

---

## PHASE 3: QUALITY & SECURITY HARDENING (Tier 3)
**Duration: 50-70 hours | Priority: P2**

### 3.1 Security Audit Remediation (20 hours)
**Agent Delegation:** 3 security specialist agents + automated scanning

```yaml
Task: security-hardening
Type: General agents + automated tools
Tools: bandit, semgrep, roslyn security analyzers, trivy
Parallel Scans:
  - bandit -r /home/rowan/Meta-Skill-Engineering/skill creator/ -f json
  - semgrep --config=auto /home/rowan/Meta-Skill-Engineering/
  - roslynator analyze /home/rowan/Meta-Skill-Engineering/windows-wpf/MetaSkillStudio.sln
```

| CWE | Count | Remediation Strategy |
|-----|-------|---------------------|
| CWE-78 | 3 | String→ArgumentList conversion |
| CWE-22 | 2 | Path validation |
| CWE-502 | 3 | JSON schema validation |
| CWE-338 | 1 | secrets module |
| CWE-94 | 1 | Input sanitization |
| CWE-377 | 1 | UUID-based temp files |
| CWE-400 | 1 | File size limits |
| CWE-312 | 1 | Secure memory wiping |
| CWE-362 | 1 | File locking |

### 3.2 Performance Optimization (15 hours)
**Agent Delegation:** 2 performance specialist agents

```yaml
Task: performance-optimization
Type: General agents
Profiling:
  - dotnet-trace collect --process-id <pid>
  - Identify hot paths
Changes:
  - Remove reflection from Converters (replace with direct casts)
  - Cache JsonSerializerOptions (static readonly)
  - Replace Contains() in loops with HashSet
  - Add ConfigureAwait(false) to service layer
  - Fix blocking WaitForExit
Verification:
  - Benchmark before/after
  - UI responsiveness tests
```

### 3.3 Test Infrastructure Completion (15 hours)
**Agent Delegation:** 3 general agents + 1 explore agent for coverage analysis

```yaml
Task: test-completion
Type: General agents
New Test Files:
  - ConfigurationStorageTests.cs
  - DialogServiceTests.cs
  - DispatcherServiceTests.cs
  - EnvironmentProviderTests.cs
  - RegexCacheTests.cs
  - TaskExtensionsTests.cs
Coverage Targets:
  - Services: 80%+
  - ViewModels: 90%+
  - Helpers: 85%+
```

### 3.4 Documentation & Standards (10 hours)
**Agent Delegation:** 2 general agents

```yaml
Task: documentation-completion
Type: General agents
Files to Create:
  - CONTRIBUTING.md (skill package contribution guidelines)
  - LICENSE (internal use license)
  - skill-orchestrator/references/conditional-branching-rules.md
  - skill-orchestrator/evals/behavior.jsonl
  - skill-orchestrator/evals/trigger-negative.jsonl
Fixes:
  - 14 skill heading levels (replace # with ##)
  - 2 skill section ordering fixes
  - 47 XML documentation gaps
```

---

## PHASE 4: FINAL VALIDATION & PRODUCTION READINESS
**Duration: 20-30 hours | Priority: P3**

### 4.1 Comprehensive Testing (10 hours)
**Agent Delegation:** 4 parallel general agents + CI integration

```yaml
Task: comprehensive-testing
Type: General agents (4 parallel)
Commands:
  - dotnet test --verbosity normal
  - dotnet test --collect:"XPlat Code Coverage"
  - ./scripts/pre-commit-check.sh
  - ./scripts/nightly-full-test.sh
  - ./scripts/regression-alert.sh --create-baseline
  - ./scripts/validate-skills.sh
Verification:
  - All tests pass
  - Code coverage > 80%
  - No new issues introduced
```

### 4.2 Accessibility Compliance (10 hours)
**Agent Delegation:** 2 general agents + accessibility specialist

```yaml
Task: accessibility-compliance
Type: General agents
Standards: WCAG 2.1 Level AA
Changes:
  - Add TabIndex to all interactive elements
  - Add skip navigation link to MainWindow
  - Add focus indicators to all control styles
  - Associate form labels with inputs (LabeledBy)
  - Verify color contrast ratios
  - Ensure 44px touch targets
Verification:
  - Keyboard-only navigation works
  - Screen reader testing
  - Color contrast analyzer pass
```

### 4.3 Localization & Resources (5 hours)
**Agent Delegation:** 2 general agents

```yaml
Task: localization-completion
Type: General agents
Changes:
  - Replace 110+ hardcoded strings with resource bindings
  - Add culture-aware date/number formatting
  - Add FlowDirection support for RTL languages
Verification:
  - All strings in Resources.resx
  - No hardcoded text in XAML or C#
  - Test with different cultures
```

### 4.4 Final Production Verification (5 hours)

```yaml
Task: production-readiness
Type: Explore agents + General agents
Checklist:
  - dotnet build (0 errors, 0 warnings)
  - dotnet test (all pass, >80% coverage)
  - Security scan (0 critical/high issues)
  - Performance benchmark (acceptable)
  - Accessibility audit (WCAG 2.1 AA)
  - Documentation complete (all sections)
  - Skill self-consistency (all skills follow teachings)
  - CI/CD pipelines green
  - Git hooks installed
  - No placeholder data
```

---

## AGENT DELEGATION STRATEGY

### General Agent Tasks (Use for Implementation)
| Task Type | Best For | Example Commands |
|-----------|----------|------------------|
| File edits | Code fixes, refactoring | `edit`, `write` |
| Build verification | Compilation checks | `dotnet build`, `dotnet test` |
| Tool execution | Static analysis | `bandit`, `shellcheck`, `yamllint` |
| Refactoring | Large-scale changes | `replaceAll`, multi-file edits |
| Documentation | SKILL.md fixes | Section reordering, heading fixes |

### Explore Agent Tasks (Use for Discovery)
| Task Type | Best For | Example Patterns |
|-----------|----------|----------------|
| Pattern discovery | Find similar issues across files | "Find all empty catch blocks" |
| Cross-reference analysis | Map dependencies | "Map all skill Next steps references" |
| Architecture analysis | System structure | "Identify all MVVM violations" |
| Gap identification | Missing pieces | "Find all TODOs and stubs" |
| Compliance checking | Standards verification | "Check all SKILL.md section ordering" |

### Sub-Agent Parallel Execution Map

```
Phase 1:
  ├─ Agent A: Fix ambiguous types (9 files)
  ├─ Agent B: Fix property renames (5 files)
  ├─ Agent C: Fix ScottPlot API (1 file)
  ├─ Agent D: Security patches (3 files)
  └─ Agent E: Infrastructure setup

Phase 2:
  ├─ Agent F: Extract SettingsViewModel
  ├─ Agent G: Extract RunDetailsViewModel
  ├─ Agent H: Extract PipelineViewModel
  ├─ Agent I: Extract BenchmarkViewModel
  └─ Agent J: Type consolidation

Phase 3:
  ├─ Agent K: Security hardening
  ├─ Agent L: Performance optimization
  ├─ Agent M: Test infrastructure
  └─ Agent N: Documentation

Phase 4:
  ├─ Agent O: Testing & validation
  └─ Agent P: Accessibility & localization
```

---

## MONITORING & REPORTING

### Daily Progress Tracking
```yaml
Metrics:
  - Issues resolved (P0/P1/P2/P3)
  - Test coverage percentage
  - Build status (errors/warnings)
  - Security scan results
  - Performance benchmarks
Reports:
  - Daily agent task completion
  - Blocked items (with reasons)
  - New issues discovered
  - Time tracking vs estimates
```

### Quality Gates
| Gate | Criteria | Verification |
|------|----------|------------|
| **Build Gate** | 0 compilation errors | `dotnet build` |
| **Test Gate** | >80% coverage, all tests pass | `dotnet test` |
| **Security Gate** | 0 critical/high CWEs | bandit + semgrep |
| **Performance Gate** | <100ms UI response | Benchmarks |
| **Accessibility Gate** | WCAG 2.1 AA | Manual + automated |
| **Documentation Gate** | All sections complete | Markdown validation |

---

## ESTIMATED TIMELINE

| Phase | Duration | Cumulative | Deliverable |
|-------|----------|------------|-------------|
| Phase 0: Setup | 4-6 hrs | 6 hrs | Tools installed, environment ready |
| Phase 1: Foundation | 40-60 hrs | 66 hrs | Build passes, security patched |
| Phase 2: Architecture | 60-90 hrs | 156 hrs | MVVM compliant, DI complete |
| Phase 3: Quality | 50-70 hrs | 226 hrs | Tests passing, secure, fast |
| Phase 4: Validation | 20-30 hrs | 256 hrs | Production ready |
| **TOTAL** | **174-256 hrs** | | **4-6 weeks (1 FTE)** |

---

## RISK MITIGATION

| Risk | Mitigation | Owner |
|------|------------|-------|
| Build breaks during refactor | Fix in small, testable chunks | All agents |
| Security patches cause regressions | Add integration tests | Security agents |
| ScottPlot API changes breaking | Pin to specific version | General agents |
| Missing skill directories breaking flow | Create stubs first | General agents |
| Time overrun | Prioritize P0/P1, defer P3 | Project lead |

---

## SUCCESS CRITERIA

✅ **Production Ready When:**
1. `dotnet build` produces 0 errors, 0 warnings
2. All 174 documented issues resolved or accepted
3. Test coverage > 80%
4. Security scan: 0 critical/high issues
5. Performance: UI responsive (<100ms)
6. Accessibility: WCAG 2.1 AA compliant
7. All 17 skills have complete evals/ directories
8. Self-consistency: All skills follow what they teach
9. CI/CD pipelines green
10. Documentation complete (AGENTS.md, README.md, CONTRIBUTING.md)

---

## NEXT STEPS

**User Decision Required:**

1. **Approve Phase 0** (tool installation) - 4-6 hours
2. **Prioritize issues** - Should we focus on build-first or security-first?
3. **Resource allocation** - 1 FTE for 4-6 weeks, or parallel team?
4. **Acceptance criteria** - Are all 174 issues required, or can some be deferred?
5. **Start immediately with** - Phase 1.1 (build fixes) or Phase 1.2 (security)?

**Recommended Immediate Actions:**
1. Install shellcheck, yamllint, dotnet-format (Phase 0)
2. Begin 5-agent parallel build fix (Phase 1.1)
3. Patch command injection vulnerability (Phase 1.2)
4. Create missing skill directories (Phase 1.3)
