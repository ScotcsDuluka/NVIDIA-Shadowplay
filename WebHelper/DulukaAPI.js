//! DulukaAPI.js — drop-in replacement for NvAccountAPI.js (standalone mode).
//!
//! Same factory signature and interface surface as NvAccountAPI.js:
//!   module(app, io, logger, sendFeedback, NvBackendAPI, NvPiplConfig)
//!   -> { version, initialize, cleanup, notifyUiLanguageChange }
//!
//! Serves the /Account/* + /Settings/* routes the osc page calls with
//! DULUKA account data instead of NVIDIA jarvis/cloud. Login state lives
//! in <appdata>\NvNode\duluka.json; the OAuth login entry point mirrors
//! the page's existing loopback capture (docs/osc/13-duluka-account.md).
//!
//! STANDALONE SAFE: no native calls, initialize() always resolves — the
//! boot chain can never hang on cloud/consent checks.

'use strict'

const fs = require('fs');
const path = require('path');

const STATE_VERSION = 'duluka-1.0';

function StateFilePath() {
    // mirrors GetNvNodeAppdataDirectoryPath()\duluka.json
    const base = process.env.LOCALAPPDATA || 'C:\\Users\\Public';
    return path.join(base, 'NVIDIA Corporation', 'NvNode', 'duluka.json');
}

function LoadState() {
    try {
        return JSON.parse(fs.readFileSync(StateFilePath(), 'utf8'));
    } catch (err) {
        return null;
    }
}

function SaveState(state) {
    try {
        const p = StateFilePath();
        fs.mkdirSync(path.dirname(p), { recursive: true });
        fs.writeFileSync(p, JSON.stringify(state, null, 2));
        return true;
    } catch (err) {
        return false;
    }
}

function DefaultState() {
    return {
        version: STATE_VERSION,
        loggedIn: false,
        user: null,                 // { id, displayName, avatarUrl, email }
        token: null,                // Duluka session token
        privacy: {                  // jarvis-shaped privacy defaults
            dataCollection: false,
            personalization: false
        },
        rewards: {}
    };
}

module.exports = function DulukaAPI(app, io, logger, sendFeedback, NvBackendAPI, NvPiplConfig) {

    let state = LoadState() || DefaultState();

    function State() { return state; }

    // ── REST surface the osc page calls ────────────────────────────

    // session token: the page treats 200+token as "logged in"
    app.get('/Account/v.1.0/UserToken', function (req, res) {
        if (!state.loggedIn || !state.token) {
            res.status(401).json({ type: 'Error', code: '401', codeText: 'NotLoggedIn', message: 'Duluka account not logged in' });
            return;
        }
        res.json({ token: state.token, user: state.user, provider: 'duluka' });
    });
    app.post('/Account/v.1.0/UserToken', function (req, res) {
        // body: { token, user } — written by the Duluka login bridge
        try {
            const body = JSON.parse(req.body || '{}');
            if (body.token) {
                state.token = body.token;
                state.user = body.user || null;
                state.loggedIn = true;
                SaveState(state);
                io.emit('/Account/v.1.0/update', { loggedIn: true, user: state.user });
            }
            res.json({});
        } catch (err) {
            res.status(400).json({ type: 'Error', message: String(err) });
        }
    });

    // logout
    app.post('/Account/v.1.0/Logout', function (req, res) {
        state.loggedIn = false;
        state.token = null;
        state.user = null;
        SaveState(state);
        io.emit('/Account/v.1.0/update', { loggedIn: false });
        res.json({});
    });

    // privacy settings (page reads on boot)
    app.get('/Account/v.1.0/PrivacySettings', function (req, res) {
        res.json(state.privacy || {});
    });
    app.post('/Account/v.1.0/PrivacySettings', function (req, res) {
        try {
            const body = JSON.parse(req.body || '{}');
            state.privacy = Object.assign(state.privacy || {}, body);
            SaveState(state);
            res.json({});
        } catch (err) {
            res.status(400).json({ type: 'Error', message: String(err) });
        }
    });
    app.post('/Account/v.1.0/RefreshPrivacySettings', function (req, res) {
        res.json({});
    });

    // localized endpoints: previously errored on standalone — accept & ack
    app.post('/Applications/v.1.0/SetLocalizedEndpoints', function (req, res) {
        res.json({});
    });

    // rewards preference (page boot call)
    app.get('/Settings/v.1.0/RewardsNotificationPreference', function (req, res) {
        res.json(state.rewards || {});
    });
    app.post('/Settings/v.1.0/RewardsNotificationPreference', function (req, res) {
        try {
            state.rewards = JSON.parse(req.body || '{}');
            SaveState(state);
        } catch (err) { /* keep going */ }
        res.json({});
    });

    // socket surface: the page may ask for account state live
    io.on('connection', function (socket) {
        socket.on('/Account/v.1.0/GetState', function () {
            socket.emit('/Account/v.1.0/state', {
                loggedIn: state.loggedIn,
                user: state.user,
                provider: 'duluka'
            });
        });
    });

    // ── interface parity with NvAccountAPI ─────────────────────────

    return {
        version: function version() {
            return STATE_VERSION;
        },
        initialize: function initialize() {
            // never hangs: no native calls, no cloud, no consent checks
            logger.info('DulukaAPI initialized (standalone account provider)');
            return Promise.resolve();
        },
        cleanup: function cleanup() {
            SaveState(state);
        },
        notifyUiLanguageChange: function notifyUiLanguageChange() {
            return undefined;
        },
        State: State
    };
};
