#!/usr/bin/env python3
"""vtable_scan.py - discover vtables and string xrefs in a stripped x64 PE.

Evidence tooling for the NvCapture lane (read-only on the target file).

Method:
  1. .pdata RUNTIME_FUNCTION entries give exact function boundaries on x64.
  2. Every function is disassembled with capstone; rip-relative string refs
     are collected -> string-substring -> function index.
  3. .rdata/.data are scanned for runs of qwords pointing into .text
     (resolving ILT E9 thunks) -> vtable candidates with slot maps.

Modes:
  find <substring>            - functions whose disasm references a string
                                containing <substring>
  vtables [--min N]           - list vtable candidates (runs of >= N text ptrs)
  vt <vtable_rva> [n]         - dump one vtable with labels
  labels <r1> <r2> ...        - label functions by RVA
"""
import struct
import sys
import re

sys.path.insert(0, r"C:/My Project/NVIDIA-Shadowplay-gfe/NvCapture/abi-evidence/tools")
from abi_probe import PE  # noqa: E402
from capstone import Cs, CS_ARCH_X86, CS_MODE_64  # noqa: E402


def parse_pdata(pe):
    va, size = pe.dd[3]  # exception directory
    off = pe.rva2off(va)
    fns = []
    if off is None:
        return fns
    for i in range(size // 12):
        start, end, unw = struct.unpack_from("<III", pe.data, off + i * 12)
        if start == 0:
            break
        fns.append((start, end))
    return fns


def resolve_thunk(pe, rva, depth=8):
    for _ in range(depth):
        b = pe.read(rva, 5)
        if b and b[0] == 0xE9 and len(b) == 5:
            rel = int.from_bytes(b[1:5], "little", signed=True)
            rva = rva + 5 + rel
        else:
            break
    return rva


class Index:
    def __init__(self, pe):
        self.pe = pe
        self.fns = parse_pdata(pe)
        self.fn_starts = [s for s, e in self.fns]
        self.str_refs = {}   # fn_rva -> [(rva_target, string)]
        md = Cs(CS_ARCH_X86, CS_MODE_64)
        for s, e in self.fns:
            code = pe.read(s, e - s)
            if not code:
                continue
            refs = []
            for ins in md.disasm(code, pe.image_base + s):
                m = re.search(r"rip ([+-]) (0x[0-9a-f]+)", ins.op_str)
                if m:
                    sign = 1 if m.group(1) == "+" else -1
                    tgt = ins.address + ins.size + sign * int(m.group(2), 16) - pe.image_base
                    st = pe.string_at_or_near(tgt)
                    if st:
                        refs.append((tgt, st))
            if refs:
                self.str_refs[s] = refs

    def fn_of_rva(self, rva):
        i = self.pe and None
        import bisect
        i = bisect.bisect_right(self.fn_starts, rva) - 1
        if i >= 0:
            s, e = self.fns[i]
            if s <= rva < e:
                return s
        return None

    def find_string(self, needle):
        hits = []
        for s, refs in self.str_refs.items():
            for tgt, st in refs:
                if needle.lower() in st.lower():
                    hits.append((s, tgt, st))
        return hits

    def vtable_candidates(self, minn=3):
        pe = self.pe
        cands = []
        for secname in (".rdata", ".data"):
            for name, va, vsz, roff, rsz in pe.sections:
                if name != secname:
                    continue
                blob = pe.data[roff:roff + rsz]
                run = []
                run_start = None
                for i in range(0, len(blob) - 7, 8):
                    q = struct.unpack_from("<Q", blob, i)[0]
                    r = q - pe.image_base
                    isrva = 0x1000 <= r < 0x300000 and pe.sec_of_rva(r) == ".text"
                    if isrva:
                        if not run:
                            run_start = va + i
                        run.append(va + i)
                    else:
                        if len(run) >= minn:
                            cands.append((run_start, run))
                        run = []
                        run_start = None
                if len(run) >= minn:
                    cands.append((run_start, run))
        return cands


def slot_label(idx, fn_rva):
    frva = resolve_thunk(idx.pe, fn_rva)
    refs = idx.str_refs.get(frva) or idx.str_refs.get(fn_rva) or []
    if refs:
        return refs[0][1][:90]
    return "(no string xref)"


def main():
    pe = PE(sys.argv[1])
    idx = Index(pe)
    mode = sys.argv[2]
    if mode == "find":
        for s, tgt, st in idx.find_string(sys.argv[3]):
            print("fn RVA 0x%06X  str@0x%06X  %s" % (s, tgt, st[:100]))
    elif mode == "vtables":
        minn = int(sys.argv[3]) if len(sys.argv) > 3 else 3
        for start, slots in idx.vtable_candidates(minn):
            print("vtable candidate RVA 0x%06X  (%d slots)" % (start, len(slots)))
    elif mode == "vt":
        rva = int(sys.argv[3], 16)
        n = int(sys.argv[4]) if len(sys.argv) > 4 else 24
        for i in range(n):
            q = pe.qwords(rva + i * 8, 1)[0]
            if q == 0:
                print("  [%2d] NULL" % i)
                continue
            r = q - pe.image_base
            if pe.sec_of_rva(r) != ".text":
                print("  [%2d] 0x%X %s" % (i, q, pe.sec_of_rva(r)))
                continue
            fr = resolve_thunk(pe, r)
            print("  [%2d] thunk 0x%06X -> fn 0x%06X  %s" % (i, r, fr, slot_label(idx, r)))
    elif mode == "labels":
        for rva in sys.argv[3:]:
            r = int(rva, 16)
            fr = resolve_thunk(pe, r)
            print("  0x%06X (fn 0x%06X)  %s" % (r, fr, slot_label(idx, r)))


if __name__ == "__main__":
    main()
