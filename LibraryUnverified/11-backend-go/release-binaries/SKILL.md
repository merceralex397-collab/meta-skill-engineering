---
name: release-binaries
description: >-
  Cross-compile Go/Rust binaries and automate releases with GoReleaser or
  cargo-dist. Use when setting up cross-compilation, configuring GoReleaser,
  stripping binaries, or publishing artifacts with checksums. Do not use for
  Homebrew/deb/rpm packaging (prefer packaging-installers) or CI pipeline
  setup beyond release jobs.
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: release-binaries
  maturity: draft
  risk: low
  tags: [release, binaries, cross-compile]
---

# Purpose

Cross-compile binaries for multiple OS/arch targets and automate release publishing with GoReleaser or cargo-dist.

# When to use this skill

- setting up cross-compilation for Go (`GOOS`/`GOARCH`) or Rust (`--target`)
- configuring GoReleaser for automated GitHub releases
- stripping symbols, compressing binaries, or embedding version info
- publishing release artifacts with checksums and signatures

# Do not use this skill when

- packaging into Homebrew/deb/rpm — prefer `packaging-installers`
- building Docker images — different workflow
- the project is a library, not a binary

# Procedure

1. **Define target matrix** — `linux/amd64`, `linux/arm64`, `darwin/amd64`, `darwin/arm64`, `windows/amd64`.
2. **Configure GoReleaser** — create `.goreleaser.yaml` with `builds`, `archives`, `checksum`, `release`.
3. **Set ldflags** — `-ldflags "-s -w -X main.version={{.Version}} -X main.commit={{.Commit}}"`.
4. **Local build** — `goreleaser build --snapshot --clean`. Check output in `dist/`.
5. **CI workflow** — GitHub Actions on `tags: v*` using `goreleaser/goreleaser-action@v5`.
6. **Checksums** — GoReleaser auto-generates. Manual: `sha256sum dist/* > checksums.txt`.
7. **Sign** — `cosign sign-blob` or GPG on checksums file.
8. **Verify** — download each artifact, check checksum, run `--version`.

# GoReleaser config

```yaml
version: 2
builds:
  - env: [CGO_ENABLED=0]
    goos: [linux, darwin, windows]
    goarch: [amd64, arm64]
    ldflags: [-s, -w, "-X main.version={{.Version}}"]
archives:
  - format_overrides:
      - goos: windows
        format: zip
checksum:
  name_template: checksums.txt
```

# Rust cross-compilation

```bash
rustup target add x86_64-unknown-linux-musl aarch64-unknown-linux-musl
cargo build --release --target x86_64-unknown-linux-musl
strip target/x86_64-unknown-linux-musl/release/mytool
```

# Decision rules

- `CGO_ENABLED=0` for Go static binaries — avoids glibc issues.
- musl target for Rust static binaries on Linux.
- Always include `sha256` checksums.
- `-s -w` (Go) or `strip` (Rust) reduces binary size ~30%.
- Tag as `v1.2.3` (semver with `v` prefix) — GoReleaser expects this.

# References

- https://goreleaser.com/
- https://doc.rust-lang.org/cargo/reference/config.html

# Related skills

- `packaging-installers` — Homebrew/deb/rpm packaging
- `cli-development-go` — building the Go CLI
- `cobra-go` — command structure for the binary
