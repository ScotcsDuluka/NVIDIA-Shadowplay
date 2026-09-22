'use strict'
// routes/debug.js — diagnostics surface.
//
//   POST /ShadowPlay/v.1.0/Debug/PageLog   {lines:[...]}  — the osc page's
//       console bridge (user-config.js STANDALONE DEBUG BRIDGE forwards the
//       page console + angular boot milestones here). Lines append to
//       data/logs/page.log so bring-up is observable from the backend side.
//   POST /Debug/PageLog                    — short alias, same handler
//   GET  /Backend/v.1.0/health             — liveness + wiring summary
//   GET  /beta                             — {beta:false} (real floor)
//   GET  /gfe/bp                           — 404 {} (real floor)

const fs = require('fs');
const path = require('path');

function appendPageLog(logDir, lines) {
    try {
        fs.mkdirSync(logDir, { recursive: true });
        const p = path.join(logDir, 'page.log');
        const stamp = new Date().toISOString();
        const blob = lines.map(function (l) { return '[' + stamp + '] ' + String(l).slice(0, 600); }).join('\n') + '\n';
        fs.appendFileSync(p, blob);
    } catch (err) { /* logging must never take the backend down */ }
}

module.exports = function debugRoutes(app, ctx) {
    const bootAt = Date.now();

    function pageLog(req, res) {
        const body = req.body;
        let lines = [];
        if (body && typeof body === 'object' && Array.isArray(body.lines)) {
            lines = body.lines;
        } else if (typeof body === 'string' && body.length) {
            lines = [body];
        }
        if (lines.length) {
            appendPageLog(ctx.cfg.logDir, lines);
            ctx.logger.debug('PageLog +' + lines.length + ' line(s)');
        }
        res.status(200).json({});
    }

    app.post('/ShadowPlay/v.1.0/Debug/PageLog', pageLog);
    app.post('/Debug/PageLog', pageLog);

    app.get('/Backend/v.1.0/health', function (req, res) {
        let osc = { dir: ctx.cfg.oscDir, served: false };
        try { osc.served = fs.existsSync(path.join(ctx.cfg.oscDir, 'index.html')); } catch (e) { /* keep */ }
        res.status(200).json({
            ok: true,
            version: 'nvsp-backend-1.0',
            uptimeSec: Math.round((Date.now() - bootAt) / 1000),
            port: ctx.cfg.port,
            host: ctx.cfg.host,
            osc: osc,
            dulukaServer: ctx.cfg.dulukaServer,
            hardwareSource: ctx.hardware.source(),
            storeFile: ctx.store.filePath()
        });
    });

    app.get('/beta', function (req, res) {
        res.status(200).json({ beta: false });
    });

    app.get('/gfe/bp', function (req, res) {
        res.status(404).json({});
    });
};
