/* ShadowPlay Feedback Chat — client-side logic
 *
 * Storage: messages.json in this repo (committed via GitHub Contents API).
 * Auth: user-supplied PAT, stored in localStorage.
 *
 * Workflow:
 *   1. User types message → app GETs messages.json (with SHA) → appends
 *      new message → PUTs the file back to GitHub.
 *   2. App polls messages.json every 15s for new replies from the AI.
 *   3. AI pulls the repo, edits messages.json directly, commits + pushes.
 */

(() => {
  "use strict";

  // ── Config ────────────────────────────────────────────────────────────
  const REPO_OWNER = "ScotcsDuluka";
  const REPO_NAME  = "NVIDIA-Shadowplay";
  const BRANCH     = "Engine";
  const FILE_PATH  = "feedback/messages.json";
  const RAW_URL    = `https://raw.githubusercontent.com/${REPO_OWNER}/${REPO_NAME}/${BRANCH}/${FILE_PATH}`;
  const API_BASE   = `https://api.github.com/repos/${REPO_OWNER}/${REPO_NAME}/contents/${FILE_PATH}`;
  const POLL_MS    = 15000;  // 15s
  const STORAGE_KEY = "sp_feedback_token";

  // ── DOM ───────────────────────────────────────────────────────────────
  const chatEl       = document.getElementById("chat");
  const inputEl      = document.getElementById("input");
  const sendBtn      = document.getElementById("send-btn");
  const statusDot    = document.getElementById("status-dot");
  const statusText   = document.getElementById("status-text");
  const settingsBtn  = document.getElementById("settings-btn");
  const modal        = document.getElementById("settings-modal");
  const tokenInput   = document.getElementById("token-input");
  const settingsSave = document.getElementById("settings-save");
  const settingsCancel = document.getElementById("settings-cancel");

  // ── State ─────────────────────────────────────────────────────────────
  let token = localStorage.getItem(STORAGE_KEY) || "";
  let lastFileSha = null;
  let lastMessageCount = 0;
  let pollTimer = null;
  let isSending = false;

  // ── Utility ───────────────────────────────────────────────────────────
  function setStatus(state, text) {
    statusDot.className = "dot dot-" + state; // "on" | "off" | "err"
    statusText.textContent = text;
  }

  function fmtTime(iso) {
    try {
      const d = new Date(iso);
      return d.toLocaleString(undefined, {
        month: "short", day: "numeric",
        hour: "2-digit", minute: "2-digit"
      });
    } catch { return ""; }
  }

  function escapeHtml(s) {
    return String(s)
      .replace(/&/g, "&amp;")
      .replace(/</g, "&lt;")
      .replace(/>/g, "&gt;")
      .replace(/"/g, "&quot;");
  }

  function autoGrow() {
    inputEl.style.height = "auto";
    inputEl.style.height = Math.min(inputEl.scrollHeight, 120) + "px";
  }

  // ✅ P2.7: UTF-8 safe base64 encode/decode. Old code used
  // btoa(unescape(encodeURIComponent(str))) which is a deprecated hack —
  // it corrupts multi-byte emoji (👋, ⚙️) and Thai text into mojibake
  // like ÃÂÃÂ. Use TextEncoder/TextDecoder for proper UTF-8 handling.
  function utf8ToBase64(str) {
    const bytes = new TextEncoder().encode(str);
    let bin = "";
    for (const b of bytes) bin += String.fromCharCode(b);
    return btoa(bin);
  }
  function base64ToUtf8(b64) {
    const bin = atob(b64.replace(/\n/g, ""));
    const bytes = new Uint8Array(bin.length);
    for (let i = 0; i < bin.length; i++) bytes[i] = bin.charCodeAt(i);
    return new TextDecoder().decode(bytes);
  }

  // ── Render ────────────────────────────────────────────────────────────
  function render(data) {
    if (!data || !Array.isArray(data.messages)) {
      chatEl.innerHTML = `<div class="msg msg-system"><div class="bubble">No messages yet.</div></div>`;
      return;
    }

    // Only re-render if message count changed (avoid destroying input focus).
    if (data.messages.length === lastMessageCount) return;
    lastMessageCount = data.messages.length;

    chatEl.innerHTML = "";
    for (const m of data.messages) {
      const wrap = document.createElement("div");
      const role = m.role || "user";
      wrap.className = "msg msg-" + role;

      const bubble = document.createElement("div");
      bubble.className = "bubble";
      bubble.textContent = m.text || "";

      const meta = document.createElement("div");
      meta.className = "msg-meta";
      const who = role === "assistant" ? "AI" : role === "system" ? "system" : (m.author || "you");
      meta.textContent = `${who} · ${fmtTime(m.timestamp)}`;

      wrap.appendChild(bubble);
      wrap.appendChild(meta);
      chatEl.appendChild(wrap);
    }
    // Scroll to bottom.
    chatEl.scrollTop = chatEl.scrollHeight;
  }

  // ── Fetch messages ────────────────────────────────────────────────────
  async function fetchMessages() {
    try {
      // Use API endpoint (not raw) so we get the SHA for updates.
      const headers = { "Accept": "application/vnd.github.v3+json" };
      if (token) headers["Authorization"] = `token ${token}`;

      const r = await fetch(API_BASE + "?ref=" + BRANCH, { headers });
      if (!r.ok) throw new Error(`HTTP ${r.status}`);

      const data = await r.json();
      lastFileSha = data.sha;

      const content = JSON.parse(base64ToUtf8(data.content));
      render(content);
      setStatus(token ? "on" : "off", token ? "Connected · polling" : "Read-only (set token to send)");
      return content;
    } catch (err) {
      setStatus("err", "Error: " + err.message);
      return null;
    }
  }

  // ── Send message ──────────────────────────────────────────────────────
  async function sendMessage() {
    if (isSending) return;
    const text = inputEl.value.trim();
    if (!text) return;

    if (!token) {
      openSettings();
      return;
    }

    isSending = true;
    sendBtn.disabled = true;
    sendBtn.textContent = "Sending…";
    setStatus("off", "Sending…");

    try {
      // 1. Get latest file (with SHA) — re-fetch in case it changed.
      const r = await fetch(API_BASE + "?ref=" + BRANCH, {
        headers: {
          "Authorization": `token ${token}`,
          "Accept": "application/vnd.github.v3+json"
        }
      });
      if (!r.ok) throw new Error(`fetch failed: HTTP ${r.status}`);
      const fileData = await r.json();
      lastFileSha = fileData.sha;

      const content = JSON.parse(base64ToUtf8(fileData.content));

      // 2. Append new message.
      const newMsg = {
        id: "msg_" + Date.now().toString(36),
        author: "user",
        role: "user",
        text: text,
        timestamp: new Date().toISOString()
      };
      content.messages.push(newMsg);

      // 3. PUT back to GitHub.
      const putResp = await fetch(API_BASE, {
        method: "PUT",
        headers: {
          "Authorization": `token ${token}`,
          "Accept": "application/vnd.github.v3+json",
          "Content-Type": "application/json"
        },
        body: JSON.stringify({
          message: `feedback: ${text.slice(0, 50)}${text.length > 50 ? "…" : ""}`,
          content: utf8ToBase64(JSON.stringify(content, null, 2)),
          sha: lastFileSha,
          branch: BRANCH
        })
      });

      if (!putResp.ok) {
        const errBody = await putResp.json().catch(() => ({}));
        throw new Error(`PUT failed: HTTP ${putResp.status} — ${errBody.message || ""}`);
      }

      // 4. Update SHA + render immediately.
      const putData = await putResp.json();
      lastFileSha = putData.content.sha;
      render(content);
      inputEl.value = "";
      autoGrow();
      setStatus("on", "Sent · AI will reply soon");
    } catch (err) {
      setStatus("err", "Send failed: " + err.message);
      alert("Send failed: " + err.message + "\n\nIf the error mentions 'sha', someone else sent a message at the same time. Please retry.");
    } finally {
      isSending = false;
      sendBtn.disabled = false;
      sendBtn.textContent = "Send";
    }
  }

  // ── Settings modal ────────────────────────────────────────────────────
  function openSettings() {
    tokenInput.value = token;
    modal.classList.remove("hidden");
    setTimeout(() => tokenInput.focus(), 50);
  }
  function closeSettings() {
    modal.classList.add("hidden");
  }
  function saveSettings() {
    token = tokenInput.value.trim();
    if (token) {
      localStorage.setItem(STORAGE_KEY, token);
    } else {
      localStorage.removeItem(STORAGE_KEY);
    }
    closeSettings();
    fetchMessages();
  }

  // ── Polling ───────────────────────────────────────────────────────────
  function startPolling() {
    stopPolling();
    pollTimer = setInterval(fetchMessages, POLL_MS);
  }
  function stopPolling() {
    if (pollTimer) { clearInterval(pollTimer); pollTimer = null; }
  }

  // ── Events ────────────────────────────────────────────────────────────
  sendBtn.addEventListener("click", sendMessage);
  inputEl.addEventListener("input", autoGrow);
  inputEl.addEventListener("keydown", (e) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      sendMessage();
    }
  });
  settingsBtn.addEventListener("click", openSettings);
  settingsSave.addEventListener("click", saveSettings);
  settingsCancel.addEventListener("click", closeSettings);
  modal.addEventListener("click", (e) => {
    if (e.target === modal) closeSettings();
  });

  // ── Init ──────────────────────────────────────────────────────────────
  async function init() {
    if (!token) {
      setStatus("off", "Read-only — click ⚙️ to set token");
    }
    await fetchMessages();
    startPolling();
  }
  init();
})();
