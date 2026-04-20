#!/usr/bin/env python3
import argparse
import json
import re
import sys
from pathlib import Path


WEAK_PHRASES = [
    "works well",
    "user-friendly",
    "intuitive",
    "robust",
    "seamless",
    "as expected",
    "properly handles",
    "fast",
    "performant",
]

OVERLOAD_MARKERS = [" and ", " or ", " including ", " as well as "]


def load_text(path: str) -> str:
    input_path = Path(path)
    if not input_path.exists():
        raise FileNotFoundError(f"input file not found: {input_path}")
    return input_path.read_text(encoding="utf-8")


def analyze(text: str) -> dict:
    lines = [line.strip() for line in text.splitlines() if line.strip()]
    findings = []
    for index, line in enumerate(lines, start=1):
        lower = line.lower()
        weak_hits = [phrase for phrase in WEAK_PHRASES if phrase in lower]
        overloaded = any(marker in lower for marker in OVERLOAD_MARKERS)
        if weak_hits or overloaded:
            findings.append(
                {
                    "line": index,
                    "text": line,
                    "weak_phrases": weak_hits,
                    "overloaded": overloaded,
                }
            )
    return {
        "finding_count": len(findings),
        "findings": findings,
    }


def format_text(result: dict) -> str:
    lines = [f"findings: {result['finding_count']}"]
    for finding in result["findings"]:
        notes = []
        if finding["weak_phrases"]:
            notes.append("weak=" + ",".join(finding["weak_phrases"]))
        if finding["overloaded"]:
            notes.append("overloaded=true")
        lines.append(f"- line {finding['line']}: {'; '.join(notes)} :: {finding['text']}")
    return "\n".join(lines)


def main() -> int:
    parser = argparse.ArgumentParser(description="Flag weak or overloaded acceptance-criteria lines.")
    parser.add_argument("--input", required=True, help="Path to a markdown or text file.")
    parser.add_argument("--format", choices=["json", "text"], default="json", help="Output format.")
    args = parser.parse_args()

    try:
        text = load_text(args.input)
    except FileNotFoundError as exc:
        print(json.dumps({"error": str(exc)}), file=sys.stderr)
        return 2

    result = analyze(text)
    if args.format == "json":
        print(json.dumps(result, indent=2))
    else:
        print(format_text(result))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
