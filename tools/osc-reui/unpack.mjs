#!/usr/bin/env node
/**
 * unpack.mjs — split the legacy OSC webpack bundles (Overlay/osc/vendor.js,
 * app.js) into readable per-module files under docs/osc/unpacked/.
 *
 * Bundle shapes (verified against the artifacts):
 *   vendor.js : var vendor = function(e){…require loop…return t}( <modules> )
 *               <modules> = ARRAY of factory functions (id = array index)
 *   app.js    : webpackJsonp([1,0], <modules-array>)
 *               module 7 is the bridge: module.exports = vendor  →  n(7)(<id>)
 *               re-exports a vendor module.
 *   common.js : webpack runtime (single small file, beautified as-is)
 *
 * Output:
 *   docs/osc/unpacked/<bundle>/modules/<id>.js        beautified module
 *   docs/osc/unpacked/<bundle>/templates/<id>__*.html inline templates
 *   docs/osc/unpacked/<bundle>/styles/<id>.css        style-loader CSS (heuristic)
 *   docs/osc/unpacked/<bundle>/INDEX.md               id → role/name/requires
 *   docs/osc/unpacked/names.json                      angular name → module
 *   docs/osc/unpacked/README.md                       how to navigate
 *
 * Requires (test-only, not shipped): acorn + js-beautify from DEPS_DIR.
 * Run: DEPS_DIR="C:\…\node_modules" node tools/osc-reui/unpack.mjs
 */

import fs from 'node:fs';
import path from 'node:path';
import { createRequire } from 'node:module';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const OSC = path.resolve(__dirname, '../../Overlay/osc');
const OUT = path.resolve(__dirname, '../../docs/osc/unpacked');
const DEPS = process.env.DEPS_DIR;
if (!DEPS) {
    console.error('Set DEPS_DIR to a node_modules dir containing acorn + js-beautify');
    process.exit(1);
}
const req = createRequire(path.join(DEPS, 'acorn', 'package.json'));
const acorn = req('acorn');
const beautify = req('js-beautify');

const B = (src) => beautify.js(src, { indent_size: 2, wrap_line_length: 120, preserve_newlines: false });

function parse(src) {
    return acorn.parse(src, { ecmaVersion: 2020, ranges: true, allowReturnOutsideFunction: true });
}

/** extract module factories from an ArrayExpression (or ObjectExpression) */
function extractModules(node, src) {
    const out = [];
    if (node.type === 'ArrayExpression') {
        node.elements.forEach((el, i) => {
            if (el && (el.type === 'FunctionExpression' || el.type === 'ArrowFunctionExpression')) {
                out.push({ id: i, code: src.slice(el.range[0], el.range[1]) });
            }
        });
    } else if (node.type === 'ObjectExpression') {
        for (const prop of node.properties) {
            const id = prop.key.type === 'Literal' ? Number(prop.key.value)
                : prop.key.type === 'Identifier' ? NaN : NaN;
            if (prop.value.type === 'FunctionExpression' || prop.value.type === 'ArrowFunctionExpression') {
                out.push({ id, code: src.slice(prop.value.range[0], prop.value.range[1]) });
            }
        }
    }
    return out;
}

const ANG_KINDS = ['service', 'factory', 'controller', 'directive', 'provider', 'constant', 'value', 'filter', 'component'];

function analyze(mod, bundle) {
    const { code } = mod;
    const roles = [];
    const names = {}; // kind → [names]
    const queries = new Set();
    const channels = new Set();
    const urls = new Set();
    let moduleDef = null;
    let templateBig = null; // {raw, name}
    let cssBig = null;

    // angular module definition
    const md = code.match(/\.module\(\s*["']([a-zA-Z0-9_\.]+)["']\s*,\s*\[/);
    if (md) moduleDef = md[1];

    for (const k of ANG_KINDS) {
        const re = new RegExp(`\\.\\s*${k}\\(\\s*["']([^"']+)["']`, 'g');
        const set = new Set();
        let m;
        while ((m = re.exec(code))) set.add(m[1]);
        if (set.size) {
            names[k] = [...set].sort();
            for (const n of set) roles.push(`${k} ${n}`);
        }
    }

    for (const m of code.matchAll(/QUERY_[A-Z_0-9]+/g)) queries.add(m[0]);
    for (const m of code.matchAll(/["'](\/(?:ShadowPlay|Settings|PiplConfig|Account|SDK|NvCamera|GameShare|abHubAPI)\/v\.[0-9.]+\/[A-Za-z0-9_\.\/]+)["']/g)) channels.add(m[1]);
    for (const m of code.matchAll(/createEndpoint\(\{url:\s*["']([^"']+)["']/g)) urls.add(m[1]);

    const templateUrl = code.match(/templateUrl\s*:\s*["']([^"']+)["']/)?.[1] ?? null;

    // biggest double-quoted literal that looks like markup → template file
    for (const m of code.matchAll(/"((?:[^"\\]|\\.){300,})"/g)) {
        const raw = m[1];
        if (!templateBig && /<[a-z-]+[\s>]/i.test(raw)) templateBig = raw;
        // CSS heuristic: long string with selector{prop:value} shape
        if (!cssBig && /\{[^{}]*:[^{}]*;/.test(raw) && /(color|background|font|margin|padding|display|border|width|height)/.test(raw)) cssBig = raw;
    }

    // requires (third param is the webpack require)
    const requires = new Set();
    let bridge = new Set();
    if (mod.params?.[2]) {
        const rn = mod.params[2].name;
        const re = new RegExp(`(?<![\\w$.])${rn}\\s*\\(\\s*(\\d+)\\s*\\)`, 'g');
        let mm;
        while ((mm = re.exec(code))) requires.add(Number(mm[1]));
        // chained: n(bridge)(vendorId) — bridge module re-exports vendor require
        const bre = new RegExp(`(?<![\\w$.])${rn}\\s*\\(\\s*(\\d+)\\s*\\)\\s*\\(\\s*(\\d+)\\s*\\)`, 'g');
        while ((mm = bre.exec(code))) bridge.add(Number(mm[2]));
    }
    return { roles, names, queries: [...queries], channels: [...channels], urls: [...urls], moduleDef, templateBig, cssBig, templateUrl, requires: [...requires], bridge: [...bridge] };
}

function unescapeJson(raw) {
    try { return JSON.parse('"' + raw + '"'); } catch { return null; }
}

function writeBundle(bundleName, modules, srcLength) {
    const dir = path.join(OUT, bundleName);
    fs.mkdirSync(path.join(dir, 'modules'), { recursive: true });
    fs.mkdirSync(path.join(dir, 'templates'), { recursive: true });
    fs.mkdirSync(path.join(dir, 'styles'), { recursive: true });

    const nameMap = {}; // angular name → {module, kind}
    const rows = [];
    let templates = 0, styles = 0;

    for (const mod of modules) {
        const info = analyze(mod, bundleName);
        const pretty = B(mod.code);

        // header comment
        const role = info.roles.length ? info.roles.slice(0, 6).join(' | ') : 'utility';
        const reqs = info.requires.map((r) => `${bundleName}/${r}`).concat(info.bridge.map((v) => `vendor/${v} (via bridge)`));
        const header = [
            `// ─────────────────────────────────────────────────────────────`,
            `// ${bundleName.toUpperCase()} MODULE ${mod.id}`,
            `// role       : ${role}`,
            info.moduleDef ? `// defines    : angular.module("${info.moduleDef}")` : null,
            reqs.length ? `// requires   : ${reqs.join(', ')}` : `// requires   : (none)`,
            info.queries.length ? `// cefQuery   : ${info.queries.join(', ')}` : null,
            info.channels.length ? `// channels   : ${info.channels.join(', ')}` : null,
            info.templateUrl ? `// templateUrl: ${info.templateUrl}` : null,
            `// source     : Overlay/osc/${bundleName}.js (minified) — beautified, unrecoverable local names remain`,
            `// ─────────────────────────────────────────────────────────────`,
        ].filter(Boolean).join('\n');

        fs.writeFileSync(path.join(dir, 'modules', `${mod.id}.js`), header + '\n' + pretty + '\n');

        // template extraction
        if (info.templateBig) {
            const decoded = unescapeJson(info.templateBig);
            if (decoded) {
                const name = (info.names.directive?.[0] || info.names.controller?.[0] || 'template');
                fs.writeFileSync(path.join(dir, 'templates', `${mod.id}__${name.replace(/[^\w-]/g, '_')}.html`), decoded + '\n');
                templates++;
            }
        }
        if (info.cssBig) {
            const decoded = unescapeJson(info.cssBig);
            if (decoded) {
                fs.writeFileSync(path.join(dir, 'styles', `${mod.id}.css`), decoded + '\n');
                styles++;
            }
        }

        for (const [kind, list] of Object.entries(info.names)) {
            for (const n of list) {
                if (!nameMap[n]) nameMap[n] = { kind, module: mod.id, bundle: bundleName };
            }
        }
        rows.push({ id: mod.id, bytes: mod.code.length, role, ...info });
    }

    // INDEX.md
    rows.sort((a, b) => a.id - b.id);
    const idx = [];
    idx.push(`# ${bundleName} — module index (${rows.length} modules, source ${srcLength.toLocaleString()} bytes)`);
    idx.push('');
    idx.push('Files: `modules/<id>.js` (beautified) · `templates/<id>__*.html` · `styles/<id>.css`');
    idx.push('Roles are detected from Angular registrations and protocol strings inside each module.');
    idx.push('');
    idx.push('| id | bytes | role | requires |');
    idx.push('|---:|---:|---|---|');
    for (const r of rows) {
        const reqs = r.requires.map((x) => `${x}`).concat(r.bridge.map((v) => `vendor/${v}`)).join(', ');
        idx.push(`| ${r.id} | ${r.bytes} | ${r.role.replaceAll('|', '\\|')} | ${reqs || '—'} |`);
    }
    fs.writeFileSync(path.join(dir, 'INDEX.md'), idx.join('\n') + '\n');
    return { count: rows.length, templates, styles, nameMap };
}

// ── run ──────────────────────────────────────────────────────────────────
fs.rmSync(OUT, { recursive: true, force: true });
fs.mkdirSync(OUT, { recursive: true });

// vendor.js
const vSrc = fs.readFileSync(path.join(OSC, 'vendor.js'), 'utf8');
const vAst = parse(vSrc);
let vModules = null;
for (const stmt of vAst.body) {
    if (stmt.type === 'VariableDeclaration') {
        const init = stmt.declarations?.[0]?.init;
        if (init?.type === 'CallExpression' && init.callee?.type === 'FunctionExpression') {
            vModules = extractModules(init.arguments[0], vSrc);
        }
    }
}
if (!vModules) { console.error('vendor.js: module table not found'); process.exit(1); }
const vRes = writeBundle('vendor', vModules, vSrc.length);

// app.js
const aSrc = fs.readFileSync(path.join(OSC, 'app.js'), 'utf8');
const aAst = parse(aSrc);
let aModules = null;
for (const stmt of aAst.body) {
    if (stmt.type === 'ExpressionStatement' && stmt.expression?.type === 'CallExpression'
        && stmt.expression.callee?.type === 'Identifier' && /webpackJsonp/.test(stmt.expression.callee.name)) {
        aModules = extractModules(stmt.expression.arguments[1], aSrc);
    }
}
if (!aModules) { console.error('app.js: module table not found'); process.exit(1); }
const aRes = writeBundle('app', aModules, aSrc.length);

// common.js (runtime, beautify whole)
const cSrc = fs.readFileSync(path.join(OSC, 'common.js'), 'utf8');
fs.writeFileSync(path.join(OUT, 'common.beautified.js'), B(cSrc) + '\n');

// names.json — angular name → module (merged)
fs.writeFileSync(path.join(OUT, 'names.json'),
    JSON.stringify({ vendor: vRes.nameMap, app: aRes.nameMap }, null, 1));

// README
const readme = `# Unpacked legacy OSC bundles

Generated from \`Overlay/osc/vendor.js\` + \`app.js\` (minified webpack 1.x,
module id = array index; no source maps exist). Local variable names inside
functions remain minified (\`e, t, n\`) — everything the code *says* (Angular
registrations, templates, CSS, protocol strings, l10n keys) is intact.

## Navigation

1. \`names.json\` — Angular name (service/directive/controller/…) → module file.
   Start here: \`"cefService" → vendor/<id>\`, \`"socketService" → app/<id>\`, …
2. \`<bundle>/INDEX.md\` — every module id with detected role + requires graph.
3. \`<bundle>/modules/<id>.js\` — the beautified module; header lists role,
   requires, QUERY_* commands, socket channels it touches.
4. \`<bundle>/templates/\` — inline Angular templates extracted to HTML.
5. \`<bundle>/styles/\` — CSS recovered from style-loader strings (heuristic).
6. \`common.beautified.js\` — the webpack runtime.

## Counts

| bundle | modules | templates | css blobs |
|---|---:|---:|---:|
| vendor | ${vRes.count} | ${vRes.templates} | ${vRes.styles} |
| app    | ${aRes.count} | ${aRes.templates} | ${aRes.styles} |

## Notes

- \`app/7\`-style bridge: app modules call \`n(7)(<id>)\` to reach vendor
  modules — INDEX shows those as \`vendor/<id>\`.
- Some vendor "modules" are webpack shims (module merging, polyfills);
  their headers will read \`utility\`.
- This tree is derived material for reading — the executable truth remains
  the original bundle; do not edit files here and expect the app to change.
`;
fs.writeFileSync(path.join(OUT, 'README.md'), readme);

console.log(`vendor: ${vRes.count} modules, ${vRes.templates} templates, ${vRes.styles} css`);
console.log(`app:    ${aRes.count} modules, ${aRes.templates} templates, ${aRes.styles} css`);
console.log('names.json entries:', Object.keys(vRes.nameMap).length, '+', Object.keys(aRes.nameMap).length);
console.log('OUT:', OUT);
