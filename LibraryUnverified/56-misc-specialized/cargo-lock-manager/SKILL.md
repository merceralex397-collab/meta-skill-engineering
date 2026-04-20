---
name: cargo-lock-manager
description: "Manage Rust Cargo.lock files, dependency auditing, version pinning, and supply-chain security. Trigger: 'audit Cargo dependencies', 'update Cargo.lock', 'check for vulnerable crates', 'manage Rust dependencies', 'cargo deny', 'cargo audit', 'dependency tree analysis', 'Cargo workspace dependencies'. Do NOT use for writing Rust application code, designing Rust APIs, or general Cargo.toml project configuration unrelated to dependency management."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: cargo-lock-manager
  maturity: draft
  risk: low
  tags: [rust, cargo, dependencies, security, audit, semver, lockfile, supply-chain]
---

# Purpose

Provide a repeatable procedure for managing Rust project dependencies — from Cargo.lock commit discipline and version pinning through security auditing, license compliance, and workspace dependency management. This skill produces auditable, actionable dependency management plans.

# When to use this skill

Use this skill when:

- a Cargo.lock file needs to be updated, audited, or its commit policy needs to be decided
- `cargo audit` has reported vulnerabilities and a triage/remediation plan is needed
- dependency versions in Cargo.toml need review (pinning strategy, SemVer range selection)
- a Cargo workspace needs shared dependency management or version inheritance
- license compliance must be checked across the dependency tree with `cargo deny`
- compile times are slow and dependency reduction or feature-flag trimming could help
- a new crate dependency is being evaluated for inclusion

# Do not use this skill when

- the task is writing Rust application logic, traits, or type design — that's general Rust development
- the work is about Cargo.toml project metadata (edition, name, authors) unrelated to dependencies
- the task involves a different language's package manager (npm, pip, go mod)
- the user needs help with Rust build targets, cross-compilation, or linker configuration

# Operating procedure

## Step 1 — Determine Cargo.lock commit policy

The commit policy depends on the crate type:

| Crate type | Commit Cargo.lock? | Reason |
|---|---|---|
| Binary (application, CLI tool, server) | **Always** | Ensures reproducible builds — every `cargo build` resolves to identical dependency versions. |
| Library (published to crates.io) | **No** (or optional) | Downstream consumers generate their own lockfile; committing yours adds noise and merge conflicts without reproducibility benefit. |
| Workspace with mixed binary + library | **Yes** | The workspace lockfile is shared; binary reproducibility takes priority. |

If `.gitignore` excludes `Cargo.lock` for a binary crate, fix it immediately.

## Step 2 — Audit for known vulnerabilities

Run the security audit pipeline:

```bash
# Install tools if missing
cargo install cargo-audit cargo-deny

# Check RUSTSEC advisory database
cargo audit --json 2>/dev/null | jq '.vulnerabilities.list[] | {id: .advisory.id, crate: .advisory.package, title: .advisory.title, severity: .advisory.cvss}'

# Human-readable summary
cargo audit
```

For each reported vulnerability, triage using this priority matrix:

| Severity | Direct dependency? | Action | Timeline |
|---|---|---|---|
| Critical/High | Yes | Update or patch immediately | Same day |
| Critical/High | Transitive | Update parent crate or pin patched version | Within 48 hours |
| Medium | Yes | Schedule update in next dependency maintenance window | Within 1 week |
| Medium | Transitive | Monitor; update if parent crate releases fix | Within 2 weeks |
| Low/Informational | Any | Document and track; update opportunistically | Next maintenance cycle |

If no patched version exists:
1. Check if the vulnerability is exploitable in your usage context.
2. If exploitable, evaluate alternative crates (`cargo crev` for community reviews).
3. If not exploitable, document the risk acceptance with a `cargo audit` ignore entry and a comment explaining why.

## Step 3 — License compliance check

```bash
# Configure cargo-deny (create deny.toml if absent)
cargo deny init

# Check license compliance
cargo deny check licenses

# Check for banned crates, duplicate versions, and advisories in one pass
cargo deny check
```

In `deny.toml`, define explicit allow/deny lists:

```toml
[licenses]
allow = ["MIT", "Apache-2.0", "BSD-2-Clause", "BSD-3-Clause", "ISC", "Zlib"]
deny = ["GPL-3.0", "AGPL-3.0"]  # Adjust per project licensing requirements
confidence-threshold = 0.8
```

## Step 4 — Analyze dependency tree

```bash
# Full dependency tree
cargo tree

# Find why a specific crate is included
cargo tree -i <crate_name> --depth 1

# Find duplicate versions of the same crate
cargo tree --duplicates

# Count total dependencies
cargo tree --depth 999 --prefix none | sort -u | wc -l
```

Use the tree analysis to identify:
- **Duplicate crates**: different versions of the same crate pulled by different parents → try to unify by updating parents.
- **Heavy transitive trees**: a single dependency pulling in 50+ transitive deps → evaluate if the functionality justifies the cost.
- **Feature-flag bloat**: dependencies pulling in features you don't use → disable default features and enable only what's needed.

## Step 5 — Version pinning strategy in Cargo.toml

Follow these SemVer range conventions:

| Syntax | Meaning | When to use |
|---|---|---|
| `^1.2.3` (default) | ≥1.2.3, <2.0.0 | Most dependencies — allows compatible updates |
| `~1.2.3` | ≥1.2.3, <1.3.0 | When patch updates are safe but minor versions have broken you before |
| `=1.2.3` | Exactly 1.2.3 | Only for known-fragile crates or when reproducing a specific bug |
| `>=1.2, <1.5` | Custom range | When you need features from 1.2+ but 1.5 introduced a breaking change |

**Never use `*` (wildcard)** — it accepts any version including breaking changes.

## Step 6 — Perform targeted updates

```bash
# Update a single crate (and its dependencies)
cargo update -p <crate_name>

# Update a single crate to a specific version
cargo update -p <crate_name> --precise <version>

# Update all dependencies within SemVer-compatible ranges
cargo update

# Check for outdated dependencies (install cargo-outdated first)
cargo outdated --root-deps-only
```

After every update:
1. Run the full test suite: `cargo test --workspace`
2. Run clippy: `cargo clippy --workspace -- -D warnings`
3. Re-run `cargo audit` to confirm no new vulnerabilities were introduced
4. Review the diff to `Cargo.lock` before committing

## Step 7 — Workspace dependency management

For multi-crate workspaces, centralize dependency versions:

```toml
# Workspace root Cargo.toml
[workspace.dependencies]
serde = { version = "1.0", features = ["derive"] }
tokio = { version = "1", features = ["full"] }
anyhow = "1.0"

# Member crate Cargo.toml
[dependencies]
serde = { workspace = true }
tokio = { workspace = true }
```

Benefits:
- Single source of truth for dependency versions across all workspace members.
- `cargo update` in the workspace root updates all members consistently.
- Version inheritance via `workspace = true` eliminates version drift between crates.

## Step 8 — Reduce compile times through dependency hygiene

```bash
# Measure build times per crate
cargo build --timings

# Identify heaviest compile-time dependencies
cargo build --timings 2>&1 | head -20
```

Optimization strategies:
- **Disable default features**: `serde = { version = "1.0", default-features = false, features = ["derive"] }`
- **Use `cfg` for dev-only deps**: put heavy test/bench dependencies in `[dev-dependencies]` only.
- **Replace heavy crates**: e.g., `ureq` instead of `reqwest` if you don't need async HTTP; `fastrand` instead of `rand` if you don't need cryptographic randomness.
- **Feature-gate optional functionality**: use Cargo features to make expensive dependencies opt-in.

# Decision rules

- **Binary crates always commit Cargo.lock** — no exceptions. Reproducible builds are a correctness requirement, not a preference.
- **Critical/High vulnerabilities in direct dependencies are blockers** — do not merge PRs or ship releases until remediated or explicitly risk-accepted with documentation.
- **Wildcard versions are never acceptable** — `*` in Cargo.toml is a reject-on-sight finding.
- **Prefer fewer dependencies over more** — before adding a new crate, check if the functionality can be implemented in <50 lines of code. If so, vendor it.
- **Unified dependency versions over duplicates** — if `cargo tree --duplicates` shows multiple versions of the same crate, make unification a priority to reduce compile time and binary size.
- **Feature flags should be minimal** — enable only the features you actually use. `default-features = false` should be the starting position for every dependency.
- **Update frequency**: security audits weekly (automate in CI), general dependency updates monthly, major version upgrades quarterly with dedicated testing.

# Output requirements

Produce a structured deliverable with these sections:

1. **Dependency Audit Report** — list of all vulnerabilities found, severity, affected crate and version, remediation action (update/patch/replace/accept risk), timeline.
2. **Update Plan** — ordered list of dependency updates to perform, expected SemVer impact, test verification steps, rollback procedure if tests fail.
3. **License Compatibility Matrix** — table of all direct dependencies with their licenses, compatibility status (allowed/denied/review needed), and any copyleft concerns.
4. **Build Time Impact** — current build time baseline, identified heavy dependencies, proposed optimizations with estimated time savings.
5. **Cargo.lock Status** — commit policy confirmation, current lockfile freshness, any stale or yanked versions detected.

# Anti-patterns

- **Wildcard versions (`*`) in Cargo.toml**: accepts any version including breaking changes; defeats the entire purpose of SemVer.
- **Ignoring `cargo audit` warnings**: known vulnerabilities left unaddressed accumulate risk silently; at minimum, document risk acceptance.
- **Not committing Cargo.lock for binary crates**: makes builds non-reproducible; different developers and CI get different dependency versions.
- **Yanked dependency ignorance**: running `cargo update` may fail if a depended-upon version was yanked; monitor and update proactively.
- **Blanket `cargo update` without testing**: updating all dependencies at once makes it impossible to bisect which update introduced a regression. Update incrementally.
- **Feature-flag maximalism**: enabling all features of a crate "just in case" bloats compile time and binary size for functionality you never call.
- **Vendoring without a policy**: copying crate source into the repo without a clear update/audit process creates unmaintained forks that drift from upstream.
- **Duplicate crate versions left unchecked**: two versions of `syn` or `tokio` in the dependency tree doubles compile time for those crates with no benefit.

# Related skills

- `solidjs-patterns` — SolidJS frontend patterns (Rust backend + SolidJS frontend is a common stack)
- `tauri-solidjs` — Tauri desktop apps (Rust backend with Cargo dependency management)
- `linear-address-issue` — tracking dependency update work items in Linear

# Failure handling

- If `cargo audit` fails to run (database fetch error), ensure network access and try `cargo audit fetch` to update the advisory database manually.
- If a vulnerability has no patched version and no alternative crate exists, document the risk in a `deny.toml` exception with an `[advisories.ignore]` entry and a clear comment explaining the business justification.
- If `cargo update` causes test failures, revert to the previous Cargo.lock and update dependencies one at a time to isolate the breaking change.
- If workspace dependency unification is impossible due to conflicting version requirements between member crates, document the conflict and track upstream resolution.
