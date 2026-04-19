#!/usr/bin/env python3
"""Harvest failures from eval results into regression test entries."""

import json
import sys
import os
import re
from pathlib import Path
from datetime import datetime, timezone


def parse_failure_lines(text):
    """Extract failure entries from eval results markdown.

    Looks for lines starting with ❌ or containing FAIL with structured info.
    Returns a list of dicts with parsed failure data.
    """
    failures = []
    for line in text.splitlines():
        stripped = line.strip()
        if not stripped:
            continue
        # Match lines starting with ❌ or containing FAIL
        is_failure = stripped.startswith("❌") or stripped.startswith("\u274c")
        if not is_failure and "FAIL" not in stripped:
            continue

        failure = {"raw": stripped}

        # Try to extract structured fields from the line
        # Pattern: ❌ [N] FAIL (category): prompt... [expected=X, activated=Y
        m = re.search(
            r"\[(\d+)\]\s*FAIL\s*\(([^)]*)\):\s*(.*?)\s*\[expected=(\w+),\s*activated=(\w+)",
            stripped,
        )
        if m:
            failure["index"] = int(m.group(1))
            failure["category"] = m.group(2)
            failure["prompt"] = m.group(3).rstrip(".")
            failure["expected"] = m.group(4)
            failure["actual"] = "trigger" if m.group(5) == "true" else "no_trigger"
            failure["type"] = "trigger_failure"
        else:
            # Try simpler FAIL pattern
            m2 = re.search(r"FAIL[:\s]*(.*)", stripped)
            if m2:
                failure["prompt"] = m2.group(1).strip().rstrip(".")
            else:
                failure["prompt"] = stripped

            failure["type"] = "trigger_failure"
            failure["expected"] = "unknown"
            failure["actual"] = "unknown"

        failures.append(failure)

    return failures


def infer_skill_name(eval_path):
    """Infer skill name from eval results filename."""
    stem = Path(eval_path).stem
    # Remove -eval suffix
    if stem.endswith("-eval"):
        return stem[: -len("-eval")]
    return stem


def make_regression_id(skill, failure_type, index):
    """Generate a deterministic regression ID."""
    type_prefix = {"trigger_failure": "tp", "structural_failure": "sp", "preservation_failure": "pp"}
    prefix = type_prefix.get(failure_type, "xx")
    return f"{skill}-{prefix}-{index:03d}"


def harvest(eval_path):
    """Process an eval results file and create regression entries."""
    eval_file = Path(eval_path)
    if not eval_file.exists():
        print(f"Error: file not found: {eval_path}", file=sys.stderr)
        return 1

    text = eval_file.read_text(encoding="utf-8")
    failures = parse_failure_lines(text)

    if not failures:
        print(f"No failures found in {eval_path}")
        return 0

    # Ensure corpus/regression/ exists
    regression_dir = Path("corpus/regression")
    regression_dir.mkdir(parents=True, exist_ok=True)

    skill_name = infer_skill_name(eval_path)
    now = datetime.now(timezone.utc).strftime("%Y-%m-%dT%H:%M:%SZ")

    created = 0
    skipped = 0

    for i, failure in enumerate(failures, start=1):
        reg_id = make_regression_id(skill_name, failure["type"], i)
        out_file = regression_dir / f"{reg_id}.json"

        if out_file.exists():
            skipped += 1
            continue

        entry = {
            "id": reg_id,
            "source_eval": str(eval_path),
            "harvested_at": now,
            "type": failure["type"],
            "skill": skill_name,
            "prompt": failure.get("prompt", ""),
            "expected": failure.get("expected", "unknown"),
            "actual": failure.get("actual", "unknown"),
            "notes": "Extracted from eval run",
        }

        out_file.write_text(json.dumps(entry, indent=2) + "\n", encoding="utf-8")
        created += 1

    total = len(failures)
    print(f"Harvested {total} failure(s) from {eval_path}")
    print(f"  Created: {created}")
    print(f"  Skipped (already exist): {skipped}")
    return 0


def main():
    if len(sys.argv) != 2:
        print("Usage: python3 scripts/harvest_failures.py <eval-results-file.md>", file=sys.stderr)
        sys.exit(2)

    sys.exit(harvest(sys.argv[1]))


if __name__ == "__main__":
    main()
