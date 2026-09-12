import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { createApp } from './app.js';

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const root = path.resolve(__dirname, '..');

const port = Number(process.env.PORT) || 3000;
const dbPath = process.env.TASKBOARD_DB || path.join(root, 'data', 'taskboard.db');
const staticDir = path.join(root, 'public');

const { server, freshlySeeded } = createApp({ dbPath, staticDir });

server.listen(port, () => {
  console.log('');
  console.log('  TASK BOARD - Mission Control');
  console.log(`  http://localhost:${port}`);
  console.log('');
  console.log(`  Database : ${dbPath}`);
  if (freshlySeeded) {
    console.log('  Seeded   : M1-M4 / W1-W3 example data + users below');
  }
  console.log('  Accounts : owner/owner123 (OWNER)  admin/admin123 (ADMIN)  viewer/viewer123 (VIEWER)');
  console.log('');
  console.log('  Semantics: TASK != STATUS, REPORT != STATUS - status only changes via explicit actions.');
  console.log('');
});
