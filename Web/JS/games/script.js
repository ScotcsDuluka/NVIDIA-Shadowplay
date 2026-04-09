(function() {
  // ---------- (Optional) Particles Animation ----------
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

  // ----------  Load Games from JSON ----------
  const container = document.getElementById('gamesContainer');
  const searchInput = document.getElementById('searchInput');
  const filterBtns = document.querySelectorAll('.filter-tag');
  let currentFilter = 'all', searchTerm = '', gamesData = [];

  fetch('/JSON/games/games.json')
    .then(response => response.json())
    .then(data => {
      gamesData = data.map(item => ({
        name: item.display,
        status: item.status
      })).sort((a, b) => a.name.localeCompare(b.name));
      render();
    })
    .catch(err => {
      console.error('Failed to load games.json:', err);
      container.innerHTML = '<p style="grid-column:1/-1;text-align:center;color:red;">Error loading game list.</p>';
    });

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

  document.getElementById('backButton').addEventListener('click', (e) => {
    e.preventDefault();
    document.body.classList.add('page-exit');
    setTimeout(() => { window.location.href = 'index.html'; }, 250);
  });
})();