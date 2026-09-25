/* Build Tool SPA — hash router + views (bridge = window.chrome.webview.hostObjects.async.host) */
const bridge = () => window.chrome.webview.hostObjects.async.host;
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
async function preview() {
  try { return JSON.parse(await bridge().GetPreview()); }
  catch (e) { return { exists: false, files: [], totalMB: 0 }; }
}

/* ── Dashboard ── */
views.dashboard = async function () {
  const s = await status();
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
    $("#bstate").textContent = r.running ? "กำลัง build (BuildVer จะ +1 เมื่อสำเร็จ)..." : "พร้อม";
    if (!r.running) {
      $("#btnBuild").disabled = $("#btnClean").disabled = false;
      if (r.exit === 0) $("#bstate").textContent = "BUILD OK — BuildVer " + (JSON.parse(await bridge().GetStatus()).buildNo);
      clearInterval(buildTimer);
      refreshSide();
    }
  }, 600);
}

/* ── Version ── */
views.version = async function () {
  const c = await versionCfg();
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
      <div class="stat"><div class="v green" id="vSample">${sample()}</div></div>
      <div class="row" style="margin-top:16px">
        <button class="primary" id="btnSaveVer">บันทึก</button>
        <span class="sub" id="saveState"></span>
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
    const on = $("#hc").checked;
    const v = $("#vVersion").value.trim() || "3.41";
    $("#vSample").textContent = on ? `${v}.${c.buildNo}.61` : `3.41.${c.buildNo}.61 (unlock)`;
  };
  $("#hc").onchange = () => { sample(); };
  $("#vVersion").oninput = sample;
  $("#btnSaveVer").onclick = async () => {
    $("#saveState").textContent = "บันทึก...";
    const cfg = {
      hardcore: $("#hc").checked,
      version: $("#vVersion").value.trim(),
      company: $("#vCompany").value.trim(),
      authors: $("#vAuthors").value.trim(),
      product: $("#vProduct").value.trim(),
      copyright: $("#vCopyright").value.trim(),
    };
    const r = JSON.parse(await bridge().SaveVersionConfig(JSON.stringify(cfg)));
    $("#saveState").textContent = r.ok ? "บันทึกแล้ว ✓ (regen version.props)" : "ผิดพลาด: " + r.error;
    refreshSide();
  };
  window.sample = sample;
  sample();
};

/* ── Preview ── */
views.preview = async function () {
  const p = await preview();
  $("#main").innerHTML = `
    <h1>Preview — Build\\NVIDIA ShadowPlay</h1>
    <div class="sub">ผลลัพธ์ staging ล่าสุด — FileVersion อ่านจากไฟล์จริง</div>
    <div class="grid">
      <div class="stat"><div class="k">ขนาดรวม</div><div class="v green">${p.totalMB} MB</div></div>
      <div class="stat"><div class="k">EXE ในต้นไม้</div><div class="v">${(p.files || []).length}</div></div>
    </div>
    <div class="card">
      <h2>EXECUTABLES</h2>
      <div class="row">
        <button class="primary" onclick="bridge().LaunchApp()">▶ รัน Launcher.exe</button>
        <button onclick="bridge().OpenFolder()">เปิดโฟลเดอร์</button>
        <button onclick="go('preview')">รีเฟรช</button>
      </div>
      ${(p.files || []).length ? `<table>
        <tr><th>ไฟล์</th><th>ที่อยู่</th><th>FileVersion</th><th>Product</th><th>KB</th></tr>
        ${p.files.map((f) => `<tr><td>${esc(f.name)}</td><td style="color:var(--dim)">${esc(f.rel)}</td><td>${esc(f.ver)}</td><td>${esc(f.product)}</td><td>${f.kb}</td></tr>`).join("")}
      </table>` : `<div class="sub">ยังไม่มี staged tree — รัน Build ก่อน</div>`}
    </div>`;
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
