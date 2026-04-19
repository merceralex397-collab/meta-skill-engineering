#!/usr/bin/env python3
"""Check that a modified skill preserves protected content from the original."""

import json
import sys
import re
from pathlib import Path


def extract_section(text, heading):
    """Extract the content under a markdown heading (# or ##)."""
    pattern = re.compile(
        r"^#{1,2}\s+" + re.escape(heading) + r"\s*\n(.*?)(?=^#{1,2}\s|\Z)",
        re.MULTILINE | re.DOTALL,
    )
    match = pattern.search(text)
    return match.group(1).strip() if match else ""


def extract_words(text):
    """Extract a set of lowercase words from text."""
    return set(re.findall(r"[a-z0-9]+(?:-[a-z0-9]+)*", text.lower()))


def jaccard(a, b):
    """Jaccard similarity between two sets."""
    if not a and not b:
        return 1.0
    if not a or not b:
        return 0.0
    return len(a & b) / len(a | b)


def word_overlap(text_a, text_b):
    """Word-level Jaccard similarity between two text blocks."""
    return jaccard(extract_words(text_a), extract_words(text_b))


def extract_boundary_lines(section_text):
    """Extract individual boundary entries from a 'When NOT to use' section."""
    lines = []
    current = []
    for line in section_text.splitlines():
        stripped = line.strip()
        if stripped.startswith("- "):
            if current:
                lines.append(" ".join(current))
            current = [stripped.lstrip("- ").strip()]
        elif stripped and current:
            current.append(stripped)
    if current:
        lines.append(" ".join(current))
    return lines


def extract_file_refs(text):
    """Find references to references/*.md or scripts/*.py files."""
    return set(re.findall(r"(?:references|scripts)/[\w.-]+\.(?:md|py)", text))


def extract_cross_refs(text):
    """Find backtick-quoted skill names (kebab-case identifiers)."""
    return set(re.findall(r"`([a-z][a-z0-9]*(?:-[a-z0-9]+)+)`", text))


def extract_tool_refs(text):
    """Find tool/command references — backtick-quoted commands and CLI tool names."""
    tools = set()
    # Backtick-quoted commands (e.g., `pg_dump --schema-only`, `pgloader`)
    for match in re.findall(r"`([^`]+)`", text):
        # Filter to things that look like tools/commands, not skill names
        stripped = match.strip()
        if not stripped:
            continue
        # Skip pure skill cross-refs (handled separately)
        if re.match(r"^[a-z][a-z0-9]*(-[a-z0-9]+)+$", stripped):
            continue
        # Keep things that look like commands, tools, or filenames
        if re.search(r"[A-Z_./\\]|^[a-z_]+$", stripped) or " " in stripped:
            tools.add(stripped)
    return tools


def check_preservation(original_path, modified_path):
    """Compare original and modified SKILL.md for content preservation."""
    original = Path(original_path).read_text(encoding="utf-8")
    modified = Path(modified_path).read_text(encoding="utf-8")

    result = {
        "original": str(original_path),
        "modified": str(modified_path),
        "preserved": True,
        "checks": {},
        "violations": [],
    }

    # 1. Purpose preservation
    orig_purpose = extract_section(original, "Purpose")
    mod_purpose = extract_section(modified, "Purpose")
    if not orig_purpose:
        purpose_check = {"pass": True, "overlap": 1.0, "detail": "No purpose in original"}
    elif not mod_purpose:
        purpose_check = {"pass": False, "overlap": 0.0, "detail": "Purpose section removed"}
    else:
        overlap = round(word_overlap(orig_purpose, mod_purpose), 2)
        passed = overlap >= 0.4
        detail = "" if passed else f"Overlap {overlap} below 0.4 threshold"
        purpose_check = {"pass": passed, "overlap": overlap, "detail": detail}
    result["checks"]["purpose_preserved"] = purpose_check
    if not purpose_check["pass"]:
        result["violations"].append(f"purpose_preserved: {purpose_check['detail']}")

    # 2. Negative boundaries
    orig_boundaries_section = extract_section(original, "When NOT to use")
    mod_boundaries_section = extract_section(modified, "When NOT to use")
    orig_boundaries = extract_boundary_lines(orig_boundaries_section)
    mod_boundaries = extract_boundary_lines(mod_boundaries_section)

    missing_boundaries = []
    for orig_b in orig_boundaries:
        orig_words = extract_words(orig_b)
        found = False
        for mod_b in mod_boundaries:
            if jaccard(orig_words, extract_words(mod_b)) >= 0.4:
                found = True
                break
        if not found:
            # Use first few words as label
            label = " ".join(orig_b.split()[:5]) + "..."
            missing_boundaries.append(label)

    if not orig_boundaries:
        boundaries_check = {"pass": True, "missing": [], "detail": "No boundaries in original"}
    elif missing_boundaries:
        boundaries_check = {
            "pass": False,
            "missing": missing_boundaries,
            "detail": f"{len(missing_boundaries)} of {len(orig_boundaries)} boundaries missing",
        }
    else:
        boundaries_check = {
            "pass": True,
            "missing": [],
            "detail": f"{len(orig_boundaries)} boundaries intact",
        }
    result["checks"]["boundaries_preserved"] = boundaries_check
    if not boundaries_check["pass"]:
        for m in missing_boundaries:
            result["violations"].append(f"boundaries_preserved: missing {m}")

    # 3. File references
    orig_refs = extract_file_refs(original)
    mod_refs = extract_file_refs(modified)
    dropped_refs = orig_refs - mod_refs

    if dropped_refs:
        file_refs_check = {
            "pass": False,
            "missing": sorted(dropped_refs),
            "detail": f"{len(dropped_refs)} of {len(orig_refs)} refs dropped",
        }
    else:
        file_refs_check = {
            "pass": True,
            "detail": f"{len(orig_refs)} refs intact" if orig_refs else "No file refs in original",
        }
    result["checks"]["file_refs_preserved"] = file_refs_check
    if not file_refs_check["pass"]:
        for r in sorted(dropped_refs):
            result["violations"].append(f"file_refs_preserved: dropped {r}")

    # 4. Cross-references
    orig_xrefs = extract_cross_refs(original)
    mod_xrefs = extract_cross_refs(modified)
    dropped_xrefs = orig_xrefs - mod_xrefs

    if dropped_xrefs:
        cross_refs_check = {
            "pass": False,
            "missing": sorted(dropped_xrefs),
            "detail": f"{len(dropped_xrefs)} of {len(orig_xrefs)} cross-refs dropped",
        }
    else:
        cross_refs_check = {
            "pass": True,
            "detail": f"{len(orig_xrefs)} refs intact" if orig_xrefs else "No cross-refs in original",
        }
    result["checks"]["cross_refs_preserved"] = cross_refs_check
    if not cross_refs_check["pass"]:
        for x in sorted(dropped_xrefs):
            result["violations"].append(f"cross_refs_preserved: dropped `{x}`")

    # 5. Tool integrity
    orig_tools = extract_tool_refs(original)
    mod_tools = extract_tool_refs(modified)
    dropped_tools = orig_tools - mod_tools

    if dropped_tools:
        tool_refs_check = {
            "pass": False,
            "missing": sorted(dropped_tools),
            "detail": f"{len(dropped_tools)} of {len(orig_tools)} tool refs dropped",
        }
    else:
        tool_refs_check = {
            "pass": True,
            "detail": f"{len(orig_tools)} tool refs intact" if orig_tools else "No tool refs in original",
        }
    result["checks"]["tool_refs_preserved"] = tool_refs_check
    if not tool_refs_check["pass"]:
        for t in sorted(dropped_tools):
            result["violations"].append(f"tool_refs_preserved: dropped `{t}`")

    # Overall preserved flag
    result["preserved"] = all(c["pass"] for c in result["checks"].values())

    return result


def main():
    if len(sys.argv) != 3:
        print("Usage: python3 scripts/check_preservation.py <original.md> <modified.md>", file=sys.stderr)
        sys.exit(2)

    original = sys.argv[1]
    modified = sys.argv[2]

    for p in (original, modified):
        if not Path(p).exists():
            print(f"Error: file not found: {p}", file=sys.stderr)
            sys.exit(2)

    result = check_preservation(original, modified)
    print(json.dumps(result, indent=2))
    sys.exit(0 if result["preserved"] else 1)


if __name__ == "__main__":
    main()
