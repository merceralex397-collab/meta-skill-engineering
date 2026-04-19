#!/usr/bin/env python3
"""Validate JSONL logs for required fields and obvious secret-bearing keys."""

from __future__ import annotations

import argparse
import json
from pathlib import Path

DEFAULT_REQUIRED = ["timestamp", "level", "event"]
DEFAULT_BANNED = ["password", "secret", "token", "authorization", "api_key"]


def main() -> int:
    parser = argparse.ArgumentParser(description="Check JSONL logs for schema and redaction issues.")
    parser.add_argument("path", help="Path to a JSONL log file")
    parser.add_argument("--required", nargs="*", default=DEFAULT_REQUIRED)
    parser.add_argument("--banned", nargs="*", default=DEFAULT_BANNED)
    args = parser.parse_args()

    missing = 0
    banned_hits: list[str] = []
    lines = Path(args.path).read_text(encoding="utf-8").splitlines()
    for index, line in enumerate(lines, start=1):
        if not line.strip():
            continue
        record = json.loads(line)
        for key in args.required:
            if key not in record:
                print(f"line {index}: missing required key '{key}'")
                missing += 1
        lower_keys = {str(key).lower() for key in record.keys()}
        for banned in args.banned:
            if banned.lower() in lower_keys:
                banned_hits.append(f"line {index}: found banned key '{banned}'")

    for hit in banned_hits:
        print(hit)
    if not missing and not banned_hits:
        print("Log sample passed schema and redaction checks.")
        return 0
    return 1


if __name__ == "__main__":
    raise SystemExit(main())
