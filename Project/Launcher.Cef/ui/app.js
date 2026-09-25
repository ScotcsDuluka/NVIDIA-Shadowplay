/* NVIDIA ShadowPlay Launcher — page logic.
   RPC: window.cefQuery (the osc page contract) carrying JSON requests,
   LAUNCHER_* command namespace. State arrives two ways:
     - host push: window.__LauncherState(stateObject)  (1s supervisor poll)
     - pull: LAUNCHER_GET_STATE every 2s (fallback when pushes stall)

   NULL-SAFE BY CONTRACT: the HTML is owner-editable (elements may be
   removed/renamed freely). Every lookup and every handler attach is
   guarded — a missing element must degrade that one feature, never kill
   the script (a single throw at load detaches EVERY button). */
(function () {
  'use strict';

  function $(id) { return document.getElementById(id); }

  // ── RPC bridge ──────────────────────────────────────────────────────
  var bridgeUp = typeof window.cefQuery === 'function';
  var bridgeTag = $('bridgeState');

  function rpc(command, extra) {
    return new Promise(function (resolve, reject) {
      if (!bridgeUp) { reject(new Error('no bridge')); return; }
      var req = { command: command };
      if (extra) {
        for (var k in extra) { if (extra.hasOwnProperty(k)) req[k] = extra[k]; }
      }
      window.cefQuery({
        request: JSON.stringify(req),
        persistent: false,
        onSuccess: function (response) {
          try { resolve(JSON.parse(response)); } catch (e) { resolve(response); }
        },
        onFailure: function (code, msg) { reject(new Error(code + ': ' + msg)); }
      });
    });
  }
  if (bridgeTag) {
    if (!bridgeUp) {
      bridgeTag.textContent = 'BRIDGE OFFLINE';
      bridgeTag.className = 'bridge off';
    } else {
      bridgeTag.textContent = 'BRIDGE LIVE';
    }
  }

  // ── Elements (any may be absent) ────────────────────────────────────
  var el = {
    dotOverlay: $('dotOverlay'),
    dotNotifier: $('dotNotifier'),
    dotNvApi: $('dotNvApi'),
    stateOverlay: $('stateOverlay'),
    stateNotifier: $('stateNotifier'),
    stateNvApi: $('stateNvApi'),
    chipEngine: $('chipEngine'),
    chipCef: $('chipCef'),
    chipHub: $('chipHub'),
    tglOverlay: $('tglOverlay'),
    tglEngine: $('tglEngine'),
    lblWinform: $('lblWinform'),
    lblCef: $('lblCef'),
    modeDesc: $('modeDesc'),
    btnOpenOverlay: $('btnOpenOverlay'),
    btnObt3: $('btnObt3'),
    btnExit: $('btnExit'),
    btnMin: $('btnMin'),
    btnClose: $('btnClose')
  };

  // Guarded event attach: a missing element skips silently.
  function on(node, ev, fn) {
    if (node) node.addEventListener(ev, fn);
  }

  function setDot(dot, stateEl, cls, label) {
    if (!dot) return;
    dot.className = 'dot' + (cls ? ' ' + cls : '');
    if (stateEl) stateEl.textContent = label;
  }

  // ── Render ──────────────────────────────────────────────────────────
  function render(s) {
    if (!s) return;

    // OVERLAY API: running+ready -> RUNNING, running only -> LOADING,
    // else STOPPED (Main.vb IF_APP_Tick contract).
    if (s.overlayApi && s.overlayApi.running && s.overlayApi.ready) {
      setDot(el.dotOverlay, el.stateOverlay, 'running', 'RUNNING');
    } else if (s.overlayApi && s.overlayApi.running) {
      setDot(el.dotOverlay, el.stateOverlay, 'loading', 'LOADING');
    } else {
      setDot(el.dotOverlay, el.stateOverlay, '', 'STOPPED');
    }

    setDot(el.dotNotifier, el.stateNotifier,
      (s.notifierApi && s.notifierApi.running) ? 'running' : '',
      (s.notifierApi && s.notifierApi.running) ? 'RUNNING' : 'STOPPED');
    setDot(el.dotNvApi, el.stateNvApi,
      (s.nvApi && s.nvApi.running) ? 'running' : '',
      (s.nvApi && s.nvApi.running) ? 'RUNNING' : 'STOPPED');

    var lanes = s.lanes || {};
    if (el.chipEngine) el.chipEngine.className = 'lane-chip' + (lanes.container ? ' on' : '');
    if (el.chipCef) el.chipCef.className = 'lane-chip' + (lanes.cefOverlay ? ' on' : '');
    if (el.chipHub) el.chipHub.className = 'lane-chip' + ((s.nvApi && s.nvApi.running) ? ' on' : '');

    syncToggle(el.tglOverlay, s.overlayEnabled);
    syncMode(!!s.engineOverlay);
  }

  function syncToggle(t, on) {
    if (!t || typeof on !== 'boolean') return;
    if (t.dataset.userHold === '1') {
      // User just interacted; the authoritative 1s push re-syncs later.
      if (t.dataset.pending === String(on)) t.dataset.userHold = '';
      return;
    }
    t.classList.toggle('on', on);
    t.setAttribute('aria-checked', on ? 'true' : 'false');
    t.dataset.pending = String(on);
  }

  // Overlay Mode switch: false = WINFORM lane, true = CEF lane.
  function syncMode(cef) {
    if (!el.tglEngine) return;
    if (el.tglEngine.dataset.userHold === '1') {
      if (el.tglEngine.dataset.pending === String(cef)) el.tglEngine.dataset.userHold = '';
      return;
    }
    applyMode(cef);
    el.tglEngine.dataset.pending = String(cef);
  }

  function applyMode(cef) {
    el.tglEngine.classList.toggle('cef', cef);
    el.tglEngine.setAttribute('aria-checked', cef ? 'true' : 'false');
    if (el.lblWinform) el.lblWinform.classList.toggle('active', !cef);
    if (el.lblCef) el.lblCef.classList.toggle('active', cef);
    if (el.modeDesc) {
      el.modeDesc.textContent = cef
        ? 'CEF chain: Container \u2192 Web Helper \u2192 Share (:59001)'
        : 'WinForm family \u2014 hub-managed overlay';
    }
  }

  // ── Host push (launcher_script.h augmentation) ──────────────────────
  window.__onLauncherState(function (s) { render(s); });

  // Pull fallback: never let the page sit on stale state.
  function pull() { rpc('LAUNCHER_GET_STATE').then(render).catch(function () {}); }
  pull();
  setInterval(pull, 2000);

  // ── Toggles (user action only — config.json is the source of truth) ─
  on(el.tglOverlay, 'click', function () {
    var on = !this.classList.contains('on');
    this.dataset.userHold = '1';
    this.dataset.pending = String(on);
    this.classList.toggle('on', on);
    this.setAttribute('aria-checked', on ? 'true' : 'false');
    rpc('LAUNCHER_SET_OVERLAY', { value: on }).catch(function () {});
  });

  // Overlay Mode: click the switch or either label.
  function setMode(cef) {
    el.tglEngine.dataset.userHold = '1';
    el.tglEngine.dataset.pending = String(cef);
    applyMode(cef);
    rpc('LAUNCHER_SET_ENGINE_OVERLAY', { value: cef }).catch(function () {});
  }
  on(el.tglEngine, 'click', function () {
    setMode(!this.classList.contains('cef'));
  });
  on(el.lblWinform, 'click', function () {
    if (!el.tglEngine.classList.contains('cef')) return;  // already WINFORM
    setMode(false);
  });
  on(el.lblCef, 'click', function () {
    if (el.tglEngine.classList.contains('cef')) return;  // already CEF
    setMode(true);
  });

  // ── Buttons ─────────────────────────────────────────────────────────
  on(el.btnOpenOverlay, 'click', function () {
    var b = this;
    rpc('LAUNCHER_OPEN_OVERLAY').then(function (r) {
      var ok = r && r.ok;
      var old = b.textContent;
      b.textContent = ok ? 'SENT' : 'HUB OFFLINE';
      setTimeout(function () { b.textContent = old; }, 1200);
    }).catch(function () {});
  });

  on(el.btnObt3, 'click', function () {
    rpc('LAUNCHER_OPEN_OBT3').catch(function () {});
  });

  // EXIT ALL is armed by a first click (old RadioButton2 kills the whole
  // family — a single accidental click must not do that).
  var exitArm = null;
  on(el.btnExit, 'click', function () {
    var b = this;
    if (b.classList.contains('armed')) {
      clearTimeout(exitArm);
      b.classList.remove('armed');
      b.textContent = 'EXITING';
      rpc('LAUNCHER_EXIT_ALL').catch(function () {});
      return;
    }
    b.classList.add('armed');
    b.textContent = 'CLICK AGAIN';
    exitArm = setTimeout(function () {
      b.classList.remove('armed');
      b.textContent = 'EXIT ALL';
    }, 3000);
  });

  on(el.btnMin, 'click', function () {
    rpc('LAUNCHER_MINIMIZE').catch(function () {});
  });
  on(el.btnClose, 'click', function () {
    rpc('LAUNCHER_CLOSE').catch(function () {});
  });

  // ── Window drag (titlebar mousedown, buttons excluded) ──────────────
  var titlebar = $('titlebar');
  if (titlebar) {
    titlebar.addEventListener('mousedown', function (e) {
      if (e.button !== 0) return;
      if (e.target.closest && e.target.closest('button')) return;
      rpc('LAUNCHER_DRAG').catch(function () {});
    });
  }
})();
