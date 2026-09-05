# Playwright MCP — control GLM 1/2/3 tabs

This setup uses the official Microsoft Playwright MCP server in **browser extension mode** so it can connect to tabs already open in Chrome, including your logged-in GLM tabs.

## One-time Chrome step

1. In Chrome, install the official **Playwright MCP Chrome Extension** from the Microsoft Playwright MCP repository:
   https://github.com/microsoft/playwright-mcp/tree/main/packages/extension
2. Pin the extension so it is easy to find.
3. Keep the Chrome window with GLM 1, GLM 2, and GLM 3 open.
4. If Chrome asks for permission to connect to a tab, approve it for the GLM tabs.

## Add it to Desktop Commander

1. Open **Apps**.
2. Choose **Add Custom MCP**.
3. Add the `playwright-glm-tabs` server using `playwright-mcp-config.json`.
4. Save the app connection and restart Desktop Commander if requested.

## What this configuration does

- Connects to the existing Chrome browser through the Playwright Extension.
- Lets the assistant list and switch among the GLM 1/2/3 tabs.
- Uses the existing login/session already present in those tabs.
- Saves browser output under the project's `tools/playwright/browser` folder.
- Contains no passwords, tokens, or other credentials.

## Safe first test

After adding the MCP, ask:

> List the open tabs and identify which one is GLM 1, GLM 2, and GLM 3. Do not click, type, submit, or change anything.

## Important safety rule

I can read and navigate the tabs, but I will ask before sending prompts, clicking buttons, submitting forms, or changing account/data state. Never paste passwords or API keys into chat.

## Source

https://github.com/microsoft/playwright-mcp
