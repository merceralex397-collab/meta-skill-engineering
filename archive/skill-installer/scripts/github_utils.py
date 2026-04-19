#!/usr/bin/env python3
"""Shared helpers for skill install scripts."""

from __future__ import annotations

import os
import urllib.request

CLIENT_CHOICES = ["copilot", "codex", "opencode", "claude-code", "gemini-cli"]
DEFAULT_CLIENT = "copilot"


def client_skills_dir(client: str) -> str:
    """Return the default skills directory for the given client."""
    paths = {
        "copilot": os.path.expanduser("~/.copilot/skills"),
        "codex": os.path.expanduser("~/.codex/skills"),
        "opencode": os.path.join(".opencode", "skills"),
        "claude-code": os.path.expanduser("~/.claude/skills"),
        "gemini-cli": os.path.expanduser("~/.gemini/skills"),
    }
    if client not in paths:
        raise ValueError(f"Unknown client: {client}")
    return paths[client]


def github_request(url: str, user_agent: str) -> bytes:
    headers = {"User-Agent": user_agent}
    token = os.environ.get("GITHUB_TOKEN") or os.environ.get("GH_TOKEN")
    if token:
        headers["Authorization"] = f"token {token}"
    req = urllib.request.Request(url, headers=headers)
    with urllib.request.urlopen(req) as resp:
        return resp.read()


def github_api_contents_url(repo: str, path: str, ref: str) -> str:
    return f"https://api.github.com/repos/{repo}/contents/{path}?ref={ref}"
