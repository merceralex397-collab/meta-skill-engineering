#!/usr/bin/env python3
"""Fetch official Agent Skills repos into a local folder.

This script is intended to be run on a machine with internet access.
It mirrors the official Anthropic and OpenAI skill repos using the GitHub
archive download endpoints, then optionally extracts them.
"""
from __future__ import annotations
import json
import shutil
import sys
import tempfile
import urllib.request
import zipfile
from pathlib import Path

REPOS = {
    'anthropics/skills': 'https://github.com/anthropics/skills/archive/refs/heads/main.zip',
    'openai/skills': 'https://github.com/openai/skills/archive/refs/heads/main.zip',
}


def download(url: str, dest: Path) -> None:
    req = urllib.request.Request(url, headers={'User-Agent': 'agent-skills-fetcher'})
    with urllib.request.urlopen(req) as resp:
        dest.write_bytes(resp.read())


def main() -> int:
    out = Path(sys.argv[1]) if len(sys.argv) > 1 else Path('downloaded-official-skills')
    out.mkdir(parents=True, exist_ok=True)
    for repo, url in REPOS.items():
        slug = repo.replace('/', '__')
        zip_path = out / f'{slug}.zip'
        print(f'Downloading {repo} -> {zip_path}')
        download(url, zip_path)
        extract_dir = out / slug
        extract_dir.mkdir(parents=True, exist_ok=True)
        with zipfile.ZipFile(zip_path) as zf:
            zf.extractall(extract_dir)
        print(f'Extracted {repo} -> {extract_dir}')
    print('
Done.')
    return 0


if __name__ == '__main__':
    raise SystemExit(main())
