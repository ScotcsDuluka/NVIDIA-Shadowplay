using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Duluka.Server.Manager;

/// <summary>
/// Central skin + motion helper — the same "glass dark" language as the
/// Overlay (cooler graphite stack, NVIDIA green accent, rounded pills,
/// tweened hover/press, soft fade-in, pulsing status dot).
/// Self-contained GDI+; a single 15 ms engine timer drives all tweens.
/// </summary>
public static class UiTheme
{
    // ── palette (kept in sync with Overlay UiTheme.vb) ──────────────────────
    public static readonly Color Accent      = Color.FromArgb(118, 185, 0);   // #76B900
    public static readonly Color AccentHover = Color.FromArgb(146, 220, 20);
    public static readonly Color SurfaceDeep = Color.FromArgb(25, 30, 35);
    public static readonly Color SurfaceBase = Color.FromArgb(31, 38, 45);
    public static readonly Color SurfaceRaised = Color.FromArgb(41, 49, 58);
    public static readonly Color SurfaceHover  = Color.FromArgb(54, 64, 75);
    public static readonly Color SurfaceTop    = Color.FromArgb(64, 75, 87);
    public static readonly Color TextBright  = Color.FromArgb(233, 239, 244);
    public static readonly Color TextMuted   = Color.FromArgb(146, 161, 173);
    public static readonly Color Danger      = Color.FromArgb(240, 97, 109);
    public static readonly Color LogBg       = Color.FromArgb(14, 18, 22);

    // ── tween engine ────────────────────────────────────────────────────────
    private sealed class Job
    {
        public Control Ctrl = null!;
        public Color From, To;
        public int Ms, T;
        public Action<Color> Apply = null!;
    }

    private static readonly List<Job> Jobs = new();
    private static System.Windows.Forms.Timer? _engine;

    public static Color Lerp(Color a, Color b, double t)
    {
        t = Math.Max(0.0, Math.Min(1.0, t));
        return Color.FromArgb(
            (int)(a.A + (b.A - a.A) * t),
            (int)(a.R + (b.R - a.R) * t),
            (int)(a.G + (b.G - a.G) * t),
            (int)(a.B + (b.B - a.B) * t));
    }

    public static Color Shade(Color c, float f) => Color.FromArgb(c.A,
        Math.Max(0, (int)(c.R * f)), Math.Max(0, (int)(c.G * f)), Math.Max(0, (int)(c.B * f)));

    public static void Tween(Control c, Color to, int ms = 140) =>
        TweenApply(c, to, ms, col => c.BackColor = col, () => c.BackColor);

    /// <summary>Tween with a custom applier — used for flat buttons where the
    /// hover face comes from MouseOverBackColor (must move with the tween).</summary>
    public static void TweenApply(Control c, Color to, int ms, Action<Color> apply, Func<Color>? get = null)
    {
        if (c is null || c.IsDisposed) return;
        var from = get is not null ? get() : c.BackColor;
        Jobs.RemoveAll(j => ReferenceEquals(j.Ctrl, c));
        if (from.ToArgb() == to.ToArgb()) { apply(to); return; }
        Jobs.Add(new Job { Ctrl = c, From = from, To = to, Ms = Math.Max(30, ms), T = 0, Apply = apply });
        EnsureEngine();
    }

    private static void EnsureEngine()
    {
        if (_engine is not null) { if (!_engine.Enabled) _engine.Start(); return; }
        _engine = new System.Windows.Forms.Timer { Interval = 15 };
        _engine.Tick += (_, _) =>
        {
            if (Jobs.Count == 0) { _engine.Stop(); return; }
            List<Job>? done = null;
            foreach (var j in Jobs)
            {
                j.T += _engine.Interval;
                var t = Math.Min(1.0, j.T / (double)j.Ms);
                var eased = 1.0 - Math.Pow(1.0 - t, 3);
                if (j.Ctrl.IsDisposed || j.Ctrl.Disposing) { (done ??= new()).Add(j); continue; }
                var col = Lerp(j.From, j.To, eased);
                j.Apply(col);
                if (t >= 1.0) { j.Apply(j.To); (done ??= new()).Add(j); }
            }
            if (done is not null) foreach (var j in done) Jobs.Remove(j);
            if (Jobs.Count == 0) _engine.Stop();
        };
        _engine.Start();
    }

    // ── hover / press (for flat buttons whose MouseOverBackColor is pinned) ─
    private static readonly Dictionary<Control, (Color Hover, Color Idle)> HoverMap = new();

    public static void AttachHover(Control c, Color hover, Color? idle = null,
        Action<Color>? apply = null, Func<Color>? get = null)
    {
        HoverMap[c] = (hover, idle ?? (get is not null ? get() : c.BackColor));
        Action<Color> ap = apply ?? (col => c.BackColor = col);
        c.MouseEnter += (_, _) => { if (HoverMap.TryGetValue(c, out var s)) TweenApply(c, s.Hover, 130, ap, get); };
        c.MouseLeave += (_, _) => { if (HoverMap.TryGetValue(c, out var s)) TweenApply(c, s.Idle, 180, ap, get); };
        c.MouseDown  += (_, _) => TweenApply(c, Shade(hover, 0.85f), 60, ap, get);
        c.MouseUp    += (_, _) =>
        {
            if (!HoverMap.TryGetValue(c, out var s)) return;
            var over = c.ClientRectangle.Contains(c.PointToClient(Cursor.Position));
            TweenApply(c, over ? s.Hover : s.Idle, 90, ap, get);
        };
    }

    // ── rounded pill / card regions (kept correct across resizes) ───────────
    private static GraphicsPath RoundPath(Rectangle rc, int r)
    {
        var path = new GraphicsPath();
        int d = Math.Max(2, r * 2);
        if (d > rc.Width) d = rc.Width;
        if (d > rc.Height) d = rc.Height;
        path.AddArc(rc.Left, rc.Top, d, d, 180, 90);
        path.AddArc(rc.Right - d, rc.Top, d, d, 270, 90);
        path.AddArc(rc.Right - d, rc.Bottom - d, d, d, 0, 90);
        path.AddArc(rc.Left, rc.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    public static void Round(Control c, Func<int>? radius = null)
    {
        void Apply()
        {
            if (c.IsDisposed || c.Width < 4 || c.Height < 4) return;
            int r = radius is not null ? radius() : Math.Max(6, Math.Min(14, Math.Min(c.Width, c.Height) / 4));
            using var path = RoundPath(new Rectangle(0, 0, c.ClientSize.Width - 1, c.ClientSize.Height - 1), r);
            var old = c.Region;
            c.Region = new Region(path);
            old?.Dispose();
        }
        Apply();
        c.Resize += (_, _) => Apply();
    }

    /// <summary>1px glass edge + top highlight painted over any panel.</summary>
    public static void GlassEdge(Control c, Func<int>? radius = null)
    {
        c.Paint += (_, e) =>
        {
            try
            {
                int w = c.ClientSize.Width, h = c.ClientSize.Height;
                if (w < 12 || h < 12) return;
                int r = radius is not null ? radius() : Math.Max(6, Math.Min(14, Math.Min(w, h) / 4));
                using var path = RoundPath(new Rectangle(0, 0, w - 1, h - 1), r);
                using (var edge = new Pen(Color.FromArgb(28, 255, 255, 255)))
                    e.Graphics.DrawPath(edge, path);
                using (var top = new Pen(Color.FromArgb(46, 255, 255, 255)))
                    e.Graphics.DrawLine(top, r, 1, w - 1 - r, 1);
            }
            catch { /* decoration must never crash the form */ }
        };
        c.Invalidate();
    }

    // ── form fade-in ────────────────────────────────────────────────────────
    public static void FadeIn(Form f)
    {
        var target = f.Opacity >= 1.0 ? 1.0 : f.Opacity;
        if (f.Opacity > 0.001) f.Opacity = 0.0001;
        System.Windows.Forms.Timer? tmr = null;
        tmr = new System.Windows.Forms.Timer { Interval = 10 };
        tmr.Tick += (_, _) =>
        {
            if (f.IsDisposed || f.Disposing) { tmr.Stop(); tmr.Dispose(); return; }
            var op = f.Opacity + (target - f.Opacity) * 0.24;
            if (Math.Abs(target - op) < 0.02) { f.Opacity = target; tmr.Stop(); tmr.Dispose(); }
            else f.Opacity = op;
        };
        f.Shown += (_, _) => { if (!f.IsDisposed) { f.Opacity = 0.0001; tmr.Stop(); tmr.Start(); } };
    }

    // ── status dot pulse (running indicator) ────────────────────────────────
    public static System.Windows.Forms.Timer StartPulse(Control dot, Color a, Color b, int ms = 650)
    {
        var bright = false;
        var tmr = new System.Windows.Forms.Timer { Interval = ms };
        tmr.Tick += (_, _) => { bright = !bright; Tween(dot, bright ? a : b, (int)(ms * 0.8)); };
        return tmr;
    }
}
