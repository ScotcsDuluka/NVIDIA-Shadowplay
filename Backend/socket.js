'use strict'
// socket.js — the push channel: socket.io v2 over engine.io v3.
//
// The osc page ships socket.io-client 2.5.0 (EIO=3 wire protocol, golden
// transcript in docs/osc/03-protocol-map.md) — socket.io MUST stay on v2.
// v3/v4 changed the handshake and the page would never reconnect.
//
// Owns:
//   - connection lifecycle logging (mirrors the Web Helper's handler set)
//   - WindowState emit  (the overlay open/close front door —
//     docs/osc/11-window-flow.md)
//   - hotkey edge debounce (500ms) — RegisterHotKey WM_HOTKEY auto-repeats
//     every ~30ms while a combo is held (that flickered the overlay on
//     Intel), so a 500ms window cleanly separates a real press from its
//     repeats. First toggle in the window wins; later ones are no-ops.

const TOGGLE_COOLDOWN_MS = 500;

function create(httpServer, logger, cfg) {
    const io = require('socket.io')(httpServer, {
        // v2 API — page is same-origin (served from this very server) but
        // tooling/tests may connect from elsewhere; keep the door open
        origins: '*:*',
        serveClient: false
    });

    // optional security cookie handshake (standalone: disabled —
    // disableSecurity=1, the standalone-backend convention)
    io.use(function (socket, next) {
        if (cfg.securityCheck) {
            // real installs published the cookie via NvBackend; standalone
            // never has one, so when the check is ON we accept any cookie
            // but require its PRESENCE (parity with the old strict mode
            // without the native publisher)
            if (!socket.handshake.query.X_LOCAL_SECURITY_COOKIE) {
                next(new Error('Security token is invalid'));
                return;
            }
        }
        next();
    });

    io.on('connection', function (socket) {
        logger.info('Socket ' + socket.id + ' connected (' + socket.request.connection.remoteAddress + ')');

        socket.on('error', function (error) {
            logger.info('Socket ' + socket.id + ' error: ' + error);
        });
        socket.on('reconnect', function () { logger.info('Socket ' + socket.id + ' reconnected'); });
        socket.on('reconnecting', function () { logger.info('Socket ' + socket.id + ' reconnecting'); });
        socket.on('reconnect_attempt', function () { logger.info('Socket ' + socket.id + ' reconnect attempt'); });
        socket.on('reconnect_error', function () { logger.error('Socket ' + socket.id + ' reconnect error'); });
        socket.on('reconnect_failed', function () { logger.error('Socket ' + socket.id + ' reconnect failed'); });
        socket.on('disconnect', function (reason) {
            logger.info('Socket ' + socket.id + ' disconnected (' + reason + ')');
        });
    });

    // ── WindowState (overlay front door) ────────────────────────────
    function emitWindowState(windowMsg) {
        try {
            io.emit('/ShadowPlay/v.1.0/WindowState', { windowMsg: windowMsg });
            logger.info('WindowState emitted: ' + windowMsg);
        } catch (err) {
            logger.error('WindowState emit failed: ' + err);
        }
    }

    // generic broadcast helper (Account/Duluka/PiplConfig state pushes)
    function emitChannel(channel, payload) {
        try {
            io.emit(channel, payload);
        } catch (err) {
            logger.error('emit ' + channel + ' failed: ' + err);
        }
    }

    // ── hotkey edge debounce ────────────────────────────────────────
    const lastToggleAt = Object.create(null);

    function toggleAllowed(hk) {
        const now = Date.now();
        if (now - (lastToggleAt[hk] || 0) < TOGGLE_COOLDOWN_MS) return false;
        lastToggleAt[hk] = now;
        return true;
    }

    // wire-through used by POST /ShadowPlay/v.1.0/Hotkey/Toggle (the
    // hotkey-listener's front door) and any other toggle-ish hotkey
    function hotkeyToggle(hk, windowMsg) {
        if (!toggleAllowed(hk)) {
            logger.info('hotkey toggle suppressed (key repeat): ' + hk);
            return false;
        }
        emitWindowState(windowMsg || 'overlayToggle');
        return true;
    }

    return {
        io: io,
        emitWindowState: emitWindowState,
        emitChannel: emitChannel,
        toggleAllowed: toggleAllowed,
        hotkeyToggle: hotkeyToggle
    };
}

module.exports = { create: create, TOGGLE_COOLDOWN_MS: TOGGLE_COOLDOWN_MS };
