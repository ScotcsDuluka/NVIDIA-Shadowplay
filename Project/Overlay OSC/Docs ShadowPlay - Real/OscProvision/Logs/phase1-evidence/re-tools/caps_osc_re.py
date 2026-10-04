#!/usr/bin/env python3
# RE tool (read-only) — locate COscProcMgr::GetOSCPath / CheckOscPath / StartProcess in _nvspcaps64.dll
# Usage: python caps_osc_re.py <dll> <cmd> ...
import sys, re, struct
try:
    import capstone
except ImportError:
    capstone = None

def parse_pe(data):
    pe = struct.unpack_from("<I", data, 0x3C)[0]
    assert data[pe:pe+4] == b"PE\0\0"
    nsec = struct.unpack_from("<H", data, pe+6)[0]
    opt_size = struct.unpack_from("<H", data, pe+20)[0]
    opt = pe + 24
    image_base = struct.unpack_from("<Q", data, opt+24)[0]
    secs = []
    so = opt + opt_size
    for i in range(nsec):
        name = data[so:so+8].rstrip(b"\0").decode(errors="replace")
        vsize, va, rsize, raw = struct.unpack_from("<IIII", data, so+8)
        secs.append(dict(name=name, va=va, vsize=vsize, raw=raw, rsize=rsize))
        so += 40
    return image_base, secs

def off2va(data, secs, image_base, off):
    for s in secs:
        if s["raw"] <= off < s["raw"] + s["rsize"]:
            return image_base + s["va"] + (off - s["raw"])
    return None

def va2off(secs, image_base, va):
    rva = va - image_base
    for s in secs:
        if s["va"] <= rva < s["va"] + max(s["vsize"], s["rsize"]):
            if rva - s["va"] < s["rsize"]:
                return s["raw"] + (rva - s["va"])
    return None

def find_xrefs(data, secs, image_base, target_va):
    text = next(s for s in secs if s["name"].startswith(".text"))
    lo, hi = text["raw"], text["raw"] + text["rsize"]
    hits = []
    # LEA r64, [rip+disp32]: 48/4C 8D /r with mod=00 rm=101
    for m in re.finditer(rb"[\x48\x4c]\x8d[\x05\x0d\x15\x1d\x25\x2d\x35\x3d]", data[lo:hi]):
        off = lo + m.start()
        disp = struct.unpack_from("<i", data, off+3)[0]
        va_insn = off2va(data, secs, image_base, off)
        if va_insn is None: continue
        tgt = va_insn + 7 + disp
        if tgt == target_va:
            hits.append((off, va_insn))
    return hits

def disas(data, secs, image_base, off, back=0x60, fwd=0x140):
    if capstone is None: raise SystemExit("capstone needed")
    md = capstone.Cs(capstone.CS_ARCH_X86, capstone.CS_MODE_64)
    md.detail = False
    start = off - back
    code = data[start:off+fwd]
    out = []
    for insn in md.disasm(code, off2va(data, secs, image_base, start)):
        marker = ">>" if insn.address == off2va(data, secs, image_base, off) else "  "
        # annotate rip-relative string refs
        extra = ""
        m = re.search(r"rip \+ (0x[0-9a-f]+)|rip - (0x[0-9a-f]+)", insn.op_str)
        if m:
            d = int(m.group(1),16) if m.group(1) else -int(m.group(2),16)
            tgt = insn.address + insn.size + d
            o2 = va2off(secs, image_base, tgt)
            if o2:
                raw = data[o2:o2+80]
                s16 = raw.split(b"\0\0")[0]
                try: w = s16.decode("utf-16-le", errors="ignore")
                except: w = ""
                a = raw.split(b"\0")[0]
                try: an = a.decode("ascii", errors="ignore")
                except: an = ""
                if len(w) >= 4: extra = f'   ; W"{w}"'
                elif len(an) >= 4: extra = f'   ; A"{an}"'
        out.append(f"{marker}{insn.address:#x}: {insn.mnemonic:<9s} {insn.op_str}{extra}")
    return "\n".join(out)

if __name__ == "__main__":
    path = sys.argv[1]; cmd = sys.argv[2]
    data = open(path, "rb").read()
    ib, secs = parse_pe(data)
    print(f"# {path} image_base={ib:#x} sections=" + ", ".join(f'{s["name"]}@va:{s["va"]:#x}/raw:{s["raw"]:#x}' for s in secs))
    if cmd == "xref":
        for a in sys.argv[3:]:
            va = int(a, 16)
            hits = find_xrefs(data, secs, ib, va)
            print(f"xref -> {va:#x}: " + (", ".join(f"off:{o:#x}/va:{v:#x}" for o, v in hits) or "NONE"))
    elif cmd == "dis":
        off = int(sys.argv[3], 16)
        print(disas(data, secs, ib, off))
