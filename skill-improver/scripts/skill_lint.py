#!/usr/bin/env python3
import argparse
import sys
from pathlib import Path

REQUIRED_SECTIONS = [
    "# Purpose",
    "# When to use this skill",
    "# Failure handling",
]


def parse_frontmatter(text: str):
    if not text.startswith("---\n"):
        return None
    end = text.find("\n---\n", 4)
    if end == -1:
        return None
    return text[4:end]


def main() -> int:
    parser = argparse.ArgumentParser(description="Light structural lint for a skill package")
    parser.add_argument("skill_dir", help="Path to the skill directory")
    args = parser.parse_args()

    root = Path(args.skill_dir)
    skill_md = root / "SKILL.md"
    if not skill_md.exists():
        print("ERROR: missing SKILL.md", file=sys.stderr)
        return 2

    text = skill_md.read_text(encoding="utf-8")
    fm = parse_frontmatter(text)
    if fm is None:
        print("ERROR: invalid or missing YAML frontmatter", file=sys.stderr)
        return 2

    problems = []
    if "name:" not in fm:
        problems.append("frontmatter missing name")
    if "description:" not in fm:
        problems.append("frontmatter missing description")
    if "Do not use" not in text:
        problems.append("skill body may be missing a negative boundary")
    if " when " not in fm.lower() and "use this skill when" not in text.lower():
        problems.append("routing cues may be too weak")

    for section in REQUIRED_SECTIONS:
        if section not in text:
            problems.append(f"missing section: {section}")

    for rel in ("references", "scripts", "evals"):
        p = root / rel
        if p.exists() and not any(p.iterdir()):
            problems.append(f"{rel}/ exists but is empty")

    if problems:
        print("WARNINGS:")
        for item in problems:
            print(f"- {item}")
        return 1

    print("OK: no structural issues detected")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
