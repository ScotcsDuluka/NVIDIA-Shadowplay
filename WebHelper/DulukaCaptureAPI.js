//! DulukaCaptureAPI.js — backend module for the "Duluka Capture" osc page.
//! Drop-in module following the DulukaAPI.js pattern: registered in
//! NvNode\index.js with one require line, serves the page's REST surface
//! and persists state to <appdata>\NVIDIA Corporation\NvNode\duluka-capture.json
//!
//! This is the settings/state home for the user's own capture engine
//! (FFmpeg / OBS / GPU passthrough) - the NVIDIA capture stack is untouched.

'use strict'

const fs = require('fs');
const path = require('path');

const DEFAULTS = {
    version: 'duluka-capture-1.0',
    engine: 'ffmpeg',                       // ffmpeg | obs | gpu | apitest
    preset: 'nvidia-medium',                // nvidia-low | nvidia-medium | nvidia-high | my-maximum | custom
    framerate: 60,
    resolution: 'In-game',
    bitrateBps: 50000000,
    apiCapture: 'ffmpeg',
    codeEncoder: 'h264_nvenc',
    command: ''
};

function StateFilePath() {
    const base = process.env.LOCALAPPDATA || 'C:\\Users\\Public';
    return path.join(base, 'NVIDIA Corporation', 'NvNode', 'duluka-capture.json');
}

function LoadState() {
    try {
        return Object.assign({}, DEFAULTS, JSON.parse(fs.readFileSync(StateFilePath(), 'utf8')));
    } catch (err) {
        return Object.assign({}, DEFAULTS);
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

function BuildCommand(state) {
    // the FFmpeg command the page displays + user copies
    if (state.engine !== 'ffmpeg') return '';
    const fps = state.framerate || 60;
    const rate = Math.round((state.bitrateBps || 50000000) / 1000) + 'k';
    const enc = state.codeEncoder || 'h264_nvenc';
    return 'ffmpeg -f gdigrab -framerate ' + fps + ' -i desktop ' +
           '-c:v ' + enc + ' -b:v ' + rate + ' -preset ' + state.preset.replace('nvidia-', '') +
           ' -y duluka-capture.mp4';
}

module.exports = function DulukaCaptureAPI(app, io, logger) {

    let state = LoadState();

    function Push() {
        io.emit('/DulukaCapture/v.1.0/state', state);
    }

    // GET full state
    app.get('/DulukaCapture/v.1.0/settings', function (req, res) {
        res.json(state);
    });

    // POST partial update - merges, persists, broadcasts, returns merged
    app.post('/DulukaCapture/v.1.0/settings', function (req, res) {
        try {
            const body = JSON.parse(req.body || '{}');
            const allowed = Object.keys(DEFAULTS);
            for (const k of allowed) {
                if (body[k] !== undefined) state[k] = body[k];
            }
            state.command = BuildCommand(state);
            SaveState(state);
            Push();
            res.json(state);
        } catch (err) {
            res.status(400).json({ type: 'Error', message: String(err) });
        }
    });

    // reset to defaults
    app.post('/DulukaCapture/v.1.0/reset', function (req, res) {
        state = Object.assign({}, DEFAULTS);
        SaveState(state);
        Push();
        res.json(state);
    });

    return {
        version: function version() { return 'duluka-capture-1.0'; },
        initialize: function initialize() {
            logger.info('DulukaCaptureAPI initialized (Duluka Capture page state)');
            state.command = BuildCommand(state);
            return Promise.resolve();
        },
        cleanup: function cleanup() {
            SaveState(state);
        }
    };
};
