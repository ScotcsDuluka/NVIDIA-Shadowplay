import re
data = open(r"C:/My Project/GFE/GeForce_Experience_v3.28.0.412/NVI2/NVI2.dll", 'rb').read()
hits = set()
for m in re.finditer(rb'[ -~]{5,140}', data):
    s = m.group().decode()
    low = s.lower()
    if low.endswith('.log') or (('log' in low) and any(k in low for k in ('temp', 'programdata', 'nvi2', 'installer', '\\'))):
        hits.add(s)
for s in sorted(hits)[:50]:
    print(s)
