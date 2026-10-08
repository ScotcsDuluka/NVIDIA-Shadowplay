import io
p = r'C:\My Project\NVIDIA-Shadowplay\Project\Overlay OSC\NVIDIA NodeAPI\index.js'
s = io.open(p, encoding='utf-8').read()

# 1) static: serve osc dir ของเรา (หน้า + app.js patch 59011) — ก่อน route อื่น
old1 = """    app.get('/Backend/v.1.0/health', function (req, res) {"""
new1 = """    // OUR LANE: serve osc page ของเรา (same origin กับ API/socket = หมดปัญหา CORS/port)
    app.use(require('express').static('C:\\\\My Project\\\\NVIDIA-Shadowplay\\\\Project\\\\Overlay OSC\\\\NVIDIA OSC\\\\osc'));
    app.get('/Backend/v.1.0/health', function (req, res) {"""
print('health anchor found:', old1 in s)
s = s.replace(old1, new1, 1)

# 2) /cefquery — NODE_INFO ให้หน้า (หลัง hk routes)
old2 = """    app.get('/', hkHandler);
    app.post('/', hkHandler);"""
new2 = """    app.get('/', hkHandler);
    app.post('/', hkHandler);
    // OUR LANE: cefQuery — หน้าส่ง {command} มาทาง POST /cefquery (cefquery-shim.js)
    // QUERY_WIN_NODE_INFO = หน้าต้องใช้ก่อนวาดเมนู (route resolve localNodeInfo)
    app.post('/cefquery', function (req, res) {
        var body = '';
        req.on('data', function (c) { body += c; });
        req.on('end', function () {
            var reply = '{}';
            try {
                var q = JSON.parse(body || '{}');
                if (q.command === 'QUERY_WIN_NODE_INFO') {
                    reply = JSON.stringify({
                        port: 59011, disableSecurity: true, securityCookie: '',
                        gfwsl: { server: 'https://gfwsl.geforce.com/' },
                        jarvis: cfg.jarvis, gxtarget: cfg.gxtarget
                    });
                }
            } catch (e) {}
            res.header('Content-Type', 'application/json');
            res.header('Access-Control-Allow-Origin', '*');
            res.end(reply);
        });
    });"""
print('hk anchor found:', old2 in s)
s = s.replace(old2, new2, 1)
io.open(p, 'w', encoding='utf-8', newline='').write(s)
print('index.js updated')
