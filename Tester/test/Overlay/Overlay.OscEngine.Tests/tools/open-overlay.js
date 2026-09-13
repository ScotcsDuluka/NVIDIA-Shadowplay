'use strict';
/* open-overlay.js — speaks the REAL hub protocol (line-delimited TCP,
 * "[Send] <app>|<cmd>[:value]") exactly like Launcher does, to toggle the
 * osc overlay. Usage: node open-overlay.js [open|close|toggle]
 */
const net = require('net');

const sock = net.connect(5001, '127.0.0.1', () => {
  sock.write('[Send] Launcher|open_overlay\n');
  setTimeout(() => { sock.end(); process.exit(0); }, 800);
});
sock.on('error', (e) => { console.log('ERR ' + e.message); process.exit(1); });
sock.on('data', (d) => { const s = d.toString().trim(); if (s) console.log('HUB:', s.slice(0, 120)); });
