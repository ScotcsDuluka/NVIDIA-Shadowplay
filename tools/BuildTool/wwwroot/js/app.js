/* Build Tool SPA — hash router + views (bridge = window.chrome.webview.hostObjects.async.host) */
const pending = new Map();
let rpcSeq = 0;
chrome.webview.addEventListener("message", (e) => {
  try {
    const d = typeof e.data === "string" ? JSON.parse(e.data) : e.data;
    if (d.id && pending.has(d.id)) { pending.get(d.id)(d.result); pending.delete(d.id); }
  } catch {}
});
function rpc(method, ...args) {
  return new Promise((resolve) => {
    const id = "r" + (++rpcSeq);
    pending.set(id, resolve);
    chrome.webview.postMessage(JSON.stringify({ id, method, args }));
  });
}
const bridge = () => ({
  GetStatus: () => rpc("status"),
  StartBuild: (clean) => rpc("startBuild", clean === true),
  GetLog: (since) => rpc("log", since | 0),
  GetVersionConfig: () => rpc("version"),
  SaveVersionConfig: (j) => rpc("saveVersion", typeof j === "string" ? j : JSON.stringify(j)),
  GetPreview: () => rpc("preview"),
  LaunchApp: () => rpc("launch"),
  OpenFolder: () => rpc("openFolder"),
});

window.onerror = (m, src, l, c) => { try { chrome.webview.postMessage("JSERR: " + m + " @" + l + ":" + c); } catch {} };
window.addEventListener("unhandledrejection", (e) => { try { chrome.webview.postMessage("JSREJ: " + e.reason); } catch {} });
const $ = (s) => document.querySelector(s);
const esc = (s) => String(s ?? "").replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;");

let buildTimer = null;
let logSince = 0;

const views = {};

/* ── router ── */
function go(view) {
  document.querySelectorAll(".nav").forEach((a) =>
    a.classList.toggle("on", a.dataset.view === view));
  (views[view] || views.dashboard)();
}
window.addEventListener("hashchange", () => go(location.hash.slice(1) || "dashboard"));

/* ── helpers ── */
async function status() {
  try { return JSON.parse(await bridge().GetStatus()); }
  catch (e) { return { buildNo: "?", commit: "-", procs: [] }; }
}
async function versionCfg() {
  try { return JSON.parse(await bridge().GetVersionConfig()); }
  catch (e) { return { hardcore: false, version: "3.41", buildNo: "?" }; }
}

async function versionsCfg() {
  try { return JSON.parse(await bridge().GetVersions()); }
  catch (e) { return { global: "3.41", hardcore: false, projects: [] }; }
}async function preview() {
  try { return JSON.parse(await bridge().GetPreview()); }
  catch (e) { return { exists: false, files: [], totalMB: 0 }; }
}

/* ── Dashboard ── */
views.dashboard = async function () {
  const s = await status();
  try {
    chrome.webview.postMessage("DOM: main.len=" + document.getElementById("main").innerHTML.length +
      " statCards=" + document.querySelectorAll(".stat").length +
      " sheets=" + document.styleSheets.length +
      " rules=" + (document.styleSheets[0] ? document.styleSheets[0].cssRules.length : "?") +
      " mainRect=" + JSON.stringify(document.getElementById("main").getBoundingClientRect()));
  } catch (e) { }
  const up = (n) => (s.procs || []).some((p) => p.name === n && p.up);
  $("#main").innerHTML = `
    <h1>Dashboard</h1>
    <div class="sub">NVIDIA ShadowPlay — สถานะระบบและ build ล่าสุด</div>
    <div class="grid">
      <div class="stat"><div class="k">BUILD VER</div><div class="v green">${esc(s.buildNo)}</div></div>
      <div class="stat"><div class="k">COMMIT</div><div class="v" style="font-size:15px">${esc(s.commit)}</div></div>
      <div class="stat"><div class="k">ENGINE (nvsphelper64)</div><div class="v ${up("nvsphelper64") ? "green" : ""}" style="font-size:15px">${up("nvsphelper64") ? "RUNNING" : "STOPPED"}</div></div>
      <div class="stat"><div class="k">OVERLAY (NvShadowPlay)</div><div class="v ${up("NvShadowPlay") ? "green" : ""}" style="font-size:15px">${up("NvShadowPlay") ? "RUNNING" : "STOPPED"}</div></div>
    </div>
    <div class="card">
      <h2>FAMILY STATUS</h2>
      <table>
        <tr><th>PROCESS</th><th>สถานะ</th></tr>
        ${["NvContainer", "nvsphelper64", "NvBackend", "NvShadowPlay", "NvNotifier"]
          .map((n) => `<tr><td>${n}</td><td class="${up(n) ? "green" : "dim"}" style="color:${up(n) ? "var(--green)" : "var(--red)"}">${up(n) ? "● RUNNING" : "○ STOPPED"}</td></tr>`)
          .join("")}
      </table>
    </div>
    <div class="card">
      <h2>QUICK ACTIONS</h2>
      <div class="row">
        <button class="primary" onclick="location.hash='#build'">ไปหน้า Build</button>
        <button onclick="bridge().OpenFolder()">เปิดโฟลเดอร์ Build</button>
        <button onclick="refresh()">รีเฟรช</button>
      </div>
    </div>`;
  window.refresh = () => go("dashboard");
};

/* ── Build ── */
views.build = async function () {
  const s = await status();
  $("#main").innerHTML = `
    <h1>Build</h1>
    <div class="sub">รัน build-dev.ps1 (owner dedupe + staging อัตโนมัติ)</div>
    <div class="card">
      <h2>BUILD</h2>
      <div class="row">
        <button class="primary" id="btnBuild">▶ BUILD</button>
        <button id="btnClean">▶ CLEAN BUILD</button>
        <span id="bstate" class="sub"></span>
      </div>
      <div class="log" id="log">— log ว่าง —</div>
    </div>`;
  $("#btnBuild").onclick = () => start(false);
  $("#btnClean").onclick = () => start(true);
  const st = $("#bstate");
  st.textContent = s.running ? "กำลัง build อยู่..." : "พร้อม";
  pollLog();
};

async function start(clean) {
  const r = JSON.parse(await bridge().StartBuild(clean));
  if (!r.ok) { alert(r.error); return; }
  logSince = 0;
  $("#btnBuild").disabled = $("#btnClean").disabled = true;
  $("#log").textContent = "";
  pollLog();
}

function pollLog() {
  clearInterval(buildTimer);
  buildTimer = setInterval(async () => {
    const r = JSON.parse(await bridge().GetLog(logSince));
    if (r.lines.length) {
      logSince = r.total;
      const box = $("#log");
      if (!box) { clearInterval(buildTimer); return; }
      box.textContent += r.lines.join("\n") + "\n";
      box.scrollTop = box.scrollHeight;
    }
    const st = $("#bstate"); if (st) st.textContent = r.running ? "กำลัง build (BuildVer จะ +1 เมื่อสำเร็จ)..." : "พร้อม";
    if (!r.running) {
      $("#btnBuild").disabled = $("#btnClean").disabled = false;
      if (r.exit === 0) { const st2 = $("#bstate"); if (st2) st2.textContent = "BUILD OK — BuildVer " + (JSON.parse(await bridge().GetStatus()).buildNo); }
      clearInterval(buildTimer);
      refreshSide();
    }
  }, 600);
}

/* ── Version ── */
views.version = async function () {
  const c = await versionsCfg();
  const sampleText = String(c.version).replace("$build", c.buildNo);
  $("#main").innerHTML = `
    <h1>Version (Hardcore)</h1>
    <div class="sub">บังคับเวอร์ชันเดียวกันทุกโปรเจค — BuildVer นับปกติทุก build</div>
    <div class="card">
      <h2>HARDCORE VERSION</h2>
      <div class="row">
        <label class="switch">
          <input type="checkbox" id="hc" ${c.hardcore ? "checked" : ""}>
          <span class="slider"></span>
        </label>
        <span id="hcState" class="tag ${c.hardcore ? "on" : "off"}">${c.hardcore ? "HARDCORE ON" : "UNLOCKED"}</span>
      </div>
      <label class="f">VERSION (Major.Minor เช่น 3.41)</label>
      <input type="text" id="vVersion" value="${esc(c.version)}">
      <label class="f">ตัวอย่าง FileVersion ที่จะได้</label>
      <div class="stat"><div class="v green" id="vSample">${esc(sampleText)}</div></div>
      <div class="row" style="margin-top:16px">
        <button class="primary" id="btnSaveVer">บันทึก</button>
        <span class="sub" id="saveState"></span>
      </div>
    </div>
    <div class="card">
      <h2>VERSIONS รายโปรเจค</h2>
      <div class="sub" style="margin-bottom:10px">เวอร์ชันแยกตามโปรเจค — เว้นว่าง = ใช้ค่า global</div>
      <div style="max-height:420px; overflow-y:auto">
      <table id="verTable">
        <tr><th>โปรเจค</th><th>VERSION</th></tr>
        ${(c.projects || []).map((p) => `<tr><td>${esc(p.name)}</td><td><input type="text" class="pjver" data-pj="${esc(p.name)}" value="${esc(p.version)}" style="width:100%"></td></tr>`).join("")}
      </table>
      </div>
    </div>
    <div class="card">
      <h2>IDENTITY (เขียนทุก assembly)</h2>
      <label class="f">COMPANY</label>
      <input type="text" id="vCompany" value="${esc(c.company)}">
      <label class="f">AUTHORS</label>
      <input type="text" id="vAuthors" value="${esc(c.authors)}">
      <label class="f">PRODUCT</label>
      <input type="text" id="vProduct" value="${esc(c.product)}">
      <label class="f">COPYRIGHT</label>
      <input type="text" id="vCopyright" value="${esc(c.copyright)}">
    </div>`;
  const sample = () => {
    const v = $("#vVersion").value.trim() || "3.41";
    $("#vSample").textContent = v.replace("$build", c.buildNo);
  };
  $("#hc").onchange = () => { sample(); };
  $("#vVersion").oninput = sample;
  $("#btnSaveVer").onclick = async () => {
    $("#saveState").textContent = "บันทึก...";
    const projects = {};
    document.querySelectorAll(".pjver").forEach((inp) => {
      const n = inp.dataset.pj;
      if (n && inp.value.trim()) projects[n] = inp.value.trim();
    });
    const cfg = {
      hardcore: $("#hc").checked,
      global: $("#vVersion").value.trim(),
      projects,
      company: $("#vCompany").value.trim(),
      authors: $("#vAuthors").value.trim(),
      product: $("#vProduct").value.trim(),
      copyright: $("#vCopyright").value.trim(),
    };
    const r = JSON.parse(await bridge().SaveVersions(JSON.stringify(cfg)));
    $("#saveState").textContent = r.ok ? "บันทึกแล้ว ✓ (regen version.props)" : "ผิดพลาด: " + r.error;
    refreshSide();
  };
  window.sample = sample;
  sample();
};

/* ── Preview ── */
views.preview = async function () {
  const p = await preview();
  window.previewData = p;
  $("#main").innerHTML = `
    <h1>Preview — Build\NVIDIA ShadowPlay</h1>
    <div class="sub">ผลลัพธ์ staging ล่าสุด — ทุกไฟล์ .exe/.dll พร้อมเวอร์ชันจริงจากไฟล์</div>
    <div class="grid">
      <div class="stat"><div class="k">ขนาดรวม</div><div class="v green">${p.totalMB} MB</div></div>
      <div class="stat"><div class="k">ไฟล์ binary</div><div class="v">${p.fileCount}</div></div>
      <div class="stat"><div class="k">สถานะ</div><div class="v" style="font-size:14px">${p.exists ? "staged" : "ยังไม่ build"}</div></div>
    </div>
    <div class="card">
      <h2>BINARIES</h2>
      <div class="row">
        <input type="text" id="pvFilter" placeholder="กรองชื่อไฟล์..." style="max-width:280px" oninput="previewFilter()">
        <button class="primary" onclick="bridge().LaunchApp()">▶ รัน Launcher.exe</button>
        <button onclick="bridge().OpenFolder()">เปิดโฟลเดอร์</button>
        <button onclick="go('preview')">รีเฟรช</button>
      </div>
      <div style="max-height:480px; overflow-y:auto">
      <table id="pvTable">
        <tr><th>ไฟล์</th><th>ที่อยู่</th><th>FileVersion</th><th>ProductVersion</th><th>Company</th><th>Description</th><th>KB</th><th>แก้ล่าสุด</th></tr>
        ${(p.files || []).map((f) => `<tr><td>${esc(f.name)}</td><td style="color:var(--dim)">${esc(f.rel)}</td><td>${esc(f.ver)}</td><td>${esc(f.prod)}</td><td>${esc(f.comp)}</td><td>${esc(f.desc)}</td><td>${f.kb}</td><td>${esc(f.mod)}</td></tr>`).join("")}
      </table>
      </div>
    </div>`;
  window.previewFilter = () => {
    const q = ($("#pvFilter") ? $("#pvFilter").value : "").toLowerCase();
    document.querySelectorAll("#pvTable tr").forEach((tr, i) => {
      if (i === 0) return;
      tr.style.display = tr.textContent.toLowerCase().includes(q) ? "" : "none";
    });
  };
};

function refreshSide() {
  bridge().GetStatus().then((s) => {
    try { $("#sideBuildNo").textContent = JSON.parse(s).buildNo; } catch {}
  });
}

/* ── boot ── */
window.addEventListener("load", () => {
  go(location.hash.slice(1) || "dashboard");
  refreshSide();
  setInterval(refreshSide, 5000);
});
