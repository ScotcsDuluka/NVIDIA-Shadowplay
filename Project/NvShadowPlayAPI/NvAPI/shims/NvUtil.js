// nv-util.js — JS shim for NvUtil.node (genuine NvNode, node v11 ABI).
// Standalone posture: no NVIDIA services, no single-instance lock, port
// pinned to 59001, security cookie check disabled (the page is paired by
// our own host via QUERY_WIN_NODE_INFO).
'use strict';

const crypto = require('crypto');
const fs = require('fs');
const path = require('path');

const PORT = 59001;
const VERSION = '3.28.0.412';

function dataDir() {
  // Keep runtime state inside our product tree.
  return path.join(__dirname, '..', '..', 'NvConfig', 'nvnode');
}

module.exports = {
  ClaimSingleInstance: function () { return true; },

  GenerateRandom: function (n) {
    try { return crypto.randomBytes(n || 16).toString('hex'); }
    catch (e) { return '0000000000000000'; }
  },

  GetPortOverride: function () { return PORT; },

  GetGFEVersionSync: function () { return VERSION; },


  GetGFEArchSync: function () { return ''; },
  DeleteGFE2BetaFlagSync: function () { return true; },
  GetGpuArch: function () { return ''; },
  GetGFE3BetaFlagSync: function () { return false; },

  GetLanguage: function () { return 'en-US'; },

  SaveLanguage: function (lang) { try { this._lang = lang; } catch (e) {} },

  GetLocalAppdataPath: function () {
    try { fs.mkdirSync(dataDir(), { recursive: true }); } catch (e) {}
    return dataDir();
  },

  GetProgramDataPath: function () {
    const d = dataDir();
    try { fs.mkdirSync(d, { recursive: true }); } catch (e) {}
    return d;
  },

  GetSystemServiceStatus: function (name) { return true; },

  StartSystemService: function (name) { return true; },

  WaitSystemService: function (name) { return true; },

  SetExitCallback: function (cb) { this._exitCb = cb; },

  VerifyFileSignatureSync: function (file) {
    // Standalone: our own shims/signed-genuine addons pass unconditionally.
    return true;
  },

  IsSecurityCheckEnabled: function () { return false; },




  DestroyLogger: function () { return true; },
  LogInfoSync: function (line) { try { console.log('[nvnode] ' + line); } catch (e) {} },
  LogErrorSync: function (line) { try { console.error('[nvnode] ' + line); } catch (e) {} },
  LogDebugSync: function (line) { try { console.log('[nvnode:debug] ' + line); } catch (e) {} },
  LogWarnSync: function (line) { try { console.warn('[nvnode:warn] ' + line); } catch (e) {} },
  CreateLogger: function (logPath) { return true; },
  LogError: function (line) { try { console.error('[nvnode] ' + line); } catch (e) {} },
  LogDebug: function (line) { try { console.log('[nvnode:debug] ' + line); } catch (e) {} },
  LogInfo: function (line) { try { console.log('[nvnode] ' + line); } catch (e) {} },
  LogWarn: function (line) { try { console.warn('[nvnode:warn] ' + line); } catch (e) {} },
  ConfirmInitialization: function (json) {
    // The genuine chain hands {port, secret} up to the launcher/host. Our
    // host pairs with the backend through its own channel, so this is
    // recorded for diagnostics only.
    try {
      fs.writeFileSync(path.join(dataDir(), 'nvnode-init.json'),
                       String(json), 'utf8');
    } catch (e) {}
    return true;
  },
};
