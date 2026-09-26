// NvShadowPlayAPINode.js — JS shim for the genuine native addon.
// Implements the ShadowPlay hotkey system (the native half):
//   SetHotkeyCallback(cb)  — the genuine module registers its fire callback
//   HotKey(...)            — hotkey registrations (name -> combo content)
//   HotKeyMonitor(...)     — monitor enable state
// The OS listener: our host catches Alt+Z (RegisterHotKey) and pokes the
// loopback fire endpoint; the shim then drives the GENUINE HotkeyCallback
// -> /ShadowPlay/v.1.0/Hotkey socket event -> the page toggles itself.
'use strict'

const http = require('http');
const make = require('./generic-addon.js');

let hotkeyCallback = null;
let oscWindowStateCallback = null;   // the overlay open/close driver
let oscCaptureStateCallback = null;
const registered = {};   // name -> content (from HotKey registrations)
let monitorEnabled = false;

function fire(name) {
  const data = Object.assign({ hotKeyName: name }, registered[name] || {});
  try {
    console.log('[shim:hotkey] firing ' + name);
    if (hotkeyCallback) hotkeyCallback(data);
    // OpenShare = the overlay toggle: drives the WINDOW STATE (the page
    // opens/closes itself on the /ShadowPlay/v.1.0/WindowState event).
    if (name === 'OpenShare' && oscWindowStateCallback) {
      oscWindowStateCallback({ windowMsg: 'overlayToggle' });
      console.log("[shim:hotkey] WindowState windowMsg=overlayToggle emitted");
    }
  } catch (e) {
    try { console.error('[shim:hotkey] fire failed: ' + e.message); } catch (e2) {}
  }
}

// OS-side stand-in listener: the host's WM_HOTKEY (Alt+Z) lands here.
http.createServer(function (req, res) {
  const m = (req.url || '').match(/hk=([A-Za-z0-9_]+)/);
  const name = m ? m[1] : 'OpenShare';
  fire(name);
  res.writeHead(200, { 'Content-Type': 'text/plain' });
  res.end('fired ' + name);
});
const fireServer = http.createServer(function (req, res) {
  const m = (req.url || '').match(/hk=([A-Za-z0-9_]+)/);
  const name = m ? m[1] : 'OpenShare';
  fire(name);
  res.writeHead(200, { 'Content-Type': 'text/plain' });
  res.end('fired ' + name);
});
fireServer.on('error', function (e) {
  console.log('[shim:hotkey] fire listener unavailable: ' + e.code);
});
fireServer.listen(59002, '127.0.0.1', function () {
  console.log('[shim:hotkey] fire listener on http://127.0.0.1:59002');
});

const specific = {
  SetHotkeyCallback: function (cb) { hotkeyCallback = cb; },
  SetOscWindowStateChangeNotificationCallback: function (cb) {
    // The overlay open/close driver: the genuine native fires this and the
    // module relays it as the /ShadowPlay/v.1.0/WindowState socket event.
    oscWindowStateCallback = cb;
  },
  SetOscCaptureStateChangeNotificationCallback: function (cb) {
    oscCaptureStateCallback = cb;
  },
  GetDesktopCaptureSupportReason: function (doReply) {
    // Our engine IS the desktop-capture path — declare full support so the
    // record UI gates open (the page crashes on unsupportReason=undefined).
    if (typeof doReply === 'function') doReply(null, { support: true, unsupportReason: '' });
  },
  GetCaptureState: function (doReply) {
    if (typeof doReply === 'function') doReply(null, { captureMode: 0, recordingState: 0 });
  },
  GetShadowPlayStatus: function (doReply) {
    if (typeof doReply === 'function') doReply(null, { launch: true });
  },
  GetOSCMainViewData: function (doReply, content) {
    if (typeof doReply === 'function') doReply(null, {});
  },
  HotKey: function (doReply, enable, name, content) {
    if (enable) registered[name] = content || {};
    else delete registered[name];
    // The overlay toggle hotkey fires the WINDOW STATE change (the genuine
    // native -> WindowStateChangeNotificationCallback -> /ShadowPlay/v.1.0/
    // WindowState socket event -> the page toggles itself). windowMsg=2 is
    // the toggle value observed on the wire.
    if (enable && name === 'toggle' && oscWindowStateCallback) {
      try { oscWindowStateCallback({ windowMsg: 'overlayToggle' }); }
      catch (e) { try { console.error('[shim] window state fire failed: ' + e.message); } catch (e2) {} }
    }
    if (typeof doReply === 'function') doReply(null, {});
  },
  HotKeyMonitor: function (doReply, enable, content) {
    monitorEnabled = !!enable;
    if (typeof doReply === 'function') doReply(null, {});
  },
  HotKeyDynamicEnable: function (doReply, content) {
    if (typeof doReply === 'function') doReply(null, {});
  },
  GetHotKeyMonitor: function (doReply) {
    if (typeof doReply === 'function') doReply(null, { enable: monitorEnabled });
  },
};

module.exports = make(specific);
