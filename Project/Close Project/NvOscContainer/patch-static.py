import io, re
p = r'C:\My Project\NVIDIA-Shadowplay\Project\Overlay OSC\NVIDIA NodeAPI\index.js'
s = io.open(p, encoding='utf-8').read()

# 0) ถ้า static เคยใส่ใน OnContextInitialized ค้าง — ถอดก่อน
s = s.replace(
    "    // OUR LANE: serve osc page ของเรา (same origin กับ API/socket = หมดปัญหา CORS/port)\n"
    "    app.use(require('express').static('C:\\\\My Project\\\\NVIDIA-Shadowplay\\\\Project\\\\Overlay OSC\\\\NVIDIA OSC\\\\osc'));\n",
    "")

# 1) static ที่ module scope หลังสร้าง app (path forward slash — กัน escape พัง)
old = "var app = require('./node_modules/express/index.js')();"
new = old + "\n// OUR LANE: serve osc page ของเราก่อน route อื่น (same origin = หมดปัญหา CORS/port)\napp.use(require('express').static('C:/My Project/NVIDIA-Shadowplay/Project/Overlay OSC/NVIDIA OSC/osc'));"
print('app anchor found:', old in s)
s = s.replace(old, new, 1)

io.open(p, 'w', encoding='utf-8', newline='').write(s)
print('static at module scope OK')
