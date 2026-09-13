# 13 — DULUKA ACCOUNT (replacing the NVIDIA account layer)

Owner decision (2026-09-13): the real GFE/OSC system is installed and
mapped (see 12-real-system-map.md). The NVIDIA **account/cloud layer**
(`jarvis` account server, `gfwsl`, telemetry) is replaced by **Duluka
Account**. The osc UI stays the real NVIDIA bundle; only the endpoint
configuration and the Duluka-side API are ours.

## How the osc account layer works (measured)

- Account endpoints are NOT hardcoded — they arrive at runtime:
  1. boot reads `OSC_CONFIG` (config.js; servers mostly "")
  2. host/socket pushes `/PiplConfig/v.1.0/update` with the
     `{jarvis, gfwsl, aem, vrs, jsEvents, nvTelemetry}` structure
     (real sample captured in 12-real-system-map.md)
  3. page swaps `jarvis.server` (account), `gfwsl.server` (game/hw),
     `jsEvents.server` (telemetry) at runtime
- Login flows use `jarvis` (NVIDIA account session) + OAuth redirect
  capture via `QUERY_HTTPSERVER_START` (page opens browser → loopback
  HTTP capture on ports 2259/6460/7119/8870/9096, `redirectUriOverride:
  "http://localhost:{{portNumber}}"`).

## Duluka Account design (M-DLA)

```
osc UI ──(unchanged jarvis API calls)──▶ jarvis.server = Duluka Auth Server
                                        (https://<duluka>/... same shapes)
Duluka Auth Server
  ├─ POST /login, /token      (Duluka credentials — replaces NVIDIA login)
  ├─ GET  /user               (profile the UI displays)
  └─ optional /upload, /broadcast mirrors (M3)
```

### Host-side wiring (OscControllerServer)

`/PiplConfig/v.1.0/data` (GET) and `/PiplConfig/v.1.0/update` (push) will
serve:

```json
{ "data": { "configData": {
    "jarvis":   { "server": "https://account.duluka.dev" },
    "gfwsl":    { "server": "" },
    "aem":      { "server": "" },
    "vrs":      { "server": "" },
    "jsEvents": { "server": "" },
    "nvTelemetry": { "eventsServer": "", "feedbackServer": "", "feedbackAttachmentServer": "" }
} } }
```

→ NVIDIA telemetry/cloud disabled; account = Duluka.

### Duluka API surface (first slice — mirror the jarvis shapes the UI calls)

To be finalized by probing the page's jarvis calls with a live jarvis
server value pointed at a local listener (log-then-shape). Known needed
minimum for the UI to show a logged-in state:
- session/user fetch (display name/avatar)
- login entry point (the page opens OAuth in browser; Duluka can host a
  simple login page that redirects to the loopback capture the page
  already implements)

## FACT / INTENTIONAL / UNKNOWN

- FACT: endpoint injection point = piplConfig (captured live file).
- FACT: page swaps servers at runtime from that push.
- INTENTIONAL: telemetry/NVIDIA cloud disabled by empty servers.
- UNKNOWN: exact jarvis REST paths/shapes (next step: probe with a
  listener while clicking login in the real osc).
