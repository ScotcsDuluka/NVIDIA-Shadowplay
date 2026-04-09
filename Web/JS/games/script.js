 (function() {
      // ---------- PARTICLES ----------
      const canvas = document.getElementById('particles-canvas');
      if (canvas) {
        const ctx = canvas.getContext('2d');
        let particles = [];
        const particleCount = 35;
        function resizeCanvas() { canvas.width = window.innerWidth; canvas.height = window.innerHeight; }
        window.addEventListener('resize', resizeCanvas); resizeCanvas();
        class Particle {
          constructor() { this.x = Math.random()*canvas.width; this.y = Math.random()*canvas.height; this.size = Math.random()*1.5+0.8; this.speedX = Math.random()*1.2-0.5; this.speedY = Math.random()*1.2-0.5; this.opacity = Math.random()*0.7+0.3; }
          update() { this.x += this.speedX; this.y += this.speedY; if(this.x<0||this.x>canvas.width) this.speedX*=-1; if(this.y<0||this.y>canvas.height) this.speedY*=-1; }
          draw() { ctx.fillStyle = `rgba(120,184,2,${this.opacity})`; ctx.beginPath(); ctx.arc(this.x, this.y, this.size, 0, Math.PI*2); ctx.fill(); }
        }
        for(let i=0; i<particleCount; i++) particles.push(new Particle());
        function animate() { ctx.clearRect(0,0,canvas.width,canvas.height); particles.forEach(p=>{ p.update(); p.draw(); }); requestAnimationFrame(animate); }
        animate();
      }

      // ---------- PROCESS LIST (ชื่อที่ตรวจจับได้) พร้อม mapping เป็นชื่อเกมและสถานะ ----------
      const processMapping = {
        "minecraft": { display: "Minecraft", status: "full" },
        "javaw": { display: "Minecraft (Java)", status: "full" },
        "robloxplayerbeta": { display: "Roblox", status: "full" },
        "robloxcrashhandler": { display: "Roblox", status: "full" },
        "java": { display: "Java Edition", status: "full" },
        "crashhandler": { display: "Crash Handler", status: "limited" },
        "gta5": { display: "GTA V", status: "borderless" },
        "hd-player": { display: "HD Player", status: "limited" },
        "a dance of fire and ice": { display: "A Dance of Fire and Ice", status: "borderless" },
        "aot": { display: "Attack on Titan", status: "full" },
        "aot2_as": { display: "Attack on Titan 2", status: "full" },
        "iw5mp": { display: "CoD: MW3 Multiplayer", status: "limited" },
        "iw5sp": { display: "CoD: MW3 Singleplayer", status: "limited" },
        "obscure": { display: "Obscure", status: "full" },
        "genshinimpact": { display: "Genshin Impact", status: "borderless" },
        "gta5_enhanced": { display: "GTA V Enhanced", status: "borderless" },
        "dwrg": { display: "Dead by Daylight", status: "full" },
        "dungeons": { display: "Minecraft Dungeons", status: "full" },
        "minecraftlegends.windows": { display: "Minecraft Legends", status: "full" },
        "secret neighbour": { display: "Secret Neighbor", status: "full" },
        "smash_legends": { display: "Smash Legends", status: "full" },
        "asphalt9_steam_x64_rtl": { display: "Asphalt 9: Legends", status: "full" },
        "furmark_gui": { display: "FurMark", status: "limited" },
        "misidefull": { display: "MiSide", status: "full" },
        "miside zero": { display: "MiSide Zero", status: "full" },
        "HSHO-Win64-Shipping": { display: "Hello Neighbor", status: "full" },
        "re9": { display: "Resident Evil 9", status: "limited" },
        "re4": { display: "Resident Evil 4", status: "full" }
      };

      const processList = [
        "minecraft", "javaw", "robloxplayerbeta", "robloxcrashhandler", "java",
        "crashhandler", "gta5", "hd-player", "a dance of fire and ice", "aot",
        "aot2_as", "iw5mp", "iw5sp", "obscure", "genshinimpact", "gta5_enhanced",
        "dwrg", "dungeons", "minecraftlegends.windows", "secret neighbour",
        "smash_legends", "asphalt9_steam_x64_rtl", "furmark_gui", "misidefull",
        "miside zero", "HSHO-Win64-Shipping", "re9", "re4"
      ];

      // สร้าง gamesData แบบกระชับ (แค่ชื่อกับ status)
      const gamesData = processList.map(proc => {
        const mapped = processMapping[proc] || { display: proc, status: "full" };
        return {
          name: mapped.display,
          status: mapped.status
        };
      }).sort((a, b) => a.name.localeCompare(b.name));

      // ---------- RENDER (ไม่มีไอคอน) ----------
      const container = document.getElementById('gamesContainer');
      const searchInput = document.getElementById('searchInput');
      const filterBtns = document.querySelectorAll('.filter-tag');
      let currentFilter = 'all', searchTerm = '';

      function render() {
        const filtered = gamesData.filter(g => 
          (currentFilter === 'all' || g.status === currentFilter) &&
          (!searchTerm || g.name.toLowerCase().includes(searchTerm.toLowerCase()))
        );
        if (!filtered.length) {
          container.innerHTML = '<p style="grid-column:1/-1;text-align:center;color:var(--text-gray);padding:3rem;">No games found</p>';
          return;
        }
        container.innerHTML = filtered.map((g, i) => {
          const cls = g.status === 'full' ? 'status-full' : g.status === 'borderless' ? 'status-borderless' : 'status-limited';
          const txt = g.status === 'full' ? 'FULL SUPPORT' : g.status === 'borderless' ? 'BORDERLESS' : 'LIMITED';
          return `
            <div class="game-card" style="--index:${i}">
              <div class="game-name">${g.name}</div>
              <span class="game-status ${cls}">${txt}</span>
            </div>
          `;
        }).join('');
      }

      searchInput.addEventListener('input', (e) => { searchTerm = e.target.value; render(); });
      filterBtns.forEach(btn => btn.addEventListener('click', () => {
        filterBtns.forEach(b => b.classList.remove('active'));
        btn.classList.add('active');
        currentFilter = btn.dataset.filter;
        render();
      }));
      render();

      document.getElementById('backButton').addEventListener('click', (e) => {
        e.preventDefault();
        document.body.classList.add('page-exit');
        setTimeout(() => { window.location.href = 'index.html'; }, 250);
      });
    })();