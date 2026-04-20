# Changelog

All notable changes to the Meta Skill Engineering project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Complete UI overhaul — modern card-based dashboard with collapsible AI Assistant sidebar
- Chat-style AI assistant with conversation history (replaces form-based prompt)
- Workflow cards with icons and descriptions for all 6 core operations
- Status cards showing runtime, core skills, library count, and run history at a glance
- New "Find External Skills" workflow (community-skill-harvester integration)
- Discovery Pipeline documented (community-skill-harvester → evaluation → install)
- All 17 skills mirrored in `.opencode/skills/` (was 12)
- CONTRIBUTING.md populated with guidelines

### Changed
- User-facing terminology cleaned: "OpenCode" → "AI Runtime/Assistant", "repo-owned packages" → "Core Skills", "LibraryUnverified" → "Skill Library"
- Settings dialog rebranded from "OpenCode Configuration" to "AI Runtime Configuration"
- Skill names displayed as friendly titles (e.g., "Skill Creator" not "skill-creator")
- Library count uses leaf-directory counting for more accurate skill totals
- "OpenCode mirror" surface removed from UI entirely
- Creation Pipeline corrected — `skill-creator` is the entry point, not `community-skill-harvester`
- Stale files archived to `docs/historical/` (DEEP_INVESTIGATION_PLAN.md, active-issues.md, tasks/, "skill creator/")
- Removed empty LibraryUnverified category directories and `.blender-mcp/`

### Fixed
- Pipeline definitions in AGENTS.md and README.md corrected
- All 12+ commands properly bound in new layout

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
