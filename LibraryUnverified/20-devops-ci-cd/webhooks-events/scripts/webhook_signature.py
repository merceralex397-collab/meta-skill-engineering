#!/usr/bin/env python3
"""Sign or verify webhook payloads with HMAC-SHA256."""

from __future__ import annotations

import argparse
import hashlib
import hmac
from pathlib import Path


def read_body(args) -> bytes:
    if args.body is not None:
        return args.body.encode("utf-8")
    return Path(args.body_file).read_bytes()


def sign(secret: str, body: bytes) -> str:
    digest = hmac.new(secret.encode("utf-8"), body, hashlib.sha256).hexdigest()
    return f"sha256={digest}"


def main() -> int:
    parser = argparse.ArgumentParser(description="Sign or verify webhook payloads.")
    parser.add_argument("mode", choices=["sign", "verify"])
    parser.add_argument("--secret", required=True)
    parser.add_argument("--body")
    parser.add_argument("--body-file")
    parser.add_argument("--signature")
    args = parser.parse_args()

    body = read_body(args)
    expected = sign(args.secret, body)
    if args.mode == "sign":
      print(expected)
      return 0
    if not args.signature:
      raise SystemExit("--signature is required in verify mode")
    if hmac.compare_digest(expected, args.signature):
      print("signature verified")
      return 0
    print("signature mismatch")
    return 1


if __name__ == "__main__":
    raise SystemExit(main())
