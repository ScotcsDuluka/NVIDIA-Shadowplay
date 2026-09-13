#!/usr/bin/env node
/**
 * unpack-src.mjs — second-stage unpacker: turn the per-module dump into a
 * SOURCE-LIKE tree.
 *
 * For every webpack module factory `function(e, t, n){ … }`:
 *   - scope-aware rename (babel): e→module, t→exports, n→require
 *     (shadowed inner `n`s are left alone — correct, not regex-blind)
 *   - emit `docs/osc/unpacked/src/<bundle>/<id>.<name>.js`
 *   - annotate every `require(N)` with a comment naming the target module
 *     (app ↔ vendor via the bridge module `module.exports = vendor`)
 *   - regenerate INDEX.md with the source-like filenames
 *
 * Requires (test-only): @babel/* + js-beautify in DEPS_DIR.
 */

import fs from 'node:fs';
import path from 'node:path';
import { createRequire } from 'node:module';
import { fileURLToPath } from 'node:url';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const OSC = path.resolve(__dirname, '../../Overlay/osc');
const OUT = path.resolve(__dirname, '../../docs/osc/unpacked');
const DEPS = process.env.DEPS_DIR;
if (!DEPS) { console.error('Set DEPS_DIR'); process.exit(1); }
const req = createRequire(path.join(DEPS, '@babel', 'parser', 'package.json'));
const parser = req('@babel/parser');
const traverseMod = req('@babel/traverse');
const traverse = traverseMod.default ?? traverseMod;
const genMod = req('@babel/generator');
const generate = genMod.default ?? genMod;
const beautify = req('js-beautify');

const B = (s) => beautify.js(s, { indent_size: 2, wrap_line_length: 110, preserve_newlines: false });

function extract(file) {
    const src = fs.readFileSync(file, 'utf8');
    const ast = parser.parse(src, { ecmaVersion: 2020 });
    const modules = [];
    function visit(node) {
        if (!node || typeof node.type !== 'string') return;
        if (node.type === 'CallExpression') {
            const c = node.callee;
            const args = node.arguments;
            if (c.type === 'Identifier' && /webpackJsonp/.test(c.name) && args[1]?.type === 'ArrayExpression') {
                collect(args[1]); return;
            }
            if (c.type === 'FunctionExpression' && args[0]?.type === 'ArrayExpression') {
                collect(args[0]); return;
            }
        }
        for (const k of Object.keys(node)) {
            if (k === 'loc' || k === 'leadingComments' || k === 'trailingComments') continue;
            const v = node[k];
            if (Array.isArray(v)) v.forEach(visit);
            else if (v && typeof v.type === 'string') visit(v);
        }
    }
    function collect(arr) {
        arr.elements.forEach((el, i) => {
            if (el?.type === 'FunctionExpression') modules.push({ id: i, node: el });
        });
    }
    visit(ast);
    return { src, modules };
}

function transform(raw, resolvers) {
    // parse the factory as an expression so params are in scope
    const ast2 = parser.parse(`(${raw})`, { allowReturnOutsideFunction: true });
    const fn = ast2.program.body[0].expression;

    const params = fn.params.map((p) => p.name).filter(Boolean);
    const ren = [];
    if (params[0]) ren.push([params[0], 'module']);
    if (params[1]) ren.push([params[1], 'exports']);
    if (params[2]) ren.push([params[2], 'require']);

    traverse(ast2, {
        Program(progPath) {
            const fnPath = progPath.get('body.0.expression');
            const s = fnPath.scope;
            for (const [from, to] of ren) {
                const binding = s.getBinding(from);
                if (binding) s.rename(from, to);
            }
        },
    });

    let out = generate(ast2.program.body[0].expression, { comments: false, jsescOption: { minimal: true } }).code;
    out = B(out);
    // annotate cross-bundle requires: require(bridgeId)(vendorId) → vendorModule(vendorId)
    out = out.replace(new RegExp(`\\brequire\\(${resolvers.bridgeId}\\)\\((\\d+)\\)`, 'g'), (m, vid) => {
        const n = resolvers.vendor(Number(vid));
        return `vendorModule(${vid}) /* vendor/${vid}${n ? ' — ' + n : ''} */`;
    });
    out = out.replace(new RegExp(`\\brequire\\(${resolvers.bridgeId}\\)`, 'g'),
        'vendorModule /* vendor bundle require */');
    // annotate same-bundle requires (skip the bridge id)
    out = out.replace(/\brequire\((\d+)\)/g, (m, id) => {
        const n = resolvers.self(Number(id));
        return n ? `require(${id}) /* ${resolvers.bundle}/${id} — ${n} */` : m;
    });
    return { code: out, params: ren.map(([, to]) => to) };
}

async function main() {
    // pass 1: collect names (reuse names.json logic quickly)
    const names = { app: {}, vendor: {} };
    const kinds = ['service', 'factory', 'controller', 'directive', 'provider', 'constant', 'value', 'filter', 'component'];
    function scanNames(bundleName, modules, src) {
        for (const m of modules) {
            const code = src.slice(m.node.start, m.node.end);
            for (const k of kinds) {
                const re = new RegExp(`\\.\\s*${k}\\(\\s*["']([^"']+)["']`, 'g');
                let mm;
                while ((mm = re.exec(code))) {
                    const n = mm[1];
                    if (!names[bundleName][n]) names[bundleName][n] = { id: m.id, kind: k };
                }
            }
            const md = code.match(/\.module\(\s*["']([a-zA-Z0-9_\.]+)["']\s*,\s*\[/);
            if (md && !names[bundleName][md[1]]) names[bundleName][md[1]] = { id: m.id, kind: 'module' };
        }
    }

    const v = extract(path.join(OSC, 'vendor.js'));
    scanNames('vendor', v.modules, v.src);
    const a = extract(path.join(OSC, 'app.js'));
    scanNames('app', a.modules, a.src);

    // bridge module (app): body == `module.exports = vendor`
    let bridgeId = null;
    for (const m of a.modules) {
        const body = a.src.slice(m.node.start, m.node.end);
        if (body.length < 120 && /=\s*vendor\s*;?\s*\}?/.test(body)) { bridgeId = m.id; break; }
    }

    const nameFor = (bundle, id) => {
        for (const [n, info] of Object.entries(names[bundle])) {
            if (info.id === id) return `${n} (${info.kind})`;
        }
        return null;
    };
    const resolversFor = (bundleName) => ({
        bundle: bundleName,
        bridgeId: bridgeId ?? -1,
        self: (id) => nameFor(bundleName, id),
        vendor: (id) => nameFor('vendor', id),
    });
    const fileBase = (bundle, id) => {
        const n = nameFor(bundle, id);
        const primary = n ? n.split(' (')[0].replace(/[^\w-]/g, '_') : 'module';
        return `${String(id).padStart(4, '0')}.${primary}.js`;
    };

    const dir = path.join(OUT, 'src');
    try { fs.rmSync(dir, { recursive: true, force: true }); } catch { /* transient lock — files are overwritten below */ }
    fs.mkdirSync(dir, { recursive: true });

    const summary = {};
    for (const [bundleName, bundle] of [['vendor', v], ['app', a]]) {
        const bd = path.join(dir, bundleName);
        fs.mkdirSync(bd, { recursive: true });
        const rows = [];
        for (const m of bundle.modules) {
            const raw = bundle.src.slice(m.node.start, m.node.end);
            let code, params;
            try {
                ({ code, params } = transform(raw, resolversFor(bundleName)));
            } catch (err) {
                code = '/* transform failed: ' + err.message + ' */\n' + B(raw);
                params = [];
            }
            const isBridge = bundleName === 'app' && m.id === bridgeId;
            const roles = [];
            for (const k of kinds) {
                const re = new RegExp(`\\.\\s*${k}\\(\\s*["']([^"']+)["']`, 'g');
                let mm;
                while ((mm = re.exec(raw))) roles.push(`${k} ${mm[1]}`);
            }
            const md = raw.match(/\.module\(\s*["']([a-zA-Z0-9_\.]+)["']\s*,\s*\[/);
            if (md) roles.push(`defines angular.module("${md[1]}")`);
            if (isBridge) roles.push('BRIDGE → vendor bundle (require(N) reaches vendor modules)');

            const fname = isBridge ? String(m.id).padStart(4, '0') + '._vendor-bridge.js' : fileBase(bundleName, m.id);
            const header = [
                `// ${'='.repeat(66)}`,
                `// ${bundleName.toUpperCase()} MODULE ${m.id}${isBridge ? '  [VENDOR BRIDGE]' : ''}`,
                roles.length ? `// ${roles.join(' | ')}` : '// (no Angular registrations — utility module)',
                `// webpack factory params: ${params.join(', ') || '(none)'}`,
                `// ${'-'.repeat(66)}`,
            ].join('\n');
            // file body: wrap as CJS-like module
            const body = `// source-like reconstruction — beautified webpack module\n` +
                `// (local identifiers inside functions remain minified; every string,\n` +
                `//  template, and protocol name is the original)\n\n${header}\n\n${code}\n`;
            fs.writeFileSync(path.join(bd, fname), body);
            rows.push({ id: m.id, file: fname, bytes: raw.length, roles });
        }
        rows.sort((x, y) => x.id - y.id);
        const idx = [`# ${bundleName} — source-like modules (${rows.length})`, '',
            'Read `names` via file suffix; every `require(N)` in the code carries a `/* bundle/N — name */` comment.', ''];
        idx.push('| id | file | bytes | role |');
        idx.push('|---:|---|---:|---|');
        for (const r of rows) idx.push(`| ${r.id} | ${r.file} | ${r.bytes} | ${r.roles.join(' \\| ') || 'utility'} |`);
        fs.writeFileSync(path.join(bd, 'INDEX.md'), idx.join('\n') + '\n');
        summary[bundleName] = rows.length;
    }

    // top README
    fs.writeFileSync(path.join(dir, 'README.md'), `# Source-like reconstruction

\`src/vendor/*.js\`, \`src/app/*.js\` — one file per webpack module, factory
params renamed to **module / exports / require** (scope-aware via babel,
shadowed inner names untouched). Every \`require(N)\` is annotated with the
target module's name; app modules reach vendor modules through
\`app/${bridgeId}._vendor-bridge.js\` (\`module.exports = vendor\`).

Reads like CommonJS source. What cannot be recovered: original local
variable names (no source maps exist) — everything the code declares,
registers, or sends over the wire is original.

- \`vendor/INDEX.md\`, \`app/INDEX.md\` — id → file → role tables
- names: \`../names.json\`
- templates/CSS dumps: \`../app/templates\`, \`../app/styles\`, \`../vendor/styles\`
`);
    console.log('src written:', JSON.stringify(summary), 'bridge=app/' + bridgeId);
}

main().catch((e) => { console.error(e); process.exit(1); });
