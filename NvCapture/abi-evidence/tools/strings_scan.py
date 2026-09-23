#!/usr/bin/env python3
"""strings_scan.py - zero-dep PE string scanner with RVA mapping.

Scans a PE file for ASCII and UTF-16LE strings (min length configurable),
maps each hit's file offset back to an RVA via the section table, and
optionally filters the output by a regex.

Usage:
  python strings_scan.py <file.dll> [--min N] [--wide] [--filter REGEX] [--all]

Output line format:
  RVA=0x%08X OFF=0x%08X <ascii|wide> <string>

Evidence-only tool for the NvCapture lane. Read-only with respect to the
scanned file.
"""
import argparse
import re
import struct
import sys


def parse_sections(data):
    e_lfanew = struct.unpack_from("<I", data, 0x3C)[0]
    assert data[e_lfanew:e_lfanew + 4] == b"PE\0\0", "not a PE file"
    coff = e_lfanew + 4
    machine, nsec, _, _, _, opt_size, _ = struct.unpack_from("<HHIIIHH", data, coff)
    opt = coff + 20
    magic = struct.unpack_from("<H", data, opt)[0]
    image_base = 0
    if magic == 0x20B:  # PE32+
        image_base = struct.unpack_from("<Q", data, opt + 16)[0]
    else:  # PE32
        image_base = struct.unpack_from("<I", data, opt + 12)[0]
    sec_off = opt + opt_size
    sections = []
    for i in range(nsec):
        o = sec_off + i * 40
        name = data[o:o + 8].rstrip(b"\0").decode("ascii", "replace")
        vsize, vaddr, rsize, roff = struct.unpack_from("<IIII", data, o + 8)
        sections.append((name, vaddr, vsize, roff, rsize))
    return machine, image_base, sections


def off_to_rva(sections, off):
    for name, vaddr, vsize, roff, rsize in sections:
        if roff <= off < roff + rsize:
            return vaddr + (off - roff)
    return None


def is_printable(b):
    return 0x20 <= b < 0x7F


def scan_ascii(data, sections, minlen):
    out = []
    start = None
    n = len(data)
    for i, b in enumerate(data):
        if is_printable(b):
            if start is None:
                start = i
        else:
            if start is not None and i - start >= minlen:
                rva = off_to_rva(sections, start)
                out.append((rva, start, data[start:i].decode("ascii")))
            start = None
    return out


def scan_utf16(data, sections, minlen):
    out = []
    n = len(data)
    i = 0
    start = None
    while i + 1 < n:
        lo, hi = data[i], data[i + 1]
        if hi == 0 and is_printable(lo):
            if start is None:
                start = i
            i += 2
        else:
            if start is not None and (i - start) // 2 >= minlen:
                s = data[start:i].decode("utf-16le", "replace")
                rva = off_to_rva(sections, start)
                out.append((rva, start, s))
            start = None
            i += 1 if start is None else 0
            i += 1
    return out


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("file")
    ap.add_argument("--min", type=int, default=6)
    ap.add_argument("--filter", default=None)
    ap.add_argument("--all", action="store_true", help="print both ascii and wide")
    ap.add_argument("--wide-only", action="store_true")
    args = ap.parse_args()

    data = open(args.file, "rb").read()
    machine, image_base, sections = parse_sections(data)
    mach_name = {0x8664: "x64", 0x14C: "x86"}.get(machine, hex(machine))
    sys.stderr.write(f"# {args.file}: machine={mach_name} image_base=0x{image_base:X} "
                     f"sections={[(s[0], hex(s[1])) for s in sections]}\n")

    rx = re.compile(args.filter) if args.filter else None

    if not args.wide_only:
        for rva, off, s in scan_ascii(data, sections, args.min):
            if rx and not rx.search(s):
                continue
            print(f"RVA=0x{rva:08X} OFF=0x{off:08X} ascii {s!r}")
    if args.all or args.wide_only:
        for rva, off, s in scan_utf16(data, sections, args.min):
            if rx and not rx.search(s):
                continue
            print(f"RVA=0x{rva:08X} OFF=0x{off:08X} wide  {s!r}")


if __name__ == "__main__":
    main()
