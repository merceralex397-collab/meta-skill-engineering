#!/usr/bin/env python3
"""Calculate retry delay windows and total wait budget."""

from __future__ import annotations

import argparse


def delay_for(attempt: int, base_ms: int, factor: float, cap_ms: int) -> float:
    return min(cap_ms, base_ms * (factor ** (attempt - 1)))


def jitter_window(delay_ms: float, mode: str) -> tuple[float, float]:
    if mode == "none":
        return delay_ms, delay_ms
    if mode == "equal":
        return delay_ms / 2, delay_ms
    if mode == "full":
        return 0.0, delay_ms
    raise ValueError(mode)


def main() -> int:
    parser = argparse.ArgumentParser(description="Compute backoff windows and total delay budget.")
    parser.add_argument("--attempts", type=int, default=5)
    parser.add_argument("--base-ms", type=int, default=250)
    parser.add_argument("--factor", type=float, default=2.0)
    parser.add_argument("--cap-ms", type=int, default=10000)
    parser.add_argument("--jitter", choices=["none", "equal", "full"], default="full")
    args = parser.parse_args()

    total_min = 0.0
    total_max = 0.0
    for attempt in range(1, args.attempts + 1):
        delay = delay_for(attempt, args.base_ms, args.factor, args.cap_ms)
        low, high = jitter_window(delay, args.jitter)
        total_min += low
        total_max += high
        print(f"attempt {attempt}: {low:.0f}ms .. {high:.0f}ms")
    print(f"total wait budget: {total_min:.0f}ms .. {total_max:.0f}ms")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
