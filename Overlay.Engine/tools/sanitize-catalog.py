#!/usr/bin/env python3
"""sanitize-catalog.py — strip capture-tool artifacts from the responses catalog.

The recorded bodies carry a leading "// status=NNN ..." marker line (and some
are raw log text, not JSON at all). The osc page JSON.parse()s these bodies at
boot; ANY contamination throws an unhandled SyntaxError that stalls the boot
chain and leaves the page ignoring socket pushes (measured 2026-09-13:
"// status=" ... is not valid JSON -> overlayToggle ignored, hash stuck #/base).

Rule per file:
  1. drop leading lines that start with "//"
  2. if the remainder parses as JSON -> write it back (compact, unchanged data)
  3. else salvage the outermost {...} / [...] slice; if that still fails or
     nothing is left -> write "{}" (every consumer fails gracefully on {})
"""
import json
import re
import sys
from pathlib import Path


def salvage(body: str) -> str:
    m = re.search(r"\{.*\}|\[.*\]", body, re.DOTALL)
    if not m:
        return "{}"
    for candidate in (m.group(0), m.group(0).rstrip().rstrip(",")):
        try:
            json.loads(candidate)
            return candidate
        except ValueError:
            continue
    return "{}"


def clean_file(path: Path) -> str:
    raw = path.read_text(encoding="utf-8", errors="replace")
    lines = raw.splitlines()
    kept, i = [], 0
    while i < len(lines) and lines[i].lstrip().startswith("//"):
        i += 1
    kept = lines[i:]
    body = "\n".join(kept).strip()
    if not body:
        cleaned = "{}"
    else:
        try:
            json.loads(body)
            cleaned = body
        except ValueError:
            cleaned = salvage(body)
    if cleaned != raw:
        path.write_text(cleaned, encoding="utf-8", newline="\n")
    return "json" if cleaned != "{}" else "fallback"


def main() -> int:
    roots = [Path(p) for p in sys.argv[1:]] or [Path(__file__).resolve().parent.parent / "responses"]
    total = fixed = 0
    for root in roots:
        for path in sorted(root.rglob("*.json")):
            total += 1
            outcome = clean_file(path)
            if outcome == "fallback":
                fixed += 1
                print(f"  fallback->{{}} : {path.name}")
        print(f"{root}: {total} files scanned, {fixed} non-JSON bodies replaced")
        total = fixed = 0
    return 0


if __name__ == "__main__":
    sys.exit(main())
