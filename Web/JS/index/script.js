(function() {
  "use strict";

  const isTouchDevice = ('ontouchstart' in window) || (navigator.maxTouchPoints > 0) || (navigator.msMaxTouchPoints > 0);
  // ★ CURSOR FIX: the CSS hides the native cursor only on (hover:hover)+(pointer:fine)
  // and shows the custom cursor there. The old check (maxTouchPoints>0) also matched
  // touch-capable machines USED WITH A MOUSE (touchscreens, Remote Play/RDP sessions)
  // -> JS hid the custom cursor while CSS hid the real one = no cursor at all.
  // The cursor decision must use the SAME media query as the CSS.
  const hasFinePointer = window.matchMedia &&
    window.matchMedia('(hover: hover) and (pointer: fine)').matches;
  if (!hasFinePointer) {
    const cursor = document.getElementById('cursor');
    const cursorDot = document.getElementById('cursorDot');
    if (cursor) cursor.style.display = 'none';
    if (cursorDot) cursorDot.style.display = 'none';
  }
 
  document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function(e) {
      const href = this.getAttribute('href');
      if (href && href !== '#') {
        e.preventDefault();
        const target = document.querySelector(href);
        if (target) target.scrollIntoView({ behavior: 'smooth', block: 'start' });
        document.getElementById('mobileMenu')?.classList.remove('active');
        document.getElementById('hamburger')?.classList.remove('active');
        const hamburger = document.getElementById('hamburger');
        if (hamburger) hamburger.setAttribute('aria-expanded', 'false');
      }
    });
  });

  if (hasFinePointer) {
    const cursor = document.getElementById('cursor'), cursorDot = document.getElementById('cursorDot');
    let mouseX = 0, mouseY = 0, cursorX = 0, cursorY = 0;
    document.addEventListener('mousemove', (e) => {
      mouseX = e.clientX; mouseY = e.clientY;
      cursorDot.style.left = mouseX + 'px';
      cursorDot.style.top = mouseY + 'px';
      // First real mousemove: the custom cursor is proven drawing — only
      // now may the native cursor be hidden (see style.css CURSOR block).
      if (!document.body.classList.contains('custom-cursor-active')) {
        document.body.classList.add('custom-cursor-active');
      }
    });
    function animateCursor() {
      cursorX += (mouseX - cursorX) * 0.15;
      cursorY += (mouseY - cursorY) * 0.15;
      cursor.style.left = cursorX + 'px';
      cursor.style.top = cursorY + 'px';
      requestAnimationFrame(animateCursor);
    }
    animateCursor();
    document.querySelectorAll('a, button, .feature-card, .team-member, .btn-primary, .btn-outline, .api-spec-card, .arch-card').forEach(el => {
      el.addEventListener('mouseenter', () => cursor.classList.add('hover'));
      el.addEventListener('mouseleave', () => cursor.classList.remove('hover'));
    });
  }

  const reduceMotion = window.matchMedia('(prefers-reduced-motion: reduce)').matches;
  let particlesEnabled = !reduceMotion;
  const canvas = document.getElementById('particles-canvas');
  let ctx, particlesArray = [], animationId = null;
  const particleCount = (isTouchDevice || window.innerWidth < 768) ? 20 : 80;

  if (particlesEnabled && canvas) {
    ctx = canvas.getContext('2d');
    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;
    const mouse = { x: null, y: null, radius: 120 };

    if (!isTouchDevice) {
      window.addEventListener('mousemove', (e) => { mouse.x = e.x; mouse.y = e.y; });
    }

    class Particle {
      constructor() {
        this.x = Math.random() * canvas.width;
        this.y = Math.random() * canvas.height;
        this.size = Math.random() * 0.5 + 1;
        this.speedX = Math.random() * 1.2 - 0.5;
        this.speedY = Math.random() * 1.2 - 0.5;
        this.opacity = Math.random() * 0.9 + 0.4;
      }
      update() {
        this.x += this.speedX;
        this.y += this.speedY;
        if (this.x > canvas.width || this.x < 0) this.speedX *= -1;
        if (this.y > canvas.height || this.y < 0) this.speedY *= -1;
        if (mouse.x && mouse.y) {
          const dx = this.x - mouse.x;
          const dy = this.y - mouse.y;
          const distance = Math.sqrt(dx * dx + dy * dy);
          if (distance < mouse.radius) {
            const force = (mouse.radius - distance) / mouse.radius;
            const angle = Math.atan2(dy, dx);
            this.x += Math.cos(angle) * force * 3;
            this.y += Math.sin(angle) * force * 3;
            this.x += (Math.random() - 0.5) * 2;
            this.y += (Math.random() - 0.5) * 2;
          }
        }
      }
      draw() {
        ctx.fillStyle = `rgba(118, 185, 0, ${this.opacity})`;
        ctx.beginPath();
        ctx.arc(this.x, this.y, this.size, 0, Math.PI * 2);
        ctx.fill();
      }
    }

    function initParticles() {
      particlesArray = [];
      for (let i = 0; i < particleCount; i++) particlesArray.push(new Particle());
    }

    function animateParticles() {
      if (!ctx) return;
      ctx.clearRect(0, 0, canvas.width, canvas.height);
      particlesArray.forEach(p => { p.update(); p.draw(); });
      if (!isTouchDevice) {
        for (let a = 0; a < particlesArray.length; a++) {
          for (let b = a; b < particlesArray.length; b++) {
            const dx = particlesArray[a].x - particlesArray[b].x;
            const dy = particlesArray[a].y - particlesArray[b].y;
            const distance = Math.sqrt(dx * dx + dy * dy);
            if (distance < 120) {
              ctx.strokeStyle = `rgba(118, 185, 0, ${(110 - distance) / 120 * 0.95})`;
              ctx.lineWidth = 1;
              ctx.beginPath();
              ctx.moveTo(particlesArray[a].x, particlesArray[a].y);
              ctx.lineTo(particlesArray[b].x, particlesArray[b].y);
              ctx.stroke();
            }
          }
        }
      }
      animationId = requestAnimationFrame(animateParticles);
    }

    window.addEventListener('resize', () => {
      if (!particlesEnabled) return;
      canvas.width = window.innerWidth;
      canvas.height = window.innerHeight;
      initParticles();
    });
    initParticles();
    animateParticles();
  } else if (canvas) {
    canvas.style.display = 'none';
  }

  async function fetchGitHubStars() {
    const starElem = document.getElementById('starCount');
    if (!starElem) return;
    const cacheKey = 'github_stars_cache';
    const cacheTimeKey = 'github_stars_time';
    const now = Date.now();
    const cachedStars = localStorage.getItem(cacheKey);
    const cachedTime = localStorage.getItem(cacheTimeKey);
    if (cachedStars && cachedTime && (now - parseInt(cachedTime)) < 3600000) {
      starElem.setAttribute('data-target', cachedStars);
      starElem.innerText = cachedStars;
      return;
    }
    try {
      const res = await fetch('https://api.github.com/repos/ScotcsDuluka/NVIDIA-Shadowplay');
      if (!res.ok) throw new Error('GitHub API error');
      const data = await res.json();
      const stars = data.stargazers_count;
      if (stars !== undefined) {
        starElem.setAttribute('data-target', stars);
        starElem.innerText = stars;
        localStorage.setItem(cacheKey, stars);
        localStorage.setItem(cacheTimeKey, now);
      } else {
        throw new Error('No stars field');
      }
    } catch (e) {
      console.warn("GitHub API failed, using fallback or cached");
      if (cachedStars) {
        starElem.setAttribute('data-target', cachedStars);
        starElem.innerText = cachedStars;
      } else {
        starElem.innerText = '?';
        starElem.setAttribute('data-target', '0');
      }
    }
  }
  fetchGitHubStars();

  const counters = document.querySelectorAll('.stat-number');
  const observerOptions = { threshold: 0.5 };
  const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
      if (entry.isIntersecting) {
        const el = entry.target;
        let target = parseInt(el.getAttribute('data-target'));
        if (isNaN(target)) target = 0;
        let count = 0;
        const animate = () => {
          count += target / 200;
          if (count < target) {
            el.textContent = Math.ceil(count);
            requestAnimationFrame(animate);
          } else {
            el.textContent = target.toLocaleString();
          }
        };
        animate();
        observer.unobserve(el);
      }
    });
  }, observerOptions);
  counters.forEach(c => observer.observe(c));

  if (!isTouchDevice) {
    document.querySelectorAll('.btn-primary, .btn-outline, .feature-card, .team-member').forEach(btn => {
      btn.addEventListener('click', function(e) {
        const rect = this.getBoundingClientRect();
        const ripple = document.createElement('span');
        ripple.className = 'ripple-effect';
        ripple.style.left = (e.clientX - rect.left) + 'px';
        ripple.style.top = (e.clientY - rect.top) + 'px';
        this.appendChild(ripple);
        setTimeout(() => ripple.remove(), 600);
      });
    });
  }

  const ioSupported = 'IntersectionObserver' in window;

  document.querySelectorAll('section').forEach(section => {
    section.style.opacity = '0';
    section.style.transform = 'translateY(30px)';
    section.style.transition = 'opacity 0.6s ease, transform 0.6s ease';

    if (ioSupported) {
      const sectionObs = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
          if (entry.isIntersecting) {
            entry.target.style.opacity = '1';
            entry.target.style.transform = 'translateY(0)';
            sectionObs.unobserve(entry.target);
          }
        });
      }, { threshold: 0.05, rootMargin: '0px 0px -30px 0px' });
      sectionObs.observe(section);
    } else {
      section.style.opacity = '1';
      section.style.transform = 'translateY(0)';
    }
  });

  setTimeout(() => {
    document.querySelectorAll('section').forEach(section => {
      if (parseFloat(getComputedStyle(section).opacity) < 0.1) {
        section.style.opacity = '1';
        section.style.transform = 'translateY(0)';
      }
    });
  }, 1500);

  document.querySelectorAll('.animate-fade-up, .animate-scale').forEach(el => {
    if (ioSupported) {
      const animObs = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
          if (entry.isIntersecting) {
            entry.target.classList.add('visible');
            animObs.unobserve(entry.target);
          }
        });
      }, { threshold: 0.05, rootMargin: '0px 0px -30px 0px' });
      animObs.observe(el);
    } else {
      el.classList.add('visible');
    }
  });

  setTimeout(() => {
    document.querySelectorAll('.animate-fade-up, .animate-scale').forEach(el => {
      if (!el.classList.contains('visible')) el.classList.add('visible');
    });
  }, 1500);

  if (!isTouchDevice) {
    document.querySelectorAll('.tilt-card').forEach(card => {
      card.addEventListener('mousemove', (e) => {
        const rect = card.getBoundingClientRect();
        const x = e.clientX - rect.left, y = e.clientY - rect.top;
        card.style.transform = `perspective(1000px) rotateX(${((y - rect.height/2) / rect.height * -5)}deg) rotateY(${(x - rect.width/2) / rect.width * 5}deg) scale3d(1.02,1.02,1.02)`;
      });
      card.addEventListener('mouseleave', () => {
        card.style.transform = 'perspective(1000px) rotateX(0) rotateY(0) scale3d(1,1,1)';
      });
    });
    document.querySelectorAll('.magnetic-btn').forEach(btn => {
      btn.addEventListener('mousemove', (e) => {
        const rect = btn.getBoundingClientRect();
        btn.style.transform = `translate(${(e.clientX - rect.left - rect.width/2) * 0.2}px, ${(e.clientY - rect.top - rect.height/2) * 0.2}px)`;
      });
      btn.addEventListener('mouseleave', () => {
        btn.style.transform = 'translate(0,0)';
      });
    });
  }

  function updateProgressBar() {
    const progressBar = document.getElementById('scrollProgress');
    if (progressBar) {
      const winHeight = document.documentElement.scrollHeight - window.innerHeight;
      if (winHeight > 0) {
        progressBar.style.width = (window.scrollY / winHeight) * 100 + '%';
      }
    }
    requestAnimationFrame(updateProgressBar);
  }
  updateProgressBar();

  const hamburger = document.getElementById('hamburger');
  const mobileMenu = document.getElementById('mobileMenu');
  if (hamburger && mobileMenu) {
    hamburger.addEventListener('click', () => {
      const expanded = hamburger.getAttribute('aria-expanded') === 'true' ? false : true;
      hamburger.classList.toggle('active');
      mobileMenu.classList.toggle('active');
      hamburger.setAttribute('aria-expanded', expanded);
    });
    document.querySelectorAll('.mobile-menu a').forEach(link => {
      link.addEventListener('click', () => {
        hamburger.classList.remove('active');
        mobileMenu.classList.remove('active');
        hamburger.setAttribute('aria-expanded', 'false');
      });
    });
  }
  if (isTouchDevice && mobileMenu && hamburger) {
    document.addEventListener('touchstart', (e) => {
      if (mobileMenu.classList.contains('active') && !mobileMenu.contains(e.target) && !hamburger.contains(e.target)) {
        mobileMenu.classList.remove('active');
        hamburger.classList.remove('active');
        hamburger.setAttribute('aria-expanded', 'false');
      }
    });
  }

  document.querySelectorAll('.faq-question').forEach(q => {
    q.addEventListener('click', () => { q.parentElement.classList.toggle('active'); });
  });

  const yearEl = document.getElementById('currentYear');
  if (yearEl) yearEl.innerText = new Date().getFullYear();

  function spawnTapEffect(clientX, clientY) {
    const wave = document.createElement('div');
    wave.className = 'click-wave';
    wave.style.left = clientX + 'px';
    wave.style.top = clientY + 'px';
    document.body.appendChild(wave);
    setTimeout(() => wave.remove(), 600);
    for (let i = 0; i < 8; i++) {
      const p = document.createElement('div');
      p.className = 'click-particle';
      p.style.left = clientX + 'px';
      p.style.top = clientY + 'px';
      const angle = Math.random() * 2 * Math.PI;
      const distance = Math.random() * 60 + 20;
      p.style.setProperty('--x', Math.cos(angle) * distance + 'px');
      p.style.setProperty('--y', Math.sin(angle) * distance + 'px');
      document.body.appendChild(p);
      setTimeout(() => p.remove(), 800);
    }
  }
  // ★ Same root cause as the cursor fix: gate on the fine-pointer media
  // query (aligned with CSS), not maxTouchPoints — touch-capable machines
  // driven by a mouse were treated as touch-only and lost the tap effect.
  if (hasFinePointer) {
    document.addEventListener('click', (e) => spawnTapEffect(e.clientX, e.clientY));
  }

  function fixOverlayCardGrid() {
    const overlayInner = document.querySelector('.overlay-card-inner');
    if (overlayInner) {
      overlayInner.style.gridTemplateColumns = window.innerWidth <= 768 ? '1fr' : '1fr 1fr';
    }
  }
  fixOverlayCardGrid();
  window.addEventListener('resize', fixOverlayCardGrid);

  window.downloadAndScroll = function(e) {
    e.preventDefault();
    document.getElementById("download-section").scrollIntoView({ behavior: "smooth" });
  };
})();



document.querySelectorAll('.req-tab').forEach(tab => {
  tab.addEventListener('click', () => {
    document.querySelectorAll('.req-tab').forEach(t => t.classList.remove('active'));
    document.querySelectorAll('.req-tab-content').forEach(c => c.classList.remove('active'));
    tab.classList.add('active');
    const tabId = tab.getAttribute('data-tab');
    document.getElementById(`tab-${tabId}`).classList.add('active');
    setTimeout(() => {
      document.querySelectorAll('.score-fill').forEach(fill => {
        const score = fill.getAttribute('data-score');
        fill.style.width = `${score}%`;
      });
    }, 300);
  });
});