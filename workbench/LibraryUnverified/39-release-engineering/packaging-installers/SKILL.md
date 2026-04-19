---
name: packaging-installers
description: >-
  Package CLI tools for Homebrew, APT (deb), and RPM distribution. Use when
  creating a Homebrew formula, building .deb packages, writing RPM spec files,
  or automating package builds with GoReleaser nfpms. Do not use for container
  images (prefer Docker skills) or language registries (npm, PyPI, crates.io).
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: packaging-installers
  maturity: draft
  risk: low
  tags: [packaging, homebrew, apt, rpm]
---

# Purpose

Package CLI tools and binaries for distribution via Homebrew, APT (.deb), and RPM (.rpm).

# When to use this skill

- creating a Homebrew formula or tap for a CLI tool
- building a `.deb` package for Ubuntu/Debian
- writing an RPM `.spec` file for Fedora/RHEL
- automating package builds in CI with GoReleaser nfpms

# Do not use this skill when

- publishing to a language registry (npm, PyPI, crates.io) — different workflows
- building Docker/OCI container images — different domain
- cross-compiling binaries without packaging — prefer `release-binaries`

# Procedure

1. **Choose target formats** — Homebrew for macOS/Linux dev tools, `.deb` for Ubuntu servers, `.rpm` for RHEL/Fedora.
2. **Write Homebrew formula** — define `class MyTool < Formula` with `url`, `sha256`, `depends_on`, and `install` method.
3. **Build .deb** — create directory structure: `DEBIAN/control`, `usr/local/bin/mytool`. Run `dpkg-deb --build`.
4. **Write RPM spec** — define `Name`, `Version`, `Release`, `%install`, `%files`. Build with `rpmbuild -ba`.
5. **Automate with GoReleaser** — add `nfpms:` section to `.goreleaser.yaml` for deb/rpm; add `brews:` for Homebrew tap.
6. **Set up Homebrew tap** — create `homebrew-tap` repo with `Formula/mytool.rb`. Update SHA on each release.
7. **Test installation** — install from local package, verify binary runs, check completions are placed correctly.

# Homebrew formula

```ruby
class MyTool < Formula
  desc "Short description of my tool"
  homepage "https://github.com/org/mytool"
  url "https://github.com/org/mytool/releases/download/v1.2.0/mytool-1.2.0.tar.gz"
  sha256 "abc123..."
  license "MIT"
  depends_on "go" => :build

  def install
    system "go", "build", *std_go_args(ldflags: "-s -w")
    bash_completion.install "completions/mytool.bash"
  end

  test do
    assert_match version.to_s, shell_output("#{bin}/mytool --version")
  end
end
```

# Debian control file

```
Package: mytool
Version: 1.2.0
Architecture: amd64
Maintainer: You <you@example.com>
Depends: libc6 (>= 2.31)
Section: utils
Priority: optional
Description: Short description of my tool
```

# Decision rules

- Always include a `test` block in Homebrew formulas — `brew audit` requires it.
- Pin `sha256` for every download URL — never use `head` for stable releases.
- In `.deb`, set `Depends` for shared libraries; use `ldd` to discover them.
- Use GoReleaser `nfpms` to auto-generate deb/rpm from a single config.
- Include shell completions and man pages in every package.

# References

- https://docs.brew.sh/Formula-Cookbook
- https://www.debian.org/doc/manuals/maint-guide/
- https://goreleaser.com/customization/nfpm/

# Related skills

- `release-binaries` — cross-compiling before packaging
- `cli-development-go` — building the Go binary
- `linux-ubuntu-ops` — installing packages on target systems
