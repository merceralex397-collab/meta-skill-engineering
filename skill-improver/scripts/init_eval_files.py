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
        {"id": "tp-001", "prompt": "Replace me with a realistic positive trigger prompt.", "should_trigger": True}
    ])
    write_jsonl(evals / "trigger-negative.jsonl", [
        {"id": "tn-001", "prompt": "Replace me with a realistic negative trigger prompt.", "should_trigger": False}
    ])
    write_jsonl(evals / "behavior.jsonl", [
        {"id": "bh-001", "prompt": "Replace me with a realistic behavior test.", "assertions": ["replace me"]}
    ])
    print(json.dumps({"status": "ok", "eval_dir": str(evals)}))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
