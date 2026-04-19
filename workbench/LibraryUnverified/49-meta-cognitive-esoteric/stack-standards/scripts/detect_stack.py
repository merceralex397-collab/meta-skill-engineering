#!/usr/bin/env python3
import argparse
import json
from pathlib import Path


SIGNALS = {
    "node": ["package.json", "pnpm-lock.yaml", "yarn.lock", "package-lock.json"],
    "python": ["pyproject.toml", "requirements.txt", "uv.lock", "poetry.lock"],
    "rust": ["Cargo.toml"],
    "go": ["go.mod"],
    "dotnet": [".csproj", ".sln"],
}


def detect(root: Path) -> dict:
    found = {key: [] for key in SIGNALS}
    for path in root.rglob("*"):
        if not path.is_file():
            continue
        for stack, patterns in SIGNALS.items():
            for pattern in patterns:
                if path.name == pattern or path.name.endswith(pattern):
                    found[stack].append(str(path.relative_to(root)))
    return {key: value for key, value in found.items() if value}


def main() -> int:
    parser = argparse.ArgumentParser(description="Detect common stack evidence in a repository.")
    parser.add_argument("--root", required=True)
    parser.add_argument("--format", choices=["json", "text"], default="json")
    args = parser.parse_args()

    result = detect(Path(args.root))
    if args.format == "json":
        print(json.dumps(result, indent=2))
    else:
        for stack, files in result.items():
            print(f"{stack}:")
            for item in files:
                print(f"  - {item}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
