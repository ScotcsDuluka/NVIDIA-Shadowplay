import re, sys

path = sys.argv[1]
data = open(path, 'rb').read()

# ASCII strings >= 6 chars
strings = set(m.decode() for m in re.findall(rb'[ -~]{6,120}', data))

def show(title, pred, limit=50):
    hits = sorted(s for s in strings if pred(s))
    print(f'=== {title} ({len(hits)}) ===')
    for s in hits[:limit]:
        print(' ', s)
    print()

show('PIPE PATHS', lambda s: 'pipe' in s.lower() and ('\\' in s or '/' in s))
show('MESSAGEBUS NAMES', lambda s: 'messagebus' in s.lower() or 'MessageBus' in s)
show('PROTOBUF DESCRIPTORS', lambda s: '.proto' in s.lower() or 'google.protobuf' in s)
show('TOPIC-ISH (dotted)', lambda s: s.count('.') >= 1 and any(
    k in s.lower() for k in ('topic', 'shadowplay', 'overlay', 'hotkey', 'record',
                             'instantreplay', 'broadcast', 'session', 'subscribe',
                             'publish', 'state', 'event')))
show('CEF PLUGIN BITS', lambda s: 'cef' in s.lower() or 'plugin' in s.lower(), 40)
