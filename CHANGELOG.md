# Changelog

All notable changes to the Meta Skill Engineering project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- WPF-based Meta Skill Studio GUI with modern Windows UI
- Shared resources (Resources.resx) for localization support
- Standardized eval artifact schema documentation
- Pipeline definitions reference documentation
- CHANGELOG template for tracking changes

### Changed
- Consolidated pipeline definitions into `references/pipeline-definitions.md`
- Updated skill-orchestrator to reference external pipeline definitions

### Fixed
- Namespace consistency across all WPF project files
- Added missing resource files to project configuration

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
