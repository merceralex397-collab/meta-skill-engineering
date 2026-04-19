#!/usr/bin/env python3
"""List skills from a GitHub repo path."""

from __future__ import annotations

import argparse
import json
import os
import sys
import urllib.error

from github_utils import (
    CLIENT_CHOICES,
    DEFAULT_CLIENT,
    client_skills_dir,
    github_api_contents_url,
    github_request,
)

DEFAULT_PATH = "skills/.curated"
DEFAULT_REF = "main"


class ListError(Exception):
    pass


class Args(argparse.Namespace):
    repo: str
    path: str
    ref: str
    format: str
    client: str


def _request(url: str) -> bytes:
    return github_request(url, "scafforge-skill-list")


def _installed_skills(client: str) -> set[str]:
    root = client_skills_dir(client)
    if not os.path.isdir(root):
        return set()
    entries = set()
    for name in os.listdir(root):
        path = os.path.join(root, name)
        if os.path.isdir(path):
            entries.add(name)
    return entries


def _list_skills(repo: str, path: str, ref: str) -> list[str]:
    api_url = github_api_contents_url(repo, path, ref)
    try:
        payload = _request(api_url)
    except urllib.error.HTTPError as exc:
        if exc.code == 404:
            raise ListError(
                "Skills path not found: "
                f"https://github.com/{repo}/tree/{ref}/{path}"
            ) from exc
        raise ListError(f"Failed to fetch skills: HTTP {exc.code}") from exc
    data = json.loads(payload.decode("utf-8"))
    if not isinstance(data, list):
        raise ListError("Unexpected skills listing response.")
    skills = [item["name"] for item in data if item.get("type") == "dir"]
    return sorted(skills)


def _parse_args(argv: list[str]) -> Args:
    parser = argparse.ArgumentParser(description="List skills.")
    parser.add_argument("--repo", required=True, help="owner/repo to list skills from")
    parser.add_argument(
        "--path",
        default=DEFAULT_PATH,
        help="Repo path to list (default: skills/.curated)",
    )
    parser.add_argument("--ref", default=DEFAULT_REF)
    parser.add_argument(
        "--format",
        choices=["text", "json"],
        default="text",
        help="Output format",
    )
    parser.add_argument(
        "--client",
        choices=CLIENT_CHOICES,
        default=DEFAULT_CLIENT,
        help="Target agent client (determines installed-skill check path)",
    )
    return parser.parse_args(argv, namespace=Args())


def main(argv: list[str]) -> int:
    args = _parse_args(argv)
    try:
        skills = _list_skills(args.repo, args.path, args.ref)
        installed = _installed_skills(args.client)
        if args.format == "json":
            payload = [
                {"name": name, "installed": name in installed} for name in skills
            ]
            print(json.dumps(payload))
        else:
            for idx, name in enumerate(skills, start=1):
                suffix = " (already installed)" if name in installed else ""
                print(f"{idx}. {name}{suffix}")
        return 0
    except ListError as exc:
        print(f"Error: {exc}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
