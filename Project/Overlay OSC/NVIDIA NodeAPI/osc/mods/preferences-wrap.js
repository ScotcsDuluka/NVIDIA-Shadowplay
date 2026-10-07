// preferences-wrap.js — ครอบ .preferences-root-panel ด้วย
// <div class="osc-central-div layout-column" layout="column"> (สเปก OWNER 05:0x)
// รันซ้ำปลอดภัย (เช็คก่อนครอบ) + จับ element ที่เกิดใหม่หลังเปลี่ยนหน้าทุก 500ms
(function () {
    function wrap() {
        document.querySelectorAll('.preferences-root-panel').forEach(function (root) {
            var p = root.parentElement;
            if (p && p.classList.contains('osc-central-div')) return; // ครอบแล้ว
            var w = document.createElement('div');
            w.className = 'osc-central-div layout-column';
            w.setAttribute('layout', 'column');
            p.insertBefore(w, root);
            w.appendChild(root);
        });
    }
    wrap();
    setInterval(wrap, 500);
})();
