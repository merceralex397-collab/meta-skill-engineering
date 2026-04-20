#!/usr/bin/env python3
import argparse
import json
import sys
from pathlib import Path


def load_payload(path: str) -> dict:
    input_path = Path(path)
    if not input_path.exists():
        raise FileNotFoundError(f"input file not found: {input_path}")
    return json.loads(input_path.read_text(encoding="utf-8"))


def score(payload: dict) -> dict:
    criteria = payload.get("criteria", [])
    options = payload.get("options", [])
    weights = {criterion["name"]: criterion.get("weight", 1) for criterion in criteria}

    ranked = []
    for option in options:
        subtotal = 0
        breakdown = {}
        for criterion_name, weight in weights.items():
            raw = option.get("scores", {}).get(criterion_name, 0)
            weighted = raw * weight
            breakdown[criterion_name] = {"raw": raw, "weight": weight, "weighted": weighted}
            subtotal += weighted
        ranked.append({"name": option["name"], "total": subtotal, "breakdown": breakdown})

    ranked.sort(key=lambda item: item["total"], reverse=True)
    return {"ranked_options": ranked}


def format_text(result: dict) -> str:
    lines = []
    for index, option in enumerate(result["ranked_options"], start=1):
        lines.append(f"{index}. {option['name']} ({option['total']})")
    return "\n".join(lines)


def main() -> int:
    parser = argparse.ArgumentParser(description="Score structured options against weighted criteria.")
    parser.add_argument("--input", required=True, help="Path to a JSON file containing criteria and options.")
    parser.add_argument("--format", choices=["json", "text"], default="json", help="Output format.")
    args = parser.parse_args()

    try:
        payload = load_payload(args.input)
    except FileNotFoundError as exc:
        print(json.dumps({"error": str(exc)}), file=sys.stderr)
        return 2

    result = score(payload)
    if args.format == "json":
        print(json.dumps(result, indent=2))
    else:
        print(format_text(result))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
