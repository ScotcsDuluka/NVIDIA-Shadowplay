import io, subprocess

# original card block from d5310b1 (lines 257..292 in that file)
old = subprocess.run(
    ["git", "show", "d5310b1:Web/index.html"],
    cwd=r"C:\My Project\NVIDIA-Shadowplay", capture_output=True, text=True,
    encoding="utf-8").stdout.splitlines(keepends=True)
card = "".join(old[256:292])  # lines 257..292 (1-based) = index 256..292
assert "In-game Overlay Card" in card and "overlay.png" in card, card[:200]

p = r"C:\My Project\NVIDIA-Shadowplay\Web\index.html"
s = io.open(p, encoding="utf-8").read()

# 1. remove the overlay section created last commit (keep the download anchor)
sec_start = s.index("  <!-- ==================== OVERLAY SECTION ==================== -->")
anchor = "  <!-- Anchor for download section -->"
sec_end = s.index(anchor, sec_start) + len(anchor)
s = s[:sec_start] + anchor + s[sec_end:]

# 2. re-insert the original card as the last item of the hero grid
hero_close = """        </div>
      </div>
    </div>
  </section>

  <!-- Anchor for download section -->"""
assert hero_close in s, "hero close not found"
s = s.replace(hero_close, "        " + card.strip() + "\n" + hero_close, 1)

# 3. drop the Overlay nav link (its anchor lives inside the hero again)
s = s.replace('      <li><a href="#overlay">Overlay</a></li>\n', "", 1)

io.open(p, "w", encoding="utf-8", newline="").write(s)
print("original GAME READY / In-game Overlay card restored into hero")
