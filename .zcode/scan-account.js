// Scan the REAL NvAccountAPI.js for its jarvis client surface.
const fs = require('fs');
const src = fs.readFileSync('C:\\My Project\\gfe-3.28.0.412-extract\\nodejs\\NvAccountAPI.js', 'utf8');
const out = [];
src.split('\n').forEach((line, i) => {
    if (/app\.(get|post|put|delete)\(|jarvis|UserToken|\/Session|\/Login|\/UserInfo|isLoggedIn|loggedIn|userId|UserInfo/i.test(line)) {
        out.push((i + 1) + ': ' + line.trim().slice(0, 140));
    }
});
fs.writeFileSync('C:\\My Project\\NVIDIA-Shadowplay\\.zcode\\account-api-scan.txt', out.join('\r\n'));
console.log('SCAN ' + out.length + ' lines');
