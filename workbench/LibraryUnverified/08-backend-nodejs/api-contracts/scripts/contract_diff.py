#!/usr/bin/env python3
"""Flag likely breaking changes between two API schema files."""

from __future__ import annotations

import argparse
import json
from pathlib import Path


def load_doc(path: Path):
    text = path.read_text(encoding="utf-8")
    if path.suffix.lower() == ".json":
        return json.loads(text)
    try:
        import yaml  # type: ignore
    except ImportError as exc:  # pragma: no cover
        raise SystemExit("YAML input requires PyYAML; install it or use JSON.") from exc
    return yaml.safe_load(text)


def schema_required(schema: dict) -> set[str]:
    required = schema.get("required", [])
    return set(required if isinstance(required, list) else [])


def compare(old_doc: dict, new_doc: dict) -> list[str]:
    issues: list[str] = []
    old_paths = old_doc.get("paths", {})
    new_paths = new_doc.get("paths", {})

    for path_name, old_methods in old_paths.items():
        if path_name not in new_paths:
            issues.append(f"Removed path: {path_name}")
            continue
        for method, old_op in old_methods.items():
            if method not in new_paths[path_name]:
                issues.append(f"Removed method: {method.upper()} {path_name}")
                continue
            new_op = new_paths[path_name][method]
            old_codes = set((old_op.get("responses") or {}).keys())
            new_codes = set((new_op.get("responses") or {}).keys())
            for code in sorted(old_codes - new_codes):
                issues.append(f"Removed response code: {method.upper()} {path_name} -> {code}")

            old_body = ((old_op.get("requestBody") or {}).get("content") or {}).get("application/json", {})
            new_body = ((new_op.get("requestBody") or {}).get("content") or {}).get("application/json", {})
            old_req = schema_required(old_body.get("schema") or {})
            new_req = schema_required(new_body.get("schema") or {})
            for field in sorted(new_req - old_req):
                issues.append(f"New required request field: {method.upper()} {path_name} -> {field}")

    old_schemas = ((old_doc.get("components") or {}).get("schemas") or {})
    new_schemas = ((new_doc.get("components") or {}).get("schemas") or {})
    for schema_name, old_schema in old_schemas.items():
        if schema_name not in new_schemas:
            issues.append(f"Removed component schema: {schema_name}")
            continue
        old_props = set((old_schema.get("properties") or {}).keys())
        new_props = set((new_schemas[schema_name].get("properties") or {}).keys())
        for field in sorted(old_props - new_props):
            issues.append(f"Removed schema property: {schema_name}.{field}")

    return issues


def main() -> int:
    parser = argparse.ArgumentParser(description="Compare two API schema files.")
    parser.add_argument("old")
    parser.add_argument("new")
    parser.add_argument("--format", choices=["text", "json"], default="text")
    args = parser.parse_args()

    issues = compare(load_doc(Path(args.old)), load_doc(Path(args.new)))
    if args.format == "json":
        print(json.dumps(issues, indent=2))
        return 0
    if not issues:
        print("No likely breaking changes detected.")
        return 0
    for issue in issues:
        print(issue)
    return 1


if __name__ == "__main__":
    raise SystemExit(main())
