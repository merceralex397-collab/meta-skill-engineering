#!/usr/bin/env python3
import argparse
import json
import re
import sys
from pathlib import Path


CHECKS = {
    "goal": [r"\bgoal\b", r"\bobjective\b", r"\boutcome\b", r"\bdone\b"],
    "ordered_steps": [r"^\s*\d+\.", r"^\s*-\s", r"^\s*\*\s"],
    "dependencies": [r"\bdepends\b", r"\bprereq", r"\bbefore\b", r"\bafter\b"],
    "verification": [r"\bverify\b", r"\bvalidate\b", r"\btest\b", r"\bcheck\b"],
    "rollback": [r"\brollback\b", r"\brevert\b", r"\bfeature flag\b", r"\bbackup\b"],
    "observability": [r"\bmonitor", r"\balert", r"\bdashboard", r"\blog"],
}


def read_input(path: str) -> str:
    input_path = Path(path)
    if not input_path.exists():
        raise FileNotFoundError(f"input file not found: {input_path}")
    return input_path.read_text(encoding="utf-8")


def find_matches(text: str, patterns: list[str]) -> int:
    count = 0
    flags = re.IGNORECASE | re.MULTILINE
    for pattern in patterns:
        count += len(re.findall(pattern, text, flags))
    return count


def analyze(text: str) -> dict:
    checks = {}
    for name, patterns in CHECKS.items():
        count = find_matches(text, patterns)
        checks[name] = {
            "hits": count,
            "status": "present" if count > 0 else "missing",
        }

    score = sum(1 for item in checks.values() if item["status"] == "present")
    return {
        "score": score,
        "max_score": len(CHECKS),
        "checks": checks,
    }


def emit_text(result: dict) -> str:
    lines = [f"score: {result['score']}/{result['max_score']}"]
    for name, details in result["checks"].items():
        lines.append(f"- {name}: {details['status']} ({details['hits']} hits)")
    return "\n".join(lines)


def main() -> int:
    parser = argparse.ArgumentParser(description="Run a heuristic completeness pass over an execution plan.")
    parser.add_argument("--input", required=True, help="Path to a markdown or text file containing the plan.")
    parser.add_argument("--format", choices=["json", "text"], default="json", help="Output format.")
    args = parser.parse_args()

    try:
        text = read_input(args.input)
    except FileNotFoundError as exc:
        print(json.dumps({"error": str(exc)}), file=sys.stderr)
        return 2

    result = analyze(text)
    if args.format == "json":
        print(json.dumps(result, indent=2))
    else:
        print(emit_text(result))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
