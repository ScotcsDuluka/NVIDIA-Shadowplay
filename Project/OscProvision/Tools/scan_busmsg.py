import re, sys

path = sys.argv[1]
data = open(path, 'rb').read()

i = data.find(b'BusMessage.proto')
print('descriptor blob at', hex(i), 'total', len(data))

# window around the descriptor pool where BusMessage.proto lives
lo, hi = max(0, i - 3000), min(len(data), i + 6000)
blob = data[lo:hi]

strings = []
for m in re.finditer(rb'[A-Za-z_][A-Za-z0-9_.]{2,80}', blob):
    strings.append((m.start() + lo, m.group().decode()))

# print in order, dedup consecutively
prev = None
for off, s in strings:
    if s != prev:
        print(f'{off:#08x}  {s}')
    prev = s
