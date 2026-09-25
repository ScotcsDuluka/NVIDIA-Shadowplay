// generic-addon.js — factory for native-addon shims. Every function the
// genuine JS layer calls arrives as `api.Fn(doReply, ...args)` (callback
// style); the default shim answers `callback(null, {})` so unknown surface
// degrades to empty-200 (same as our previous stub routes) instead of
// crashing. Specific functions get real implementations by passing a map.
'use strict';

function makeAddon(specific, log) {
  const target = {};
  return new Proxy(target, {
    get(t, name) {
      if (name in t) return t[name];
      if (specific && typeof specific[name] === 'function') {
        return specific[name];
      }
      return function (cb) {
        // *Callback registrations take a REAL callback to invoke later —
        // standalone: keep nothing, never call it now (calling with null
        // payloads crashed ApplicationChangedCallback et al).
        if (/Callback$/.test(String(name))) return;
        if (log) {
          try { log('[shim] ' + String(name) + ' -> {}'); } catch (e) {}
        }
        if (typeof cb === 'function') cb(null, {});
        return {};
      };
    },
  });
}

module.exports = makeAddon;
