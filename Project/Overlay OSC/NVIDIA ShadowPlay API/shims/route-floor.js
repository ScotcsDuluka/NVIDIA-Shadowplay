// route-floor.js — UNLOCK ALL: ดักทุก route ของ OSC ตอบ shape จริง
// + state สลับได้จริง (record/IR toggle ปุ่มหน้า = สถานะเปลี่ยนจริง)
// mount: require('./shims/route-floor.js')(app);  — วางก่อน module routes
'use strict';

const HOTKEYS = {
  openshare: [18, 90],
  screenshot: [18, 112],
  recordtoggle: [18, 120],
  recordsave: [18, 121],
  dvrtoggle: [16, 18, 121],
  instantreplaytoggle: [18, 117],
  broadcasttoggle: [18, 119],
  broadcastpausetoggle: [16, 18, 119],
  cameratoggle: [18, 67],
  mictoggle: [18, 77],
  fps: [18, 80],
  ptt: [86],
  commentstoggle: [18, 88],
  overlayaswitch: [18, 65],
  overlaybswitch: [18, 66],
  overlaycswitch: [16, 18, 65],
  pmocoverlay: [18, 82],
  pmocoverlaycycle: [16, 18, 82],
  pmocresetaveragemetrics: [16, 18, 80],
  pmocloggingtoggle: [16, 18, 76],
  modsui: [18, 114],
  modstoggle: [16, 18, 114],
  modspreset1: [18, 116],
  modspreset2: [18, 117],
  modspreset3: [18, 118],
  modspresetcycle: [18, 115],
  nvcameraui: [18, 113],
};

const RES = ['In-game', '2160p 4K', '1440p HD', '1080p HD', '720p HD', '480p', '360p', '240p'];
const FPS = [60, 30];

// ---- settings persistence (ไฟล์เดียวกับ NvShadowPlayAPINode shim — seed จากของแท้) ----
const fs = require('fs');
const path = require('path');
const SETTINGS_FILE = path.join(__dirname, '..', 'shadowplay-settings.json');
const SETTINGS_DEFAULTS = {
  record:       { quality: 'VeryGood', resolution: 'In-game', framerate: 60, bitrateBps: 50000000 },
  instantReplay:{ quality: 'Custom', resolution: '1440p HD', framerate: 60, bitrateBps: 50000000, replayLengthSeconds: 15 },
  broadcast:    { quality: 'Good', resolution: '720p HD', framerate: 30, bitrateBps: 3500000, provider: 'Twitch' },
  audio:        { mode: 'both' },
  audioSettings:{ systemAudio: true, microphone: false },
  webcam:       { enable: false, position: 'RightBottom', size: 'Small' },
  recordpaths:  { videos: 'C:\\Users\\ScotcsDuluka\\Videos', tempFiles: 'C:\\Users\\SCOTCS~1\\AppData\\Local\\Temp\\' },
};
function loadSettings() {
  try { return Object.assign({}, SETTINGS_DEFAULTS, JSON.parse(fs.readFileSync(SETTINGS_FILE, 'utf8'))); }
  catch (e) { return JSON.parse(JSON.stringify(SETTINGS_DEFAULTS)); }
}
function saveSettings(s) {
  try { fs.writeFileSync(SETTINGS_FILE, JSON.stringify(s, null, 2)); } catch (e) {}
}
// อ่าน body POST ทั้งก้อน → JSON (คืน null ถ้า parse ไม่ได้/ไม่มี body)
function readBody(req, done) {
  let raw = '';
  req.on('data', function (c) { raw += c; if (raw.length > 256 * 1024) req.destroy(); });
  req.on('end', function () {
    try { done(raw ? JSON.parse(raw) : null); } catch (e) { done(null); }
  });
}
// คีย์ที่ผู้ใช้ปรับเอง (ไฟล์ settings) มาก่อนค่า default
function savedHotkey(name) {
  const s = loadSettings();
  return (s.hotkeys && s.hotkeys[name]) || HOTKEYS[name] || [];
}

const state = {
  recordEnabled: false, recordRunning: false,
  irEnabled: false, irRunning: false,
  broadcastEnabled: false, broadcastRunning: false,
  micPresent: true, ptt: false,
  webcamPresent: false, webcamEnable: false,
  desktopCapture: true,
  fpsIndicator: false, recordIndicator: false,
  coplay: false,
};

function settingsShape(kind) {
  const s = loadSettings();
  if (kind === 'instantreplay') return s.instantReplay;
  if (kind === 'broadcast') return s.broadcast;
  return s.record;
}

module.exports = function routeFloor(app, io) {
  // ตัวอัด = NvCapture.exe (bus: NvContainer.exe spawn + watchdog)
  // หน้า/REST คุม recordEnabled → engine poll /Duluka plane เอง → Actual กลับมาที่นี่

  // ★ แบบของแท้ (CaptureStateChangeNotificationCallback): state เปลี่ยน = emit
  //   ผ่าน socket.io ให้หน้าเห็นทันที ไม่ต้องรอ poll รอบถัดไป
  function emitCapture(name, data) {
    if (!io) return;
    try { io.emit(name, data); } catch (e) {}
  }

  app.use(function (req, res, next) {
    const m = req.method;
    let p = req.path.replace(/\/\d+([\/]?)/, '$1');   // ตัด trailing id (ProcessInfo/123 → /ProcessInfo)

    // ---- POST toggles: สลับ state จริง (ปุ่มหน้ามีชีวิต) ----
    if (m === 'POST') {
      // /Duluka plane: engine confirms actuals — authority for /Record/Running
      if (p === '/Duluka/v.1.0/Actual') {
        readBody(req, function (body) {
          state.recordRunning = !!(body && body.running);
          // engine ยืนยันแล้ว = push ให้หน้าทันที (แบบ CaptureStateChangeCallback ของแท้)
          emitCapture('/ShadowPlay/v.1.0/Record/Enable', { status: state.recordRunning });
          emitCapture('/ShadowPlay/v.1.0/Record/Running', { running: state.recordRunning });
          res.status(200).json({});
        });
        return;
      }
      if (p === '/ShadowPlay/v.1.0/Record/Enable') {
        // recordRunning ไม่แตะที่นี่แล้ว — engine เป็นคนยืนยันผ่าน /Actual
        state.recordEnabled = !state.recordEnabled;
        emitCapture('/ShadowPlay/v.1.0/Record/Enable', { status: state.recordEnabled });
        res.status(200).json({ status: state.recordEnabled });
        return;
      }
      if (p === '/ShadowPlay/v.1.0/InstantReplay/Save') {
        // สัญญาณ Saving หนึ่งจังหวะ → poller เห็น edge → engine save buffer
        state.irSaving = true;
        setTimeout(function () { state.irSaving = false; }, 1500);
        res.status(200).json({ status: true });
        return;
      }
      if (p === '/ShadowPlay/v.1.0/InstantReplay/Enable') {
        state.irEnabled = !state.irEnabled;
        state.irRunning = state.irEnabled;
        emitCapture('/ShadowPlay/v.1.0/InstantReplay/Started', { started: state.irEnabled });
        res.status(200).json({ status: state.irEnabled });
        return;
      }
      if (p === '/ShadowPlay/v.1.0/InstantReplay/Save') {
        res.status(200).json({ status: true });
        return;
      }
      if (p === '/ShadowPlay/v.1.0/Broadcast/Enable') {
        state.broadcastEnabled = !state.broadcastEnabled;
        state.broadcastRunning = state.broadcastEnabled;
        res.status(200).json({ status: state.broadcastEnabled });
        return;
      }
      if (p === '/ShadowPlay/v.1.0/Microphone') {
        res.status(200).json({});
        return;
      }
      if (p === '/ShadowPlay/v.1.0/Microphone/PTT') {
        state.ptt = !state.ptt;
        res.status(200).json({});
        return;
      }
      if (p === '/ShadowPlay/v.1.0/Webcam/Enable') {
        state.webcamEnable = !state.webcamEnable;
        res.status(200).json({});
        return;
      }
      if (p === '/ShadowPlay/v.1.0/CoPlay/Enable') {
        state.coplay = !state.coplay;
        res.status(200).json({});
        return;
      }
      if (p === '/ShadowPlay/v.1.0/DesktopCapture/Enable') {
        state.desktopCapture = !state.desktopCapture;
        res.status(200).json({});
        return;
      }
      if (p === '/ShadowPlay/v.1.0/Audio' || p === '/ShadowPlay/v.1.0/AudioSettings') {
        // POST เปลี่ยนค่าเสียง → เก็บจริง (ไฟล์ settings) — อ่าน body เอง (express ไม่ parse ให้)
        const key = (p === '/ShadowPlay/v.1.0/Audio') ? 'audio' : 'audioSettings';
        readBody(req, function (body) {
          const s = loadSettings();
          if (body) s[key] = Object.assign(s[key] || {}, body);
          saveSettings(s);
          res.status(200).json({});
        });
        return;
      }
      if (p === '/ShadowPlay/v.1.0/Record/Settings' || p === '/ShadowPlay/v.1.0/InstantReplay/Settings' ||
          p === '/ShadowPlay/v.1.0/Broadcast/Settings' || p === '/ShadowPlay/v.1.0/Webcam/Settings' ||
          p === '/ShadowPlay/v.1.0/RecordPaths') {
        // settings ทุกตัว: รวม body เข้าไฟล์ — ค่าคงอยู่ข้ามรีสตาร์ท
        const key = p.indexOf('Record/Settings') >= 0 ? 'record'
          : p.indexOf('InstantReplay') >= 0 ? 'instantReplay'
          : p.indexOf('Broadcast') >= 0 ? 'broadcast'
          : p.indexOf('Webcam') >= 0 ? 'webcam' : 'recordpaths';
        readBody(req, function (body) {
          const s = loadSettings();
          if (body) s[key] = Object.assign(s[key] || {}, body);
          saveSettings(s);
          res.status(200).json({});
        });
        return;
      }
      if (p.indexOf('/ShadowPlay/v.1.0/Hotkey/') === 0 && p.indexOf('/DynamicToggle') < 0) {
        // POST /Hotkey/<name> = ตั้งคีย์ใหม่ (เก็บลงไฟล์ — ค้างข้ามรีสตาร์ท)
        // POST /Hotkey/Monitor = เปิด/ปิดโหมดจับคีย์
        const name = p.substring('/ShadowPlay/v.1.0/Hotkey/'.length).toLowerCase();
        readBody(req, function (body) {
          if (name === 'monitor') {
            state.hotkeyMonitor = !!(body && body.enable);
            res.status(200).json({});
            return;
          }
          // ★ validator แบบของแท้ (native แท้เป็นคนปฏิเสธ — เราทำแทน):
          //   ต้องมี modifier (Ctrl/Alt/Shift/Win) + มีปุ่มจริง + ไม่ซ้ำกับ hotkey อื่น
          const keys = body && Array.isArray(body.keys) ? body.keys : null;
          const hasMod = keys && keys.some(function (k) {
            return k === 16 || k === 17 || k === 18 || k === 91 || k === 92;
          });
          const vk = keys ? keys.filter(function (k) {
            return [16, 17, 18, 91, 92].indexOf(k) < 0;
          })[0] : undefined;
          if (!keys || !hasMod || vk === undefined || keys.length < 2) {
            // หน้า: e.data.code === ET_INVALID_DATA(4) → โชว์ข้อความ error ในหน้า
            res.status(400).json({ type: 'Error', code: 4, codeText: 'ET_INVALID_DATA',
              message: 'Hotkey must include a modifier (Ctrl/Alt/Shift/Win) plus a key' });
            return;
          }
          const s = loadSettings();
          // กันซ้ำ: คีย์ชุดเดียวกันถูกใช้โดยชื่ออื่นอยู่แล้ว?
          const cur = JSON.stringify(keys.slice().sort(function (a, b) { return a - b; }));
          const others = s.hotkeys || {};
          for (var on in others) {
            if (on === name) continue;
            const oc = JSON.stringify(others[on].slice().sort(function (a, b) { return a - b; }));
            if (oc === cur) {
              res.status(400).json({ type: 'Error', code: 4, codeText: 'ET_INVALID_DATA',
                message: 'Shortcut already in use by ' + on });
              return;
            }
          }
          s.hotkeys = s.hotkeys || {};
          s.hotkeys[name] = body.keys;
          saveSettings(s);
          res.status(200).json({});
        });
        return;
      }
    }

    // ---- GET floors ----
    if (m === 'GET') {
      const G = function (body) { res.status(200).json(body); };

      if (p === '/ShadowPlay/v.1.0/Capture/State') return G({ captureMode: 0, recordingState: 0 });
      if (p === '/ShadowPlay/v.1.0/Capture/PIDMode') return G({ pidMode: false });
      if (p.indexOf('/ShadowPlay/v.1.0/Capture/ProcessInfo') === 0) return G({});
      if (p === '/ShadowPlay/v.1.0/DesktopCapture/Enable') return G({ enabled: state.desktopCapture });
      if (p === '/ShadowPlay/v.1.0/DesktopCapture/Support') return G({ support: true });
      if (p.indexOf('/ShadowPlay/v.1.0/DesktopCapture/Support/Reason') === 0) return G({ support: true, unsupportReason: '' });

      if (p === '/ShadowPlay/v.1.0/Record/Enable') return G({ status: state.recordEnabled });
      if (p === '/ShadowPlay/v.1.0/Record/Running') return G({ running: state.recordRunning });
      if (p === '/ShadowPlay/v.1.0/Record/Settings') return G(settingsShape('record'));
      if (p === '/ShadowPlay/v.1.0/Record/Concurrency/Broadcast') return G({ supported: false });
      if (p === '/ShadowPlay/v.1.0/Record/Concurrency/Gamestream') return G({ supported: false });
      if (p === '/ShadowPlay/v.1.0/RecordPaths') return G(loadSettings().recordpaths);

      if (p === '/ShadowPlay/v.1.0/InstantReplay/Enable') return G({ status: state.irEnabled });
      if (p === '/ShadowPlay/v.1.0/InstantReplay/Running') return G({ running: state.irRunning });
      if (p === '/ShadowPlay/v.1.0/InstantReplay/Settings') return G(settingsShape('instantreplay'));
      if (p === '/ShadowPlay/v.1.0/InstantReplay/BufferLength') return G({ bufferLengthSeconds: 15 });

      if (p === '/ShadowPlay/v.1.0/Broadcast/Enable') return G({ status: state.broadcastEnabled });
      if (p === '/ShadowPlay/v.1.0/Broadcast/Running') return G({ running: state.broadcastRunning });
      if (p === '/ShadowPlay/v.1.0/Broadcast/Settings') return G(settingsShape('broadcast'));
      if (p === '/ShadowPlay/v.1.0/Broadcast/2KSupport') return G({ support: false });
      if (p === '/ShadowPlay/v.1.0/Broadcast/Provider') return G({ providers: [] });
      if (p === '/ShadowPlay/v.1.0/Broadcast/Status') return G({ status: 'Stopped' });

      if (p === '/ShadowPlay/v.1.0/CustomOverlay/Support') return G({support:'multiple'}); // ★ 2 = ปลดล็อกหน้า hotkey settings (หน้า: 2===t → ค่อย fetch คีย์ทุกแถว)
      if (p === '/ShadowPlay/v.1.0/Hotkey/Monitor') return G({ enable: !!state.hotkeyMonitor });

      if (p === '/ShadowPlay/v.1.0/Hotkey/openshare') return G({ keys: savedHotkey('openshare') });
      if (p.indexOf('/ShadowPlay/v.1.0/Hotkey/') === 0) {
        var hk = p.substring('/ShadowPlay/v.1.0/Hotkey/'.length).toLowerCase();
        return G({ keys: savedHotkey(hk) });
      }

      if (p === '/ShadowPlay/v.1.0/Audio') return G(loadSettings().audio);
      if (p === '/ShadowPlay/v.1.0/AudioSettings') return G(loadSettings().audioSettings);
      if (p === '/ShadowPlay/v.1.0/Microphone/Present') return G({ present: state.micPresent });
      if (p === '/ShadowPlay/v.1.0/Microphone/PTT') return G({ ptt: state.ptt });
      if (p === '/ShadowPlay/v.1.0/Webcam/Present') return G({ present: state.webcamPresent });
      if (p === '/ShadowPlay/v.1.0/Webcam/Settings') return G(loadSettings().webcam);
      if (p === '/ShadowPlay/v.1.0/Webcam/Enable') return G({ enabled: state.webcamEnable });
      if (p === '/ShadowPlay/v.1.0/CoPlay/Enable') return G({ enabled: state.coplay });

      if (p === '/ShadowPlay/v.1.0/Indicator/fps/Settings') return G({ state: state.fpsIndicator });
      if (p === '/ShadowPlay/v.1.0/Indicator/fps/Support') return G({ support: true });
      if (p === '/ShadowPlay/v.1.0/Indicator/record/Settings') return G({ state: state.recordIndicator });
      if (p === '/ShadowPlay/v.1.0/Indicator/record/Support') return G({ support: true });
      if (p === '/ShadowPlay/v.1.0/Indicator/viewer/Support') return G({ support: true });

      if (p === '/ShadowPlay/v.1.0/GetHDRState') return G({ hdrState: false });

      // ---- /Duluka plane: CAPTURE-REST-PLAN wire-through (engine nvsphelper64 polls these) ----
      if (p === '/Duluka/v.1.0/State') {
        // recordState: page Enable toggle → Starting → engine confirms Actual → Recording
        const rs = !state.recordEnabled ? 'Idle' : (state.recordRunning ? 'Recording' : 'Starting');
        const ir = !state.irEnabled ? 'Idle' : (state.irSaving ? 'Saving' : 'Armed');
        return G({
          recordState: rs,
          irState: ir,
          savePath: loadSettings().recordpaths.videos || '',
        });
      }
      if (p === '/Backend/v.1.0/health') return G({ status: 'ok' });

      if (p === '/ShadowPlay/v.1.0/8k60') return G({ supported: false });
      if (p === '/ShadowPlay/v.1.0/Screenshot/Support') return G({ support: true });
      if (p.indexOf('/Nis2/') !== -1 && p.indexOf('/state') !== -1) return G({ state: false });
      if (p.indexOf('/DeepDVC/') !== -1 && p.indexOf('/state') !== -1) return G({ state: false });
      if (p === '/NvCamera/v.1.0/Compatible') return G({ compatible: true });
      if (p === '/ShadowPlay/v.1.0/Launch') return G({ launch: true });
      if (p === '/beta') return G({ beta: false });
      if (p === '/SDK/v.1.0/Highlights/Active') return G({ active: false });
    }

    next();
  });
};

