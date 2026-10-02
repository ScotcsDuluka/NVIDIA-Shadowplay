// NVIDIA ShadowPlay OSC — ของเราเอง (โครง+พฤติกรรมตาม template/config แท้ที่สกัดมา)
'use strict';

// ── state ─────────────────────────────────────────────
var IN_BROWSER = location.port !== '3000';
var backend = { port: 59001, secret: '' };

// tiles config — โครงเดียวกับ J.tiles ของแท้ (state machine + dropdown items)
var TILES = {
  InstantReplay: {
    states: {
      Off: { icon: 'icon-replay', status: 'Off', cls: '', items: [
        { t: 'Turn on', icon: 'icon-play', hk: 'Alt+Shift+F10', act: 'ir-on' },
        { div: 1 },
        { t: 'Settings', icon: 'icon-settings', act: 'settings' }
      ] },
      On: { icon: 'icon-replay', cls: 'icon-highlighted', status: 'On', cls2: 'highlighted', items: [
        { t: 'Turn off', icon: 'icon-stop', hk: 'Alt+Shift+F10', act: 'ir-off' },
        { t: 'Save', icon: 'icon-save', act: 'ir-save' },
        { div: 1 },
        { t: 'Settings', icon: 'icon-settings', act: 'settings' }
      ] }
    }
  },
  ManualRecord: {
    states: {
      Off: { icon: 'icon-record', status: 'Not recording', cls: '', items: [
        { t: 'Start', icon: 'icon-play', hk: 'Alt+F9', act: 'rec-on' },
        { div: 1 },
        { t: 'Settings', icon: 'icon-settings', act: 'settings' }
      ] },
      On: { icon: 'icon-record', cls: 'icon-highlighted', status: 'Recording', cls2: 'highlighted', items: [
        { t: 'Stop and save', icon: 'icon-stop', hk: 'Alt+F9', act: 'rec-off' },
        { div: 1 },
        { t: 'Settings', icon: 'icon-settings', act: 'settings' }
      ] }
    }
  },
  Mic: {
    states: {
      On: { icon: 'icon-mic_on', items: [
        { t: 'Push-to-talk', icon: 'icon-mic_ptt', act: 'mic-ptt' },
        { t: 'Always on', icon: 'icon-mic_on', act: 'mic-on' },
        { t: 'Off', icon: 'icon-mic_off', act: 'mic-off' },
        { div: 1 },
        { t: 'Settings', act: 'settings' }
      ] },
      Off: { icon: 'icon-mic_off', items: [
        { t: 'Always on', icon: 'icon-mic_on', act: 'mic-on' },
        { div: 1 },
        { t: 'Settings', act: 'settings' }
      ] }
    }
  },
  Camera: {
    states: {
      Off: { icon: 'icon-webcam_off', items: [
        { t: 'Turn on', icon: 'icon-webcam_on', act: 'cam-on' }
      ] },
      On: { icon: 'icon-webcam_on', items: [
        { t: 'Turn off', icon: 'icon-webcam_off', act: 'cam-off' }
      ] }
    }
  }
};
var tileState = { InstantReplay: 'Off', ManualRecord: 'Off', Mic: 'On', Camera: 'Off' };
var state = { open: false, openedAt: 0 };

// ── bridge ────────────────────────────────────────────
function oscQuery(cmd, persistent, cb) {
  if (IN_BROWSER || !window.cefQuery) return cb && cb('{}');
  window.cefQuery({ request: JSON.stringify(cmd), persistent: !!persistent,
    onSuccess: function (r) { cb && cb(r); },
    onFailure: function (e, m) { console.log('[osc] query fail: ' + m); cb && cb('{}'); }
  });
}

function rest(method, path, body, cb) {
  var xhr = new XMLHttpRequest();
  xhr.open(method, 'http://127.0.0.1:' + backend.port + path, true);
  xhr.setRequestHeader('Content-Type', 'application/json');
  if (backend.secret) xhr.setRequestHeader('X_LOCAL_SECURITY_COOKIE', backend.secret);
  xhr.onload = function () { try { cb && cb(JSON.parse(xhr.responseText)); } catch (e) { cb && cb({}); } };
  xhr.onerror = function () { cb && cb({}); };
  xhr.send(body ? JSON.stringify(body) : null);
}

// ── open / close ──────────────────────────────────────
function openOsc() {
  if (state.open) return;
  state.open = true;
  state.openedAt = Date.now();
  fitStage();
  document.body.classList.add('open');
  playFlip();
  oscQuery({ command: 'QUERY_WIN_OPEN_OSC' });
  setTimeout(sendRects, 500);
}
// ── สเกลเวที 1920x1200 ให้พอดีจอ (ทุกจอเห็นเหมือนกันหมด) ──
function fitStage() {
  var st = document.getElementById('stage');
  if (!st) return;
  var s = Math.min(window.innerWidth / 1920, window.innerHeight / 1200);
  st.style.transform = 'translate(-50%, -50%) scale(' + s + ')';
}
window.addEventListener('resize', fitStage);

// flip เข้าตอนเปิด (90°)
function playFlip() {
  var c = document.querySelector('.main-menu-container');
  if (!c) return;
  c.style.transition = 'none';
  c.style.transform = 'perspective(2400px) rotateY(90deg)';
  c.style.opacity = '0';
  void c.offsetWidth;
  c.style.transition = 'transform .45s cubic-bezier(.2,.8,.2,1), opacity .45s';
  c.style.transform = 'perspective(2400px) rotateY(0deg)';
  c.style.opacity = '1';
}
function closeOsc() {
  if (!state.open) return;
  if (Date.now() - state.openedAt < 400) return;   // กัน close-event ค้างจากรอบก่อน
  closeMenus();
  state.open = false;
  document.body.classList.remove('open');
  oscQuery({ command: 'QUERY_WIN_CLOSE_OSC' });
  oscQuery({ command: 'QUERY_OSC_SET_DISPLAY_RECTS', displayRects: [] });
}
function toggleOsc() { state.open ? closeOsc() : openOsc(); }

// ── dropdown menu ของไทล์ (แบบ md-menu แท้) ──────────
var openMenuTile = null;
function toggleTileMenu(name) {
  var el = document.querySelector('.menu-item[data-tile="' + name + '"]');
  if (!el) return;
  if (openMenuTile === name) { closeMenus(); return; }
  closeMenus();
  var dd = el.querySelector('.tile-dropdown');
  if (dd && TILES[name] && TILES[name].states[tileState[name]]) {
    dd.innerHTML = '';
    var st = TILES[name].states[tileState[name]];
    st.items.forEach(function (it) {
      if (it.div) { var d = document.createElement('div'); d.className = 'dd-divider'; dd.appendChild(d); return; }
      var a = document.createElement('div');
      a.className = 'dd-item';
      a.innerHTML = '<span class="dd-icon ' + (it.icon || '') + '"></span><span>' + it.t + '</span>' +
                    (it.hk ? '<span class="dd-hotkey">' + it.hk + '</span>' : '');
      a.addEventListener('click', function (ev) { ev.stopPropagation(); doAction(name, it.act); closeMenus(); });
      dd.appendChild(a);
    });
    el.classList.add('open');
    openMenuTile = name;
  }
}
function closeMenus() {
  document.querySelectorAll('.menu-item.open').forEach(function (e) { e.classList.remove('open'); });
  openMenuTile = null;
}

// ── actions (ตาม click functions ของแท้) ──────────────
function doAction(tile, act) {
  switch (act) {
    case 'ir-on':  rest('POST', '/ShadowPlay/v.1.0/InstantReplay/Enable', {}, function () { tileState.InstantReplay = 'On'; renderTile('InstantReplay'); }); break;
    case 'ir-off': rest('POST', '/ShadowPlay/v.1.0/InstantReplay/Enable', {}, function () { tileState.InstantReplay = 'Off'; renderTile('InstantReplay'); }); break;
    case 'ir-save': rest('POST', '/ShadowPlay/v.1.0/InstantReplay/Save', {}); break;
    case 'rec-on':  rest('POST', '/ShadowPlay/v.1.0/Record/Enable', {}, function () { tileState.ManualRecord = 'On'; renderTile('ManualRecord'); }); break;
    case 'rec-off': rest('POST', '/ShadowPlay/v.1.0/Record/Enable', {}, function () { tileState.ManualRecord = 'Off'; renderTile('ManualRecord'); }); break;
    case 'mic-on':  tileState.Mic = 'On'; renderTile('Mic'); break;
    case 'mic-off': tileState.Mic = 'Off'; renderTile('Mic'); break;
    case 'mic-ptt': tileState.Mic = 'PTT'; renderTile('Mic'); break;
    case 'cam-on':  tileState.Camera = 'On'; renderTile('Camera'); break;
    case 'cam-off': tileState.Camera = 'Off'; renderTile('Camera'); break;
    case 'screenshot': rest('POST', '/ShadowPlay/v.1.0/Screenshot/Capture', {}); break;
    case 'settings':
      showPage('settings');
      loadAllSettings();
      break;
    default: break;
  }
}

// ── render tile ตาม state machine ─────────────────────
function renderTile(name) {
  var cfg = TILES[name];
  if (!cfg || !cfg.states) return;
  var st = cfg.states[tileState[name]];
  if (name === 'InstantReplay') {
    var icIr = document.getElementById('ic-ir');
    var stIr = document.getElementById('st-ir');
    if (icIr) icIr.className = 'icon120 ' + (st.cls || 'icon-normal') + ' ' + st.icon;
    if (stIr) { stIr.className = st.cls2 || ''; stIr.textContent = st.status; }
    document.querySelectorAll('.bbtn[data-tile="InstantReplay"]').forEach(function (b) { b.classList.toggle('on', tileState.InstantReplay === 'On'); });
  }
  if (name === 'ManualRecord') {
    var icRec = document.getElementById('ic-rec');
    var stRec = document.getElementById('st-rec');
    if (icRec) icRec.className = 'icon120 ' + (st.cls || 'icon-normal') + ' ' + st.icon;
    if (stRec) { stRec.className = st.cls2 || ''; stRec.textContent = st.status; }
    document.querySelectorAll('.bbtn[data-tile="ManualRecord"]').forEach(function (b) { b.classList.toggle('rec-on', tileState.ManualRecord === 'On'); });
  }
  if (name === 'Mic') {
    var icMic = document.getElementById('ic-mic');
    if (icMic) icMic.className = 'icon48 icon-normal ' + st.icon;
  }
  if (name === 'Camera') {
    var icCam = document.getElementById('ic-cam');
    if (icCam) icCam.className = 'icon48 icon-normal ' + st.icon;
  }
}

function sendRects() {
  var rects = [];
  document.querySelectorAll('.menu-item, .bbtn, #btn-close').forEach(function (el) {
    var r = el.getBoundingClientRect();
    if (r.width > 4) rects.push({ x: Math.round(r.x), y: Math.round(r.y), width: Math.round(r.width), height: Math.round(r.height) });
  });
  oscQuery({ command: 'QUERY_OSC_SET_DISPLAY_RECTS', displayRects: rects });
}

function pollStatus() {
  rest('GET', '/ShadowPlay/v.1.0/Record/Running', null, function (r) {
    if (r && typeof r.running !== 'undefined') {
      var want = r.running ? 'On' : 'Off';
      if (tileState.ManualRecord !== want) { tileState.ManualRecord = want; renderTile('ManualRecord'); }
    }
  });
  setTimeout(pollStatus, 2000);
}

// ── input ─────────────────────────────────────────────
document.addEventListener('click', function (e) {
  var mi = e.target.closest('.menu-item');
  if (mi) {
    var name = mi.getAttribute('data-tile');
    if (name === 'Screenshot') { doAction('Screenshot', 'screenshot'); return; }
    if (TILES[name]) { toggleTileMenu(name); return; }
    return;
  }
  if (e.target.closest('#btn-close')) { closeOsc(); return; }
  if (!e.target.closest('.tile-dropdown')) closeMenus();
  if (!e.target.closest('.menu-item') && !e.target.closest('#topbar') && !e.target.closest('#bottombar')) closeOsc();
});
document.addEventListener('click', function (e) {
  if (e.target.closest('#ps-back')) { hidePages(); return; }
});
document.addEventListener('keydown', function (e) {
  if (e.key === 'Escape') { if (openMenuTile) { closeMenus(); return; } closeOsc(); }
});
document.addEventListener('contextmenu', function (e) { e.preventDefault(); });
document.addEventListener('mouseenter', function (e) {
  if (document.body.classList.contains('compact') && e.target.closest && e.target.closest('#bottombar')) {
    setMode('full');                            // ชี้ pill = เต็มจอทันที
  }
}, true);
window.addEventListener('resize', function () { if (state.open) sendRects(); });

// ── หน้า Settings: ดึงข้อมูลจริงทุก endpoint ──
function showPage(name) {
  document.getElementById('page-settings').classList.add('show');
  closeMenus();
}
function hidePages() {
  document.querySelectorAll('.page').forEach(function (p) { p.classList.remove('show'); });
  sendRects();
}
function fillSelect(id, values, current) {
  var el = document.getElementById(id);
  el.innerHTML = '';
  values.forEach(function (v) {
    var o = document.createElement('option');
    o.value = v; o.textContent = v;
    if (String(v) === String(current)) o.selected = true;
    el.appendChild(o);
  });
}
function loadAllSettings() {
  rest('GET', '/ShadowPlay/v.1.0/Record/Settings', null, function (r) {
    if (!r || !r.quality) return;
    fillSelect('set-rec-quality', ['Best', 'High', 'VeryGood', 'Good', 'Medium', 'Low'], r.quality);
    rest('GET', '/ShadowPlay/v.1.0/Resolutions', null, function (rr) {
      fillSelect('set-rec-res', (rr && rr.resolutions) || ['In-game'], r.resolution);
    });
    rest('GET', '/ShadowPlay/v.1.0/Framerates', null, function (ff) {
      fillSelect('set-rec-fps', (ff && ff.framerates) || [60, 30], r.framerate);
    });
  });
  rest('GET', '/ShadowPlay/v.1.0/InstantReplay/Settings', null, function (r) {
    if (!r || !r.quality) return;
    fillSelect('set-ir-quality', ['Custom', 'Best', 'High', 'Medium', 'Low'], r.quality);
    var len = document.getElementById('set-ir-len');
    var mins = Math.max(1, Math.round((r.replayLengthSeconds || 15) / 60));
    var matched = false;
    Array.prototype.forEach.call(len.options, function (o) {
      if (parseInt(o.value) === mins) { o.selected = true; matched = true; }
    });
    if (!matched) len.selectedIndex = 2;
  });
  rest('GET', '/ShadowPlay/v.1.0/Audio', null, function (r) {
    if (r && r.mode) document.getElementById('set-audio').value = r.mode;
  });
  rest('GET', '/ShadowPlay/v.1.0/RecordPaths', null, function (r) {
    if (!r) return;
    document.getElementById('set-videos').textContent = r.videos || '-';
    document.getElementById('set-tmp').textContent = r.tempFiles || '-';
  });
}
document.addEventListener('change', function (e) {
  var id = e.target.id;
  if (id === 'set-rec-quality' || id === 'set-rec-res' || id === 'set-rec-fps') {
    rest('GET', '/ShadowPlay/v.1.0/Record/Settings', null, function (cur) {
      var body = Object.assign({}, cur || {});
      if (id === 'set-rec-quality') body.quality = e.target.value;
      if (id === 'set-rec-res') body.resolution = e.target.value;
      if (id === 'set-rec-fps') body.framerate = parseInt(e.target.value);
      rest('POST', '/ShadowPlay/v.1.0/Record/Settings', body);
    });
  }
  if (id === 'set-ir-quality' || id === 'set-ir-len') {
    rest('GET', '/ShadowPlay/v.1.0/InstantReplay/Settings', null, function (cur) {
      var body = Object.assign({}, cur || {});
      if (id === 'set-ir-quality') body.quality = e.target.value;
      if (id === 'set-ir-len') body.replayLengthSeconds = parseInt(e.target.value) * 60;
      rest('POST', '/ShadowPlay/v.1.0/InstantReplay/Settings', body);
    });
  }
  if (id === 'set-audio') {
    rest('POST', '/ShadowPlay/v.1.0/Audio', { mode: e.target.value });
  }
});

// ── notifier แท้ (slide เขียว→ดำ + fade, ปิดเองใน 4 วิ) ──
var notifierTimer = null;
function showNotifier(msg, iconCls) {
  var n = document.getElementById('notifier');
  if (!n) return;
  document.getElementById('notifier-msg').textContent = msg || 'Press Alt+Z to use GeForce Experience in-game overlay';
  n.classList.remove('show', 'flip'); void n.offsetWidth;
  n.classList.add('show');
  if (notifierTimer) clearTimeout(notifierTimer);
  notifierTimer = setTimeout(function () {
    n.classList.add('flip');                      // พับ 90° ออก (แบบแท้ .notifier-flip)
    setTimeout(function () { n.classList.remove('show', 'flip'); }, 1100);
  }, 4000);
}

// ── boot ──────────────────────────────────────────────
if (IN_BROWSER) {
  if (location.port && location.port !== '59001') backend.port = parseInt(location.port);
  state.open = true;
  state.openedAt = Date.now();
  document.body.classList.add('open');
  renderTile('InstantReplay');
  renderTile('ManualRecord');
  renderTile('Mic');
  renderTile('Camera');
  pollStatus();
  fitStage();
  connectSocket();
  setTimeout(function () { showNotifier('Press Alt+Z to use GeForce Experience in-game overlay'); }, 1200);
} else {
  oscQuery({ command: 'QUERY_WIN_NODE_INFO' }, false, function (r) {
    try {
      var info = JSON.parse(r);
      if (info.port) { backend.port = info.port; backend.secret = info.secret || ''; }
    } catch (e) {}
    connectSocket();
  });
  oscQuery({ command: 'QUERY_OSC_REGISTER_CLOSE_EVENT' }, true, function () { closeOsc(); });
  oscQuery({ command: 'QUERY_FULLSCREEN_STATE' });
}

function connectSocket() {
  try {
    var s = document.createElement('script');
    s.src = 'http://127.0.0.1:' + backend.port + '/socket.io/socket.io.js';
    s.onload = function () {
      io.connect('http://127.0.0.1:' + backend.port, {
        query: 'X_LOCAL_SECURITY_COOKIE=' + backend.secret,
        transports: ['websocket', 'polling']
      }).on('/ShadowPlay/v.1.0/WindowState', function (d) {
        if (d && d.windowMsg === 'overlayToggle') { toggleOsc(); if (state.open) showNotifier('Press Alt+Z to use GeForce Experience in-game overlay'); }
      }).on('/ShadowPlay/v.1.0/Record/Running', function (d) {
        if (d && typeof d.running !== 'undefined') {
          var want = d.running ? 'On' : 'Off';
          if (tileState.ManualRecord !== want) { tileState.ManualRecord = want; renderTile('ManualRecord'); }
        }
      });
    };
    document.head.appendChild(s);
  } catch (e) { console.log('[osc] socket fail: ' + e.message); }
}
