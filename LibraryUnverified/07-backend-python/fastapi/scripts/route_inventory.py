#!/usr/bin/env python3
"""Inventory FastAPI routes from Python source using the standard library."""

from __future__ import annotations

import argparse
import ast
import json
from pathlib import Path

HTTP_METHODS = {"get", "post", "put", "patch", "delete", "options", "head"}


def decorator_parts(node: ast.AST) -> tuple[str | None, str | None]:
    if isinstance(node, ast.Call):
        node = node.func
    if isinstance(node, ast.Attribute) and isinstance(node.value, ast.Name):
        return node.value.id, node.attr
    return None, None


def path_from_decorator(node: ast.AST) -> str | None:
    if not isinstance(node, ast.Call) or not node.args:
        return None
    first = node.args[0]
    if isinstance(first, ast.Constant) and isinstance(first.value, str):
        return first.value
    return None


def iter_routes(py_file: Path) -> list[dict[str, object]]:
    try:
        tree = ast.parse(py_file.read_text(encoding="utf-8"), filename=str(py_file))
    except SyntaxError:
        return []

    routes: list[dict[str, object]] = []
    for node in ast.walk(tree):
        if not isinstance(node, (ast.FunctionDef, ast.AsyncFunctionDef)):
            continue
        for deco in node.decorator_list:
            base, attr = decorator_parts(deco)
            if attr not in HTTP_METHODS:
                continue
            routes.append(
                {
                    "file": str(py_file),
                    "line": node.lineno,
                    "handler": node.name,
                    "async": isinstance(node, ast.AsyncFunctionDef),
                    "owner": base,
                    "method": attr.upper(),
                    "path": path_from_decorator(deco),
                }
            )
    return routes


def collect(root: Path) -> list[dict[str, object]]:
    files = [root] if root.is_file() else sorted(root.rglob("*.py"))
    rows: list[dict[str, object]] = []
    for file_path in files:
        rows.extend(iter_routes(file_path))
    return rows


def main() -> int:
    parser = argparse.ArgumentParser(description="List FastAPI routes from Python files.")
    parser.add_argument("path", help="Python file or directory to scan")
    parser.add_argument("--format", choices=["text", "json"], default="text")
    args = parser.parse_args()

    rows = collect(Path(args.path))
    if args.format == "json":
        print(json.dumps(rows, indent=2))
        return 0

    if not rows:
        print("No FastAPI-style route decorators found.")
        return 0

    for row in rows:
        mode = "async" if row["async"] else "sync"
        print(f'{row["method"]:6} {row["path"] or "<dynamic>":30} {mode:5} {row["handler"]}  {row["file"]}:{row["line"]}')
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
