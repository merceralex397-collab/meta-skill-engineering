# Changelog

All notable changes to the Meta Skill Engineering project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- **Headless CLI contract** — expanded Meta Skill Studio action surface for evaluation, governance, distribution, orchestration, and introspection workflows
- **Structured Studio run artifacts** — versioned `meta-skill-studio-run` JSON output with summaries, measurements, and optional comparison/improvement sections
- **Authoritative platform docs** — CLI feature inventory, action contract, plugin-eval disposition, and surface-authority references under `docs/`
- **Complete Studio UI overhaul** — 3-panel layout: navigation rail (left), content area (center), AI assistant (right, collapsible)
- **10 dedicated views**: Dashboard, Skill Library, Create Skill, Improve Skill, Test & Evaluate, Automation, Import Skills, Library Management, Analytics, Settings
- **Library browser** with 3-tier system: Unverified → In Testing → Verified, with search, promote/demote
- **Automation view** — continuous improvement loop with quality threshold and max iterations
- **Import view** — import skills from local folders
- **Analytics view** — library health metrics, provider/model inventory, runtime stats, and run history
- Chat-style AI assistant with conversation history, model picker, prompt-file handoff, clear conversation, and new conversation controls
- Workflow cards with icons for quick actions on Dashboard
- Status bar showing runtime status, skill count, and active model
- `Library/` directory created as the verified tier
- `windows-wpf\smoke-test-publish.ps1` to launch-validate the published exe and surface crash details from the Windows Application log
- Discovery Pipeline documented (community-skill-harvester → evaluation → install)
- All 17 skills mirrored in `.opencode/skills/`
- CONTRIBUTING.md populated with guidelines

### Changed
- **CLI hardening**: runtime-free actions no longer require OpenCode role setup, missing parameters now fail cleanly, and pipeline actions now surface through the Studio CLI
- **Repo positioning**: root docs now describe Meta-Skill-Engineering as an automation-capable platform with the Python CLI as the authoritative surface
- **Layout restructured**: nav rail left, content center, assistant right (VS Code-style)
- **Top-level shell hardened**: Studio/Help menu added, category browser restored, promote/demote/move now route through backend commands, and automation evaluates actual testing-tier skills
- **Terminology cleaned**: all user-facing "OpenCode" → "AI Runtime/Assistant", raw folder names → friendly display names
- **Creation Pipeline corrected** — `community-skill-harvester` now documented as the entry point feeding into `skill-creator`
- Skill names displayed as friendly titles (e.g., "Skill Creator" not "skill-creator")
- Library count uses leaf-directory counting for accurate skill totals
- Settings dialog rebranded from "OpenCode Configuration" to "AI Runtime Configuration"
- Resource strings cleaned: "LibraryUnverified" → "Unverified", "LibraryWorkbench" → "In Testing"
- `windows-wpf\build-release.ps1` now runs the publish smoke test by default
- Stale files archived/removed: DEEP_INVESTIGATION_PLAN.md, active-issues.md, empty directories
- Stale remote branches deleted: remediation-complete, task5/subtask-f2

### Fixed
- Pipeline definitions in AGENTS.md and README.md corrected (community-skill-harvester leads creation pipeline)
- docs/workflow.md: absolute Linux path replaced with relative path
- All views properly wired to ViewModel commands
- GitHub skill import now clones and validates remote skill packages instead of saving a placeholder run
- Published app startup regressions caused by duplicate `SectionHeader` resources and a missing `StudioTextBox` resource
- Zone.Identifier and .blender-mcp already covered in .gitignore

## [1.0.0] - 2026-04-14

### Added
- Initial release of Meta Skill Engineering framework
- 17 core skills for skill lifecycle management:
  - skill-creator: Create new skills from scratch
  - skill-improver: Improve existing skills
  - skill-evaluation: Evaluate routing and output quality
  - skill-testing-harness: Build test infrastructure
  - skill-anti-patterns: Scan for structural issues
  - skill-trigger-optimization: Fix routing descriptions
  - skill-benchmarking: Compare skill variants
  - skill-safety-review: Audit for safety hazards
  - skill-provenance: Record origin and trust metadata
  - skill-packaging: Bundle skills with manifests
  - skill-installer: Install skills to agent
  - skill-lifecycle-management: Manage skill states
  - skill-orchestrator: Coordinate multi-skill workflows
  - skill-catalog-curation: Audit library organization
  - skill-adaptation: Port skills to new environments
  - skill-variant-splitting: Split broad skills
  - community-skill-harvester: Find external skills
- Three standardized pipelines:
  - Creation Pipeline (9 phases)
  - Improvement Pipeline (4 phases)
  - Library Management Pipeline (2 phases)
- Library organization:
  - LibraryUnverified: Raw, untested skills
  - LibraryWorkbench: Skills under active testing
- Python-based Meta Skill Studio CLI/TUI
- Task tracking system in `tasks/`

### Infrastructure
- Git-based version control
- JSONL-based eval artifact format
- SQLite analytics database
- Automated skill validation framework

[Unreleased]: https://github.com/merceralex397-collab/Meta-Skill-Engineering/compare/v1.0.0...HEAD
[1.0.0]: https://github.com/merceralex397-collab/Meta-Skill-Engineering/releases/tag/v1.0.0
