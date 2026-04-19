#!/usr/bin/env python3
"""Check a SKILL.md for structural compliance and output a JSON report."""

import json
import sys
import re
from pathlib import Path

REQUIRED_HEADINGS = [
    "Purpose",
    "When to use",
    "When NOT to use",
    "Procedure",
    "Output contract",
    "Failure handling",
]

OPTIONAL_HEADINGS = [
    "Next steps",
    "References",
]

# Canonical order: required headings first, then optional
CANONICAL_ORDER = REQUIRED_HEADINGS + OPTIONAL_HEADINGS

ALLOWED_FRONTMATTER_FIELDS = {"name", "description"}


def parse_frontmatter(text: str):
    """Extract frontmatter text between --- delimiters. Returns (raw_text, fields_dict) or (None, None)."""
    if not text.startswith("---"):
        return None, None
    # Find closing ---
    end = text.find("\n---", 3)
    if end == -1:
        return None, None
    raw = text[3:end].strip()
    # Simple field extraction (no YAML dependency)
    fields = {}
    current_key = None
    for line in raw.split("\n"):
        # Top-level key: starts at column 0 with "key:" or "key: value"
        m = re.match(r"^([a-zA-Z_-]+)\s*:\s*(.*)", line)
        if m:
            current_key = m.group(1)
            val = m.group(2).strip()
            fields[current_key] = val
        elif current_key and (line.startswith("  ") or line.startswith("\t")):
            # Continuation line for multi-line values
            fields[current_key] = (fields[current_key] + " " + line.strip()).strip()
    return raw, fields


def extract_headings(text: str):
    """Return list of (heading_text, line_number, level) for h1 and h2 headings.

    Real skills use `# Section` (h1) directly.
    Corpus files use `# Title` then `## Section` (h2) for canonical sections.
    We extract both levels so the checker works for either convention.
    """
    headings = []
    for i, line in enumerate(text.split("\n"), 1):
        m = re.match(r"^(#{1,2})\s+(.+)$", line)
        if m:
            level = len(m.group(1))
            headings.append((m.group(2).strip(), i, level))
    return headings


def check_phantom_references(text: str, skill_dir: Path):
    """Check for references to files in references/ or scripts/ that don't exist."""
    phantoms = []
    # Match patterns like references/foo.md, scripts/bar.sh
    for match in re.finditer(r"(?:references|scripts)/[a-zA-Z0-9._-]+\.[a-z]+", text):
        ref_path = skill_dir / match.group(0)
        if not ref_path.exists():
            phantoms.append(match.group(0))
    return phantoms


def check_skill(filepath: str, skill_dir: str | None = None) -> dict:
    path = Path(filepath)
    if not path.exists():
        return {
            "file": filepath,
            "valid": False,
            "score": 0,
            "max_score": 10,
            "checks": {},
            "warnings": [f"File not found: {filepath}"],
        }

    text = path.read_text(encoding="utf-8")
    lines = text.split("\n")
    line_count = len(lines)

    checks = {}
    warnings = []
    score = 0
    max_score = 10  # frontmatter_present + frontmatter_fields + frontmatter_clean + 6 required headings + heading_order

    # --- Frontmatter checks ---
    fm_raw, fm_fields = parse_frontmatter(text)

    if fm_raw is not None:
        checks["frontmatter_present"] = {"pass": True, "detail": ""}
        score += 1
    else:
        checks["frontmatter_present"] = {"pass": False, "detail": "no valid --- delimiters found"}

    if fm_fields:
        has_name = "name" in fm_fields
        has_desc = "description" in fm_fields
        name_val = fm_fields.get("name", "")
        desc_val = fm_fields.get("description", "")
        detail_parts = []
        if has_name:
            detail_parts.append(f"name={name_val}")
        if has_desc:
            desc_preview = desc_val[:60] + ("..." if len(desc_val) > 60 else "")
            detail_parts.append(f"description={desc_preview}")

        if has_name and has_desc:
            checks["frontmatter_fields"] = {"pass": True, "detail": ", ".join(detail_parts)}
            score += 1
        else:
            missing = []
            if not has_name:
                missing.append("name")
            if not has_desc:
                missing.append("description")
            checks["frontmatter_fields"] = {"pass": False, "detail": f"missing: {', '.join(missing)}"}

        # Frontmatter clean check: only name and description allowed
        extra_fields = set(fm_fields.keys()) - ALLOWED_FRONTMATTER_FIELDS
        if extra_fields:
            checks["frontmatter_clean"] = {
                "pass": False,
                "detail": f"unexpected field(s): {', '.join(sorted(extra_fields))}",
            }
        else:
            checks["frontmatter_clean"] = {"pass": True, "detail": ""}
            score += 1

        # Name kebab-case check
        if has_name and name_val:
            if re.match(r"^[a-z0-9]+(-[a-z0-9]+)*$", name_val):
                checks["name_kebab_case"] = {"pass": True, "detail": name_val}
            else:
                checks["name_kebab_case"] = {"pass": False, "detail": f"'{name_val}' is not kebab-case"}
                warnings.append(f"name '{name_val}' does not match kebab-case pattern")

        # Description length check
        if has_desc and desc_val:
            if len(desc_val) > 1024:
                warnings.append(f"description is {len(desc_val)} chars (max recommended: 1024)")
    else:
        checks["frontmatter_fields"] = {"pass": False, "detail": "no frontmatter to check"}
        checks["frontmatter_clean"] = {"pass": False, "detail": "no frontmatter to check"}

    # --- Required heading checks ---
    headings = extract_headings(text)
    heading_texts = [h[0] for h in headings]

    for heading in REQUIRED_HEADINGS:
        key = "has_" + heading.lower().replace(" ", "_").replace("NOT_", "not_")
        if heading in heading_texts:
            checks[key] = {"pass": True, "detail": ""}
            score += 1
        else:
            checks[key] = {"pass": False, "detail": "section missing"}

    # --- Optional heading checks (informational, don't affect score) ---
    for heading in OPTIONAL_HEADINGS:
        key = "has_" + heading.lower().replace(" ", "_")
        if heading in heading_texts:
            checks[key] = {"pass": True, "detail": ""}
        else:
            checks[key] = {"pass": False, "detail": "optional section missing"}

    # --- Heading order check ---
    # Filter to only canonical headings (skip title headings like "# CI Pipeline Validator")
    found_canonical = [h for h in heading_texts if h in CANONICAL_ORDER]
    expected_order = [h for h in CANONICAL_ORDER if h in found_canonical]
    if found_canonical == expected_order:
        checks["heading_order"] = {"pass": True, "detail": ""}
        score += 1
    else:
        checks["heading_order"] = {
            "pass": False,
            "detail": f"found order: {found_canonical}, expected: {expected_order}",
        }

    # --- Line count check ---
    if line_count > 500:
        checks["line_count"] = {"pass": False, "detail": f"{line_count} lines (exceeds 500)"}
        warnings.append(f"line count exceeds limit: {line_count}")
    elif line_count > 400:
        checks["line_count"] = {"pass": True, "detail": f"{line_count} lines"}
        warnings.append(f"line count approaching limit: {line_count}")
    else:
        checks["line_count"] = {"pass": True, "detail": f"{line_count} lines"}

    # --- Phantom file references ---
    if skill_dir:
        sd = Path(skill_dir)
        if sd.is_dir():
            phantoms = check_phantom_references(text, sd)
            if phantoms:
                checks["phantom_references"] = {
                    "pass": False,
                    "detail": f"missing: {', '.join(phantoms)}",
                }
                warnings.append(f"phantom file references: {', '.join(phantoms)}")
            else:
                checks["phantom_references"] = {"pass": True, "detail": ""}

    valid = checks.get("frontmatter_present", {}).get("pass", False) and all(
        checks.get("has_" + h.lower().replace(" ", "_").replace("NOT_", "not_"), {}).get("pass", False)
        for h in REQUIRED_HEADINGS
    )

    return {
        "file": filepath,
        "valid": valid,
        "score": score,
        "max_score": max_score,
        "checks": checks,
        "warnings": warnings,
    }


def main():
    import argparse

    parser = argparse.ArgumentParser(description="Check a SKILL.md for structural compliance")
    parser.add_argument("file", help="Path to the SKILL.md file to check")
    parser.add_argument("--skill-dir", help="Path to the skill directory (enables phantom reference checks)")
    parser.add_argument("--pretty", action="store_true", help="Pretty-print JSON output")
    args = parser.parse_args()

    result = check_skill(args.file, args.skill_dir)
    indent = 2 if args.pretty else None
    print(json.dumps(result, indent=indent))
    sys.exit(0 if result["valid"] else 1)


if __name__ == "__main__":
    main()
