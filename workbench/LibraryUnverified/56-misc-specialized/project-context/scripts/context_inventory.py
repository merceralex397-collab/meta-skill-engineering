#!/usr/bin/env python3
import argparse
import json
from pathlib import Path


PATTERNS = [
    ("AGENTS.md", 100),
    ("CLAUDE.md", 95),
    ("START-HERE.md", 90),
    ("README.md", 80),
    ("docs", 50),
    ("spec", 60),
    ("ticket", 55),
]


def score_path(path: Path) -> int:
    score = 0
    name = path.name.lower()
    for token, weight in PATTERNS:
        if token.lower() in name or token.lower() in str(path.parent).lower():
            score += weight
    return score


def inventory(root: Path) -> list[dict]:
    results = []
    for path in root.rglob("*"):
        if path.is_file():
            score = score_path(path)
            if score > 0:
                results.append({"path": str(path.relative_to(root)), "score": score})
    results.sort(key=lambda item: item["score"], reverse=True)
    return results


def main() -> int:
    parser = argparse.ArgumentParser(description="Rank likely authority files in a repository.")
    parser.add_argument("--root", required=True, help="Repository root.")
    parser.add_argument("--format", choices=["json", "text"], default="json")
    args = parser.parse_args()

    root = Path(args.root)
    results = inventory(root)
    if args.format == "json":
        print(json.dumps(results, indent=2))
    else:
        for item in results[:20]:
            print(f"{item['score']:>3} {item['path']}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
