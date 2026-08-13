# ShadowPlay Feedback Chat

A lightweight chat web app that lives entirely inside this repo.

## How it works

- **Storage:** `feedback/messages.json` (committed to this repo)
- **Backend:** GitHub Contents API (no separate server)
- **Hosting:** GitHub Pages (free, static)
- **Auth:** User-supplied GitHub PAT (stored in browser localStorage, never sent anywhere except github.com)

## Setup (one-time)

### 1. Enable GitHub Pages

1. Go to https://github.com/ScotcsDuluka/NVIDIA-Shadowplay/settings/pages
2. **Source:** Deploy from a branch
3. **Branch:** `Engine` / `/feedback` folder
4. Click **Save**

Wait 1–2 minutes. The chat will be live at:

```
https://scotcsduluka.github.io/NVIDIA-Shadowplay/feedback/
```

### 2. Create a Personal Access Token

1. Go to https://github.com/settings/tokens/new
2. **Scopes:** tick `repo` (full control of private repositories)
3. **Expiration:** 90 days (or whatever you prefer)
4. Generate token, copy the `ghp_…` value

### 3. Use the chat

1. Open the Pages URL above
2. Click the ⚙️ button (top-right)
3. Paste your PAT
4. Type a message, press Enter

The message is committed to `feedback/messages.json` on the `Engine` branch. The AI will pull the repo, read your message, edit the file directly, and push back. The chat auto-refreshes every 15 seconds to show the reply.

## Files

| File | Purpose |
|------|---------|
| `index.html` | Chat UI |
| `style.css` | NVIDIA-green dark theme |
| `app.js` | GitHub API client (fetch + PUT messages.json) |
| `messages.json` | The chat log itself |

## For the AI

After pulling this branch, open `feedback/messages.json`. Any new message from the user will be the last entry in the `messages` array. To reply:

1. Append a new object to the array:
   ```json
   {
     "id": "msg_<timestamp>",
     "author": "ai",
     "role": "assistant",
     "text": "Your reply here.",
     "timestamp": "<ISO 8601>"
   }
   ```
2. Commit and push to the `Engine` branch.

The web app polls every 15 seconds and will show your reply automatically.

## Notes

- The PAT is stored only in the browser's localStorage. It is never logged, never sent to any server other than api.github.com.
- If two people send a message at the exact same time, GitHub's Contents API will reject the second PUT with a SHA mismatch. The app shows an error and the user can retry.
- The chat history is public (the repo is public). Don't share secrets here.
