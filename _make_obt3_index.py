import io

src = r"C:\My Project\NVIDIA-Shadowplay\Web\index.html"
dst = r"C:\My Project\NVIDIA-Shadowplay\Web\index-obt3.html"
s = io.open(src, encoding="utf-8").read()

s = s.replace("<title>NEXTGEN NVIDIA ShadowPlay</title>",
              "<title>OBT3 IS LIVE — NVIDIA ShadowPlay</title>", 1)
s = s.replace('<title>NVIDIA ShadowPlay — OBT3 Coming Soon</title>',
              '<title>OBT3 IS LIVE — NVIDIA ShadowPlay</title>', 1)

old_inner = s[s.index('<div class="obt3-head">'):s.index('<div class="obt3-progress">')]
live_inner = '''<div class="obt3-head">
              <span class="obt3-tag" style="animation:obt3-blink 1.8s ease infinite">&#9679; LIVE</span>
              <span class="obt3-title">OBT3 — OPEN BETA TEST 3 IS HERE</span>
              <span class="obt3-sub">the audio-timeline rebuild is out now</span>
            </div>
            <div class="obt3-main">
              <a class="obt3-dl" href="obt3.html">
                <span class="obt3-dl-arrow">&#10022;</span>
                <span class="obt3-dl-txt">
                  <b>WHAT'S NEW — OBT3 vs STABLE</b>
                  <small>side-by-side comparison, measured not marketed</small>
                </span>
              </a>
            </div>
            '''
s = s.replace(old_inner, live_inner, 1)

i = s.index('<div class="obt3-meta">')
j = s.index('</div>', i) + len('</div>')
s = s[:i] + '<div class="obt3-meta"><span class="obt3-target">BUILD: OBT3 &middot; SEP 2026</span><span>SDK 26100 &middot; NVENC READY &middot; A/V 0.000s</span></div>' + s[j:]

# nav: add LIVE link into the links list
nav_anchor = '<li><a href="#features">Features</a></li>'
assert nav_anchor in s
s = s.replace(nav_anchor,
              '<li><a href="obt3.html" style="color:#76B900;font-weight:700;">OBT3 LIVE &#8594;</a></li>\n      ' + nav_anchor, 1)

# download section: OBT3 card first
dl_grid = s.index('class="dl"')
card_anchor = s.index('<div class="card rv">', dl_grid)
obt3_card = '''<div class="card rv" style="border-color:rgba(118,185,0,.5);box-shadow:0 0 40px rgba(118,185,0,.12)">
        <span class="tag b-ready">&#9679; LIVE — OPEN BETA TEST 3</span>
        <span class="ver">OBT3</span>
        <p><b>The current flagship build.</b> Audio-timeline rebuild (no tail loss), fixed video-only path, honest pass reporting, Intel QSV via FFmpeg path.</p>
        <a class="btn btn-g" style="justify-content:center" target="_blank"
           href="https://github.com/ScotcsDuluka/NVIDIA-Shadowplay/releases">Download OBT3 &#8595;</a>
        <a class="btn btn-o" style="justify-content:center" href="obt3.html">See OBT3 vs Stable &#8594;</a>
      </div>
      '''
s = s[:card_anchor] + obt3_card + s[card_anchor:]

# strip the countdown zero-state JS (no countdown on this page)
i = s.index('<script>\n(function(){\n  var TARGET')
j = s.index('</script>', i) + len('</script>')
s = s[:i] + '<script>/* countdown not used on the OBT3-live homepage */</script>' + s[j:]

io.open(dst, "w", encoding="utf-8", newline="").write(s)
print("index-obt3.html created")

# wire index.html zero-state -> index-obt3.html
p2 = r"C:\My Project\NVIDIA-Shadowplay\Web\index.html"
h = io.open(p2, encoding="utf-8").read()
old = 'el.pct.innerHTML=\'<a href="obt3.html"'
new = 'el.pct.innerHTML=\'<a href="index-obt3.html"'
if old in h:
    h = h.replace(old, new, 1)
    io.open(p2, "w", encoding="utf-8", newline="").write(h)
    print("index zero-state -> index-obt3.html")
else:
    print("zero-state link already points elsewhere — check")
