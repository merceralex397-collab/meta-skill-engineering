---
name: tauri-solidjs
description: "Desktop application specialist for the Tauri + SolidJS stack — project scaffolding, IPC design, security configuration, build/distribution, and platform integration. Triggers: 'Tauri app', 'SolidJS desktop', 'Tauri invoke', 'tauri.conf.json', 'src-tauri', 'desktop app with SolidJS', 'Tauri commands', 'Tauri allowlist', 'Tauri v2 capabilities'. Do NOT use for web-only SolidJS apps (no Tauri), Electron apps, React Native/Flutter mobile apps, or pure Rust CLI tools without a frontend."
license: Apache-2.0
compatibility:
  clients: [openai-codex, gemini-cli, opencode, github-copilot]
metadata:
  owner: codex
  domain: tauri-solidjs
  maturity: draft
  risk: low
  tags: [tauri, solidjs, desktop-app, rust, typescript, cross-platform]
---

# Purpose

Domain skill for building cross-platform desktop applications using the Tauri framework (Rust backend) with a SolidJS frontend. Covers project structure, IPC contract design, security model configuration, state management across the Rust–JS boundary, build pipelines, and platform-specific distribution (macOS, Windows, Linux).

# When to use this skill

Use this skill when:

- scaffolding a new Tauri + SolidJS desktop application
- designing or implementing IPC contracts between SolidJS frontend and Rust backend (commands, events)
- configuring Tauri security: allowlists (v1), capabilities/permissions (v2), CSP headers
- building and distributing the app: platform bundles, auto-updater, code signing
- implementing desktop-specific features: system tray, file drag-and-drop, native menus, splash screens, deep linking, multi-window management
- managing state synchronization between SolidJS stores and Tauri's managed state
- troubleshooting Tauri build issues, WebView quirks, or platform-specific behavior

# Do not use this skill when

- the app is a **web-only SolidJS** project with no desktop shell — use a SolidJS/web skill instead
- the desktop framework is **Electron**, **Wails**, or **Neutralinojs** — different architecture and APIs
- the task is a **mobile app** (React Native, Flutter, Capacitor) — even if it uses a webview
- the backend is **pure Rust CLI** with no frontend — use a Rust skill instead
- a quick one-off task unrelated to the Tauri+SolidJS stack — use `misc-helper`

# Operating procedure

## 1. Assess project state

- Check for existing project structure: does `src-tauri/` exist? Is there a `tauri.conf.json` (v1) or `tauri.conf.json` with `capabilities` (v2)?
- Identify Tauri version: v1 (`@tauri-apps/api` ^1.x, `tauri` crate ^1.x) vs v2 (`@tauri-apps/api` ^2.x, `tauri` crate ^2.x). The security model, plugin system, and IPC patterns differ significantly.
- Check frontend setup: SolidJS (`solid-js`), bundler (`vite` with `vite-plugin-solid`), router (`@solidjs/router`).

## 2. Project structure (scaffold or validate)

```
my-app/
├── src/                          # SolidJS frontend
│   ├── index.tsx                 # Entry point, render(<App />)
│   ├── App.tsx                   # Root component with router
│   ├── routes/                   # Page components
│   ├── components/               # Shared UI components
│   ├── lib/
│   │   ├── tauri.ts              # Typed wrappers around invoke() calls
│   │   └── store.ts              # SolidJS stores for app state
│   └── index.html                # HTML shell (loaded by WebView)
├── src-tauri/
│   ├── Cargo.toml                # Rust dependencies
│   ├── tauri.conf.json           # Tauri configuration (window, security, bundle)
│   ├── capabilities/             # (v2) Permission/capability definitions
│   ├── src/
│   │   ├── main.rs               # Tauri builder setup
│   │   ├── commands/             # #[tauri::command] handlers
│   │   ├── state.rs              # Managed state (app_handle.manage())
│   │   └── lib.rs                # Module declarations
│   └── icons/                    # App icons for all platforms
├── vite.config.ts                # Vite + solid plugin + Tauri dev server config
└── package.json
```

## 3. IPC contract design

**Tauri Commands (frontend → backend):**
- Define commands in Rust with `#[tauri::command]`:
  ```rust
  #[tauri::command]
  async fn read_file(path: String) -> Result<String, String> {
      std::fs::read_to_string(&path).map_err(|e| e.to_string())
  }
  ```
- Register in the builder: `.invoke_handler(tauri::generate_handler![read_file])`
- Call from SolidJS: `const content = await invoke<string>('read_file', { path: '/tmp/data.txt' })`
- Always create typed wrappers in `src/lib/tauri.ts` to centralize invoke calls and provide TypeScript types.

**Event System (bidirectional):**
- Backend → Frontend: `app_handle.emit_all("event-name", payload)` or window-scoped `window.emit("event-name", payload)`
- Frontend → Backend: `emit('event-name', payload)` with `app.listen_global("event-name", handler)` in Rust
- Frontend listening: `listen<PayloadType>('event-name', (event) => { ... })` — always unlisten on component cleanup.

## 4. Security configuration

**Tauri v1 (allowlist):**
- In `tauri.conf.json` → `tauri.allowlist`, enable only the APIs the app needs:
  ```json
  { "fs": { "scope": ["$APPDATA/*"], "readFile": true, "writeFile": true },
    "dialog": { "open": true, "save": true },
    "shell": { "open": true } }
  ```
- Set restrictive CSP in `tauri.conf.json` → `tauri.security.csp`:
  `"default-src 'self'; script-src 'self'; style-src 'self' 'unsafe-inline'"`

**Tauri v2 (capabilities):**
- Define capabilities in `src-tauri/capabilities/*.json`:
  ```json
  { "identifier": "main-window", "windows": ["main"],
    "permissions": ["fs:read", "fs:write", "dialog:open"] }
  ```
- Use permission scoping to restrict file system access to specific directories.
- Never grant `fs:*` or `shell:*` — always scope to the minimum required.

## 5. SolidJS desktop-specific patterns

- **Routing**: use `HashRouter` from `@solidjs/router` (not `Router`) — there is no server to handle history-mode fallback in a desktop WebView.
- **File system access**: always go through Tauri's `fs` API or custom commands. Never use browser `fetch('file://...')`.
- **Native menus**: define in `tauri.conf.json` → `tauri.menu` or programmatically in Rust. Connect menu events to frontend via the event system.
- **State management**: use SolidJS `createStore`/`createSignal` for UI state. For state that must persist or be shared with Rust, sync through Tauri commands or the event system. Avoid duplicating state across boundaries.

## 6. Build and distribution

- **Development**: `npm run tauri dev` — starts Vite dev server + Tauri with hot reload.
- **Production build**: `npm run tauri build` — produces platform-specific bundles:
  - macOS: `.dmg` and `.app` bundle
  - Windows: `.msi` and `.exe` (NSIS installer)
  - Linux: `.deb`, `.AppImage`, `.rpm`
- **Code signing**:
  - macOS: set `APPLE_CERTIFICATE`, `APPLE_CERTIFICATE_PASSWORD`, `APPLE_ID`, `APPLE_PASSWORD` env vars for notarization.
  - Windows: set `TAURI_SIGNING_PRIVATE_KEY` for Authenticode signing.
- **Auto-updater**: configure `tauri.conf.json` → `tauri.updater` with endpoint URL, public key. The updater checks for new versions on app launch or on a timer.

## 7. Validate and deliver

- Test IPC contract: verify all commands are registered and callable from the frontend.
- Test on target platforms: WebView rendering can differ between macOS (WebKit), Windows (WebView2/Edge), and Linux (WebKitGTK).
- Verify bundle size: Tauri apps should be 5–15 MB. If significantly larger, check for unnecessary Rust dependencies or unoptimized assets.

# Decision rules

- **Tauri v2 over v1**: for new projects, always use Tauri v2 unless the team has a specific v1 dependency. v2 has a better security model, mobile support, and plugin architecture.
- **HashRouter always**: never use history-mode routing in a Tauri WebView — it will break on refresh and deep links.
- **Commands over events for request/response**: use `invoke()` when the frontend needs a response. Use events only for push notifications (backend → frontend) or fire-and-forget.
- **Async commands by default**: Rust commands should be `async` to avoid blocking the main thread. Only use sync commands for trivial operations (<1ms).
- **Typed IPC wrappers**: never call `invoke()` with raw string command names scattered throughout the frontend. Centralize in a typed module.
- **Minimal permissions**: start with zero permissions and add only what the app needs. Review permissions on every feature addition.
- **Vite over other bundlers**: Tauri's tooling is optimized for Vite. Don't use Webpack or Rollup unless forced by a dependency.

# Output requirements

Every response must include the applicable sections:

1. **`Project Scaffold`** — directory structure with file purposes annotated, if scaffolding a new project or adding a major feature.
2. **`Configuration Files`** — complete `tauri.conf.json` snippets (or capability definitions for v2) with security settings annotated.
3. **`IPC Contract`** — Rust command definitions paired with their TypeScript invoke wrappers. Include types for both sides.
4. **`Build Configuration`** — build commands, signing setup, and updater configuration if distribution is involved.
5. **`Platform Notes`** (if relevant) — any platform-specific behavior, workarounds, or testing instructions.

# Anti-patterns

- **Exposing all Tauri APIs**: granting `"all": true` in the allowlist (v1) or `"*"` permissions (v2). This defeats Tauri's security model and gives the WebView access to the full file system, shell, and network.
- **Blocking the main thread**: synchronous, long-running Rust commands freeze the entire UI. Always use `async` commands and spawn blocking work with `tauri::async_runtime::spawn_blocking`.
- **Bundling unnecessary permissions**: requesting `shell:execute` when the app only needs `shell:open` (open URLs in default browser). Review each permission's actual scope.
- **Raw invoke calls**: calling `invoke('my_command', { arg: val })` directly in components instead of through typed wrappers. This leads to typo bugs and makes refactoring IPC contracts painful.
- **History-mode router**: using `Router` instead of `HashRouter`. The app will break on refresh because there's no server to handle the fallback.
- **Duplicated state**: keeping the same data in both a SolidJS store and Tauri managed state without a sync mechanism. Pick one source of truth and derive the other.
- **Ignoring WebView differences**: assuming Chrome DevTools behavior in production. Test on all target platforms — WebKit (macOS), WebView2 (Windows), and WebKitGTK (Linux) have different feature support.

# Related skills

- `fastapi-patterns` — for building backend APIs that a Tauri app might connect to
- `bigquery-skill` — for analytics dashboards built as Tauri desktop apps
- `misc-helper` — for quick utility tasks during development

# Failure handling

- If the Tauri version is ambiguous, check `Cargo.toml` for the `tauri` crate version and `package.json` for `@tauri-apps/api` version. v1 and v2 have incompatible APIs — confirm before writing code.
- If a build fails, check: (1) Rust toolchain installed and up to date, (2) system dependencies for the target platform (e.g., `webkit2gtk` on Linux), (3) Vite dev server configuration matches `tauri.conf.json` → `build.devPath`.
- If IPC calls fail silently, verify the command is registered in `generate_handler![]` and the argument names match exactly between Rust and TypeScript (Tauri uses snake_case → camelCase conversion).
- If the app renders blank on a specific platform, check CSP headers and WebView compatibility. WebKitGTK on Linux is often behind on web API support.
