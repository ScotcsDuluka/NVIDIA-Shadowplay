#!/usr/bin/env python3
"""abi_probe.py - targeted x64 disassembly probe for PE binaries.

Zero-GUI, evidence tooling for the NvCapture lane. Given a PE (x64) and an
RVA, this prints a capstone disassembly annotated with:

  * string literals referenced by RIP-relative operands (from an ASCII/UTF-16
    scan of the image)
  * import calls resolved through the IAT  (dll!Function)
  * section of any RIP-relative target (.rdata/.data => potential vtable)

Sub-commands:
  disasm <file> <rva> [nins]      - disassemble nins instructions at RVA
  vtable <file> <va> [count]      - dump count qwords at VA; annotate fn targets
  fn <file> <rva> [nins]          - disasm + auto-follow referenced rdata
                                    qword tables (vtable discovery aid)

All addresses are given in hex with or without 0x prefix. RVAs are relative
to image base; VAs include it.
"""
import struct
import sys
import re
import bisect

from capstone import Cs, CS_ARCH_X86, CS_MODE_64

import os
sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
from strings_scan import parse_sections, scan_ascii, scan_utf16  # noqa: E402


class PE:
    def __init__(self, path):
        self.path = path
        self.data = open(path, "rb").read()
        d = self.data
        e_lfanew = struct.unpack_from("<I", d, 0x3C)[0]
        coff = e_lfanew + 4
        self.machine = struct.unpack_from("<H", d, coff)[0]
        self.nsec = struct.unpack_from("<H", d, coff + 2)[0]
        optsz = struct.unpack_from("<H", d, coff + 16)[0]
        opt = coff + 20
        magic = struct.unpack_from("<H", d, opt)[0]
        if magic == 0x20B:
            self.image_base = struct.unpack_from("<Q", d, opt + 24)[0]
        else:
            raise SystemExit("only PE32+ supported")
        self.entry_rva = struct.unpack_from("<I", d, opt + 16)[0]
        # data directories
        self.dd = []
        numdd = struct.unpack_from("<I", d, opt + 108)[0]
        for i in range(numdd):
            va, sz = struct.unpack_from("<II", d, opt + 112 + i * 8)
            self.dd.append((va, sz))
        secoff = opt + optsz
        self.sections = []  # (name, va, vsize, roff, rsize)
        for i in range(self.nsec):
            o = secoff + i * 40
            name = d[o:o + 8].rstrip(b"\0").decode("ascii", "replace")
            vsz, va, rsz, roff = struct.unpack_from("<IIII", d, o + 8)
            self.sections.append((name, va, vsz, roff, rsz))
        self._strings = None
        self._imports = None

    # ---- address helpers -------------------------------------------------
    def rva2off(self, rva):
        for name, va, vsz, roff, rsize in self.sections:
            if va <= rva < va + max(vsz, rsize):
                if rva - va < rsize:
                    return roff + (rva - va)
                return None
        return None

    def sec_of_rva(self, rva):
        for name, va, vsz, roff, rsize in self.sections:
            if va <= rva < va + max(vsz, rsize):
                return name
        return "?"

    def read(self, rva, n):
        off = self.rva2off(rva)
        if off is None:
            return None
        return self.data[off:off + n]

    def qwords(self, rva, count):
        b = self.read(rva, count * 8)
        if b is None:
            return []
        return list(struct.unpack_from("<%dQ" % count, b)) if len(b) == count * 8 else []

    # ---- strings index ----------------------------------------------------
    @property
    def strings(self):
        if self._strings is None:
            self._strings = {}
            for rva, off, s in scan_ascii(self.data, self.sections, 4):
                if rva is not None:
                    self._strings.setdefault(rva, s)
            for rva, off, s in scan_utf16(self.data, self.sections, 4):
                if rva is not None:
                    self._strings.setdefault(rva, s)
            self._rvas = sorted(self._strings)
        return self._strings

    def string_at_or_near(self, va):
        """If va points at or into a string literal, return it."""
        ss = self.strings
        i = bisect.bisect_right(self._rvas, va) - 1
        if i < 0:
            return None
        rva = self._rvas[i]
        if 0 <= va - rva < 256:
            s = ss[rva]
            tail = s[va - rva:] if va >= rva else s
            if tail and all(0x20 <= ord(c) < 0x7F for c in tail[:32]):
                return tail
        return None

    # ---- imports -----------------------------------------------------------
    @property
    def imports(self):
        if self._imports is None:
            self._imports = {}
            d = self.data
            imp_va, _ = self.dd[1]
            off = self.rva2off(imp_va)
            if off is None:
                self._imports = {}
                return self._imports
            while True:
                oft, tds, fwd, name_rva, ft = struct.unpack_from("<IIIII", d, off)
                if name_rva == 0:
                    break
                dll_off = self.rva2off(name_rva)
                dll = d[dll_off:d[0:].index(b"\0", dll_off)].decode("ascii", "replace") \
                    if dll_off is not None else "?"
                thunk_rva = oft or ft
                toff = self.rva2off(thunk_rva)
                ftoff = self.rva2off(ft)
                idx = 0
                while toff is not None and ftoff is not None:
                    val = struct.unpack_from("<Q", d, toff)[0]
                    slot_va = self.image_base + ft + idx * 8
                    if val == 0:
                        break
                    if val & (1 << 63):
                        fname = "#%d" % (val & 0xFFFF)
                    else:
                        fo = self.rva2off(val & 0x7FFFFFFF)
                        if fo is None:
                            fname = "?"
                        else:
                            end = d.index(b"\0", fo)
                            fname = d[fo + 2:end].decode("ascii", "replace")
                    self._imports[slot_va] = "%s!%s" % (dll, fname)
                    toff += 8
                    ftoff += 8
                    idx += 1
                off += 20
        return self._imports


def ann(pe, va):
    """Human annotation for a code-referenced address (va = image VA)."""
    rva = va - pe.image_base
    if rva < 0:
        return None
    s = pe.string_at_or_near(rva)
    if s:
        return 'str "%s"' % (s[:110] + ("..." if len(s) > 110 else ""))
    imp = pe.imports.get(va)
    if imp:
        return "iat %s" % imp
    sec = pe.sec_of_rva(va)
    if sec in (".rdata", ".data", "_RDATA"):
        return "%s @0x%X" % (sec, va)
    return None


def disasm(pe, rva, nins=60):
    code = pe.read(rva, nins * 12 + 64)
    md = Cs(CS_ARCH_X86, CS_MODE_64)
    md.detail = False
    out = []
    for i, ins in enumerate(md.disasm(code, pe.image_base + rva)):
        if i >= nins:
            break
        line = "  0x%012X  %-24s %s" % (ins.address, ins.mnemonic, ins.op_str)
        # find rip-relative displacement
        m = re.search(r"\[rip (?:\+|-) (0x[0-9a-f]+)\]", ins.op_str)
        if not m:
            m = re.search(r"\[rip ([+-]) (0x[0-9a-f]+)\]", ins.op_str)
        note = None
        mm = re.search(r"rip ([+-]) (0x[0-9a-f]+)", ins.op_str)
        if mm:
            sign = 1 if mm.group(1) == "+" else -1
            disp = int(mm.group(2), 16) * sign
            tgt = ins.address + ins.size + disp
            note = ann(pe, tgt)
        if note:
            line += "   ; " + note
        out.append(line)
        if ins.mnemonic == "ret" and i > 2:
            pass
    return out


def vtable_dump(pe, rva, count=32):
    qs = pe.qwords(rva, count)
    base = pe.image_base + rva
    print("vtable @VA 0x%X (RVA 0x%X, section %s):" % (base, rva, pe.sec_of_rva(rva)))
    for i, q in enumerate(qs):
        if q == 0:
            print("  [%2d] NULL" % i)
            continue
        sec = pe.sec_of_rva(q - pe.image_base) if q > pe.image_base else "?"
        mark = ""
        if sec == ".text":
            mark = "  <fn RVA 0x%X>" % (q - pe.image_base)
        elif pe.imports.get(q):
            mark = "  <iat %s>" % pe.imports[q]
        print("  [%2d] 0x%012X %s%s" % (i, q, sec, mark))
    return qs


def main():
    import argparse
    ap = argparse.ArgumentParser()
    ap.add_argument("cmd", choices=["disasm", "vtable", "fn"])
    ap.add_argument("file")
    ap.add_argument("rva")
    ap.add_argument("count", nargs="?", default="60")
    a = ap.parse_args()
    rva = int(a.rva, 16)
    n = int(a.count, 0)
    pe = PE(a.file)
    if a.cmd == "disasm" or a.cmd == "fn":
        for line in disasm(pe, rva, n):
            print(line)
    elif a.cmd == "vtable":
        vtable_dump(pe, rva, n)


if __name__ == "__main__":
    main()
