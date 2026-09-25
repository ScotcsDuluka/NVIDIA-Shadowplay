/* NVIDIA ShadowPlay Launcher — page logic.
   RPC: window.cefQuery (the osc page contract) carrying JSON requests,
   LAUNCHER_* command namespace. State arrives two ways:
     - host push: window.__LauncherState(stateObject)  (1s supervisor poll)
     - pull: LAUNCHER_GET_STATE every 2s (fallback when pushes stall)   */
(function () {
  'use strict';

  // ── RPC bridge ──────────────────────────────────────────────────────
  var bridgeUp = typeof window.cefQuery === 'function';
  var bridgeTag = document.getElementById('bridgeState');

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
  if (!bridgeUp) {
    bridgeTag.textContent = 'BRIDGE OFFLINE';
    bridgeTag.className = 'bridge off';
  } else {
    bridgeTag.textContent = 'BRIDGE LIVE';
  }

  // ── Elements ────────────────────────────────────────────────────────
  var el = {
    dotOverlay: document.getElementById('dotOverlay'),
    dotNotifier: document.getElementById('dotNotifier'),
    dotNvApi: document.getElementById('dotNvApi'),
    stateOverlay: document.getElementById('stateOverlay'),
    stateNotifier: document.getElementById('stateNotifier'),
    stateNvApi: document.getElementById('stateNvApi'),
    chipEngine: document.getElementById('chipEngine'),
    chipCef: document.getElementById('chipCef'),
    chipHub: document.getElementById('chipHub'),
    tglOverlay: document.getElementById('tglOverlay'),
    tglEngine: document.getElementById('tglEngine'),
    btnOpenOverlay: document.getElementById('btnOpenOverlay'),
    btnObt3: document.getElementById('btnObt3'),
    btnExit: document.getElementById('btnExit'),
    btnMin: document.getElementById('btnMin'),
    btnClose: document.getElementById('btnClose')
  };

  function setDot(dot, stateEl, cls, label) {
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
    el.chipEngine.className = 'meta-chip' + (lanes.container ? ' on' : '');
    el.chipCef.className = 'meta-chip' + (lanes.cefOverlay ? ' on' : '');
    el.chipHub.className = 'meta-chip' + ((s.nvApi && s.nvApi.running) ? ' on' : '');

    syncToggle(el.tglOverlay, s.overlayEnabled);
    syncToggle(el.tglEngine, s.engineOverlay);
  }

  function syncToggle(t, on) {
    if (typeof on !== 'boolean') return;
    if (t.dataset.userHold === '1') {
      // User just interacted; the authoritative 1s push re-syncs later.
      if (t.dataset.pending === String(on)) t.dataset.userHold = '';
      return;
    }
    t.classList.toggle('on', on);
    t.setAttribute('aria-checked', on ? 'true' : 'false');
    t.dataset.pending = String(on);
  }

  // ── Host push (launcher_script.h augmentation) ──────────────────────
  window.__onLauncherState(function (s) { render(s); });

  // Pull fallback: never let the page sit on stale state.
  function pull() { rpc('LAUNCHER_GET_STATE').then(render).catch(function () {}); }
  pull();
  setInterval(pull, 2000);

  // ── Toggles (user action only — config.json is the source of truth) ─
  function armToggle(t, on) {
    t.dataset.userHold = '1';
    t.dataset.pending = String(on);
    t.classList.toggle('on', on);
    t.setAttribute('aria-checked', on ? 'true' : 'false');
  }
  el.tglOverlay.addEventListener('click', function () {
    var on = !this.classList.contains('on');
    armToggle(this, on);
    rpc('LAUNCHER_SET_OVERLAY', { value: on }).catch(function () {});
  });
  el.tglEngine.addEventListener('click', function () {
    var on = !this.classList.contains('on');
    armToggle(this, on);
    rpc('LAUNCHER_SET_ENGINE_OVERLAY', { value: on }).catch(function () {});
  });

  // ── Buttons ─────────────────────────────────────────────────────────
  el.btnOpenOverlay.addEventListener('click', function () {
    var b = this;
    rpc('LAUNCHER_OPEN_OVERLAY').then(function (r) {
      var ok = r && r.ok;
      var old = b.textContent;
      b.textContent = ok ? 'SENT' : 'HUB OFFLINE';
      setTimeout(function () { b.textContent = old; }, 1200);
    }).catch(function () {});
  });

  el.btnObt3.addEventListener('click', function () {
    rpc('LAUNCHER_OPEN_OBT3').catch(function () {});
  });

  // EXIT ALL is armed by a first click (old RadioButton2 kills the whole
  // family — a single accidental click must not do that).
  var exitArm = null;
  el.btnExit.addEventListener('click', function () {
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

  el.btnMin.addEventListener('click', function () {
    rpc('LAUNCHER_MINIMIZE').catch(function () {});
  });
  el.btnClose.addEventListener('click', function () {
    rpc('LAUNCHER_CLOSE').catch(function () {});
  });

  // ── Window drag (titlebar mousedown, buttons excluded) ──────────────
  document.getElementById('titlebar').addEventListener('mousedown', function (e) {
    if (e.button !== 0) return;
    if (e.target.closest && e.target.closest('button')) return;
    rpc('LAUNCHER_DRAG').catch(function () {});
  });
})();
