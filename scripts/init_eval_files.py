#!/usr/bin/env python3
import argparse
import json
from pathlib import Path


def write_jsonl(path: Path, rows):
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8") as f:
        for row in rows:
            f.write(json.dumps(row) + "\n")


def main() -> int:
    parser = argparse.ArgumentParser(description="Bootstrap trigger and behavior eval files for a skill package")
    parser.add_argument("skill_dir", help="Path to the skill directory")
    args = parser.parse_args()

    root = Path(args.skill_dir)
    evals = root / "evals"
    write_jsonl(evals / "trigger-positive.jsonl", [
        {"prompt": "Replace with a realistic positive trigger prompt", "expected": "trigger", "category": "core", "notes": "Replace with explanation of why this should trigger"}
    ])
    write_jsonl(evals / "trigger-negative.jsonl", [
        {"prompt": "Replace with a realistic negative trigger prompt", "expected": "no_trigger", "category": "anti-match", "notes": "Replace with explanation of why this should NOT trigger"}
    ])
    write_jsonl(evals / "behavior.jsonl", [
        {"prompt": "Replace with a realistic behavior test prompt", "expected_sections": ["Replace", "with", "expected", "sections"], "required_patterns": ["replace_pattern"], "forbidden_patterns": ["TODO", "placeholder"], "min_output_lines": 15, "notes": "Replace with explanation"}
    ])
    print(json.dumps({"status": "ok", "eval_dir": str(evals)}))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
