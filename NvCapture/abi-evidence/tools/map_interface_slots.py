#!/usr/bin/env python3
"""map_interface_slots.py - map versioned interface tables to method labels.

Reads an annotated disasm dump (from abi_probe.py disasm), finds version
branches (cmp reg, imm) followed by sequences of:

    lea  rax, [rip + D]      ; function pointer target
    mov  qword ptr [rip + D2], rax   ; store into .data interface table

and closes each branch with `mov qword ptr [rdx], rax` (interface ptr store).

For each collected function RVA, disassembles the function head and derives
a label from the first referenced string literal (log tag).

Usage: python map_interface_slots.py <file.dll> <disasm_dump.txt> [--head N]
"""
import re
import sys

sys.path.insert(0, r"C:/My Project/NVIDIA-Shadowplay-gfe/NvCapture/abi-evidence/tools")
from abi_probe import PE, disasm  # noqa: E402

RE_CMP = re.compile(r"^  0x([0-9A-F]+)  cmp\s+(e[bcds][xhl]|\w+), (0x[0-9a-f]+)")
RE_LEA = re.compile(r"^  0x([0-9A-F]+)  lea\s+\w+, \[rip ([+-]) (0x[0-9a-f]+)\]")
RE_STG = re.compile(r"^  0x([0-9A-F]+)  mov\s+qword ptr \[rip ([+-]) (0x[0-9a-f]+)\], rax")
RE_OUT = re.compile(r"^  0x([0-9A-F]+)  mov\s+qword ptr \[rdx\], rax")
RE_STR = re.compile(r'; str "(.*)"')


def label_for(pe, fn_rva, head=400):
    # resolve ILT jmp thunks (E9 rel32) before labeling
    for _ in range(8):
        b = pe.read(fn_rva, 5)
        if b and b[0] == 0xE9 and len(b) == 5:
            rel = int.from_bytes(b[1:5], "little", signed=True)
            fn_rva = fn_rva + 5 + rel
        else:
            break
    lines = disasm(pe, fn_rva, head)
    for ln in lines:
        m = RE_STR.search(ln)
        if m:
            return m.group(1)[:80]
        if "iat" in ln:
            m2 = re.search(r"iat (\S+)", ln)
            if m2:
                return "iat-call " + m2.group(1)
    return "(no string xref in first %d insns)" % head


def main():
    path, dump = sys.argv[1], sys.argv[2]
    pe = PE(path)
    base = pe.image_base
    lines = open(dump, encoding="utf-8", errors="replace").read().splitlines()

    branches = []  # (ver, [(slot_target_rva, store_rva)...], out_ptr_rva)
    cur = None
    pending_lea = None
    for ln in lines:
        m = RE_CMP.match(ln)
        if m:
            if cur and cur[1]:
                branches.append(cur)
            cur = [int(m.group(3), 16), [], None]
            pending_lea = None
            continue
        m = RE_LEA.match(ln)
        if m and cur is not None:
            sign = 1 if m.group(2) == "+" else -1
            pending_lea = int(m.group(1), 16) + 7 + sign * int(m.group(3), 16)
            continue
        m = RE_STG.match(ln)
        if m and cur is not None and pending_lea is not None:
            cur[1].append(pending_lea - base)
            pending_lea = None
            continue
        m = RE_OUT.match(ln)
        if m and cur is not None:
            # the lea rax immediately before this store points at the
            # .data interface table returned to the caller
            if pending_lea is not None:
                cur[2] = pending_lea - base
            branches.append(cur)
            cur = None
            pending_lea = None
    if cur and cur[1]:
        branches.append(cur)

    # recompute out ptr for each branch: find `lea rax,[rip+D]` immediately
    # before `mov [rdx], rax` - handled inline above via RE_OUT next-insn math.
    for ver, fns, outp in branches:
        print("=" * 100)
        print("VERSION 0x%X  (%d slots)  interface-table RVA ~0x%X"
              % (ver, len(fns), outp or 0))
        for i, frva in enumerate(fns):
            lab = label_for(pe, frva)
            print("  [%2d] fn RVA 0x%06X  %s" % (i, frva, lab))


if __name__ == "__main__":
    main()
