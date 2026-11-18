/* ============================================================================
 * HENRY HOMESTAY — LIVE C# ASP.NET CORE MVC BRIDGE
 * Kết nối trực tiếp giao diện HTML/CSS/JS với CSDL thông qua Controllers & BLL
 * ============================================================================ */
window.HenryLive = (function () {
  const money = (v) => new Intl.NumberFormat('vi-VN').format(Math.round(v || 0)) + ' ₫';
  const fmtDateVi = (isoStr) => {
    if (!isoStr) return '';
    const d = new Date(isoStr.slice(0, 10) + 'T12:00:00');
    return d.toLocaleDateString('vi-VN', { day: '2-digit', month: 'long', year: 'numeric' });
  };

  function isUserAuthenticated() {
    if (document.body && document.body.dataset.auth === 'guest') {
      if (typeof HenryData !== 'undefined') HenryData.currentUser = null;
      return false;
    }
    const hasAuthCookie = document.cookie.split(';').some(c => c.trim().startsWith('HenryUserId='));
    const hasLocalFlag = localStorage.getItem('henry-logged-in') === '1';
    const isAuthed = location.protocol.startsWith('http') ? hasAuthCookie : (hasAuthCookie || hasLocalFlag);
    if (!isAuthed) {
      if (typeof HenryData !== 'undefined') HenryData.currentUser = null;
      return false;
    }
    return typeof HenryData !== 'undefined' && !!HenryData.currentUser;
  }

  // Immediately enforce auth state before any rendering
  isUserAuthenticated();

  function showAuthRequiredModal() {
    if (typeof showToast === 'function') {
      showToast('Vui lòng đăng nhập hoặc đăng ký tài khoản để đặt phòng!');
    } else if (typeof Henry !== 'undefined' && typeof Henry.toast === 'function') {
      Henry.toast('Vui lòng đăng nhập hoặc đăng ký tài khoản để đặt phòng!');
    }

    let modal = document.getElementById('authRequiredModal');
    if (!modal) {
      modal = document.createElement('div');
      modal.id = 'authRequiredModal';
      modal.setAttribute('data-purpose', 'auth-required-modal');
      modal.style.cssText = 'position:fixed;inset:0;z-index:9999;display:flex;align-items:center;justify-content:center;padding:16px;background:rgba(24,18,12,0.65);backdrop-filter:blur(6px);';
      modal.innerHTML = `
        <div style="position:relative;width:100%;max-width:480px;background:#fff;border-radius:24px;padding:32px 28px;box-shadow:0 25px 50px -12px rgba(0,0,0,0.35);border:1px solid #e8ded5;text-align:center;font-family:'Plus Jakarta Sans',system-ui,sans-serif;color:#2A1D17;">
          <button type="button" onclick="closeAuthRequiredModal()" aria-label="Đóng" style="position:absolute;top:16px;right:16px;width:36px;height:36px;border-radius:50%;border:1px solid #e7e5e4;background:#fafaf9;color:#57534e;display:flex;align-items:center;justify-content:center;cursor:pointer;font-size:18px;">✕</button>
          <div style="width:56px;height:56px;margin:0 auto 16px;border-radius:50%;background:rgba(165,129,86,0.16);border:1px solid rgba(165,129,86,0.4);color:#A58156;display:flex;align-items:center;justify-content:center;">
            <svg width="26" height="26" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"></rect><path d="M7 11V7a5 5 0 0 1 10 0v4"></path></svg>
          </div>
          <h2 style="font-family:'Playfair Display',Georgia,serif;font-size:24px;font-weight:700;color:#2A1D17;margin:0 0 12px;">Yêu cầu đăng nhập để đặt phòng</h2>
          <p style="font-size:14px;line-height:1.65;color:#57534e;margin:0 0 24px;">Quý khách vui lòng đăng nhập tài khoản để đặt phòng tại HENRY Homestay. Nếu chưa có tài khoản, quý khách có thể đăng ký miễn phí chỉ trong 1 phút.</p>
          <div style="display:flex;flex-direction:column;gap:10px;">
            <div style="display:grid;grid-template-columns:1fr 1fr;gap:12px;">
              <a href="login.html" data-purpose="modal-login-btn" style="display:inline-flex;align-items:center;justify-content:center;padding:12px 18px;border-radius:9999px;background:#2A1D17;color:#fff;font-weight:600;font-size:14px;text-decoration:none;transition:all .2s;">Đăng nhập ngay</a>
              <a href="register.html" data-purpose="modal-register-btn" style="display:inline-flex;align-items:center;justify-content:center;padding:12px 18px;border-radius:9999px;border:1px solid #A58156;background:#faf7f2;color:#2A1D17;font-weight:600;font-size:14px;text-decoration:none;transition:all .2s;">Đăng ký tài khoản</a>
            </div>
            <button type="button" onclick="closeAuthRequiredModal()" style="margin-top:4px;padding:10px;border-radius:9999px;border:none;background:transparent;color:#78716c;font-weight:600;font-size:13px;cursor:pointer;">Đóng</button>
          </div>
        </div>
      `;
      modal.addEventListener('click', (e) => {
        if (e.target === modal) closeAuthRequiredModal();
      });
      document.body.appendChild(modal);
    } else {
      modal.classList.remove('hidden');
      modal.style.display = 'flex';
    }
    document.body.style.overflow = 'hidden';
  }

  function closeAuthRequiredModal() {
    const modal = document.getElementById('authRequiredModal');
    if (modal) {
      modal.classList.add('hidden');
      modal.style.display = 'none';
      document.body.style.overflow = '';
    }
  }

  window.showAuthRequiredModal = showAuthRequiredModal;
  window.closeAuthRequiredModal = closeAuthRequiredModal;
  window.showBookingAuthModal = showAuthRequiredModal;
  window.closeBookingAuthModal = closeAuthRequiredModal;

  async function postJson(url, payload) {
    const res = await fetch(url, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(payload)
    });
    return await res.json();
  }

  async function toggleSavedRoom(roomCode) {
    try {
      await postJson('/api/rooms/toggle-saved', { roomCode });
    } catch (e) {
      // Ignore if guest
    }
  }

  function syncHeaderUser() {
    document.querySelectorAll('header nav a[href="rooms.html"], #main-navigation a[href="rooms.html"], [data-purpose="nav-links"] a[href="rooms.html"]').forEach(a => a.remove());
    const authed = isUserAuthenticated();
    if (typeof HenryData !== 'undefined' && Array.isArray(HenryData.savedRoomCodes) && authed && HenryData.currentUser) {
      try {
        localStorage.setItem('henry-saved', JSON.stringify(HenryData.savedRoomCodes));
      } catch {}
    }

    const u = authed && typeof HenryData !== 'undefined' ? HenryData.currentUser : null;
    const initials = u && u.fullName
      ? u.fullName.trim().split(/\s+/).slice(-2).map(w => w[0].toUpperCase()).join('')
      : 'TV';

    if (u) {
      // Standard header (components.js)
      document.querySelectorAll('.account-button .avatar').forEach(el => el.textContent = initials);
      document.querySelectorAll('.account-button .account-name').forEach(el => {
        el.textContent = u.fullName;
      });

      // Legacy header (index.html, contact.html, room-detail.html)
      document.querySelectorAll('[data-purpose="user-menu"] [data-purpose="user-avatar"]').forEach(el => {
        el.textContent = initials;
      });
      document.querySelectorAll('[data-purpose="user-menu"] [data-purpose="user-name"]').forEach(el => {
        el.textContent = u.fullName;
      });

      const menus = document.querySelectorAll('#account-dropdown, .legacy-user-menu');
      menus.forEach(menu => {
        menu.innerHTML =
          `<a href="profile.html">Thông tin cá nhân (${u.fullName})</a>` +
          `<a href="profile.html#bookings">Đơn đặt phòng của tôi</a>` +
          `<a href="profile.html#saved">Phòng đã lưu</a>` +
          `<a href="#" data-action="logout">Đăng xuất</a>`;
      });
    } else {
      // Unauthenticated state: ensure 2 buttons (Đăng nhập & Đăng ký) are displayed on ALL pages
      document.querySelectorAll('.account-wrap').forEach(wrap => {
        if (!wrap.querySelector('[data-purpose="login-button"]')) {
          wrap.className = 'account-wrap auth-buttons-wrap';
          wrap.setAttribute('data-purpose', 'guest-auth-buttons');
          wrap.innerHTML =
            `<a class="header-auth-btn header-login-btn" data-purpose="login-button" href="login.html">Đăng nhập</a>` +
            `<a class="header-auth-btn header-register-btn" data-purpose="register-button" href="register.html">Đăng ký</a>`;
        }
      });

      document.querySelectorAll('[data-purpose="user-menu"]').forEach(wrap => {
        if (wrap.classList.contains('account-wrap')) return;
        if (!wrap.querySelector('[data-purpose="guest-auth-buttons"]')) {
          const guestBtns = document.createElement('div');
          guestBtns.setAttribute('data-purpose', 'guest-auth-buttons');
          guestBtns.className = 'flex items-center gap-2 sm:gap-2.5';
          guestBtns.innerHTML =
            `<a class="px-4 py-2 rounded-full border border-stone-300 bg-white hover:bg-stone-50 hover:border-brand-goldAccent text-brand-brownDark hover:text-brand-goldAccent text-xs sm:text-sm font-semibold transition-all shadow-sm" data-purpose="login-button" href="login.html">Đăng nhập</a>` +
            `<a class="px-4 py-2 rounded-full bg-brand-brownDark hover:bg-brand-goldAccent text-white text-xs sm:text-sm font-semibold transition-all shadow-sm" data-purpose="register-button" href="register.html">Đăng ký</a>`;
          const oldPill = wrap.querySelector('[data-purpose="user-account-pill"]') || wrap.querySelector('div');
          if (oldPill) oldPill.replaceWith(guestBtns);
          else wrap.prepend(guestBtns);
        }
      });
    }

    document.querySelectorAll('[data-action="logout"], nav.account-nav a[href="login.html"]').forEach(link => {
      if (link.dataset.boundLogout === '1') return;
      link.dataset.boundLogout = '1';
      link.addEventListener('click', async (e) => {
        e.preventDefault();
        try { localStorage.removeItem('henry-logged-in'); } catch {}
        document.cookie = 'HenryUserId=; path=/; expires=Thu, 01 Jan 1970 00:00:00 GMT';
        try { await postJson('/api/auth/logout', {}); } catch {}
        location.href = 'index.html';
      });
    });
  }

  function initAuthPages() {
    const loginForm = document.getElementById('login-form');
    if (loginForm && loginForm.dataset.boundLive !== '1') {
      loginForm.dataset.boundLive = '1';
      const pwdInput = document.getElementById('login-password');
      if (pwdInput) pwdInput.setAttribute('minlength', '6');

      const note = document.querySelector('.auth-note');
      if (note) {
        note.innerHTML = 'Tài khoản mẫu có sẵn trong CSDL: <b>quochuy@example.com</b> / Mật khẩu: <b>123456</b>';
      }

      loginForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        e.stopImmediatePropagation();
        const email = loginForm.elements.email.value.trim();
        const password = loginForm.elements.password.value;
        const msg = document.getElementById('auth-message');
        msg.hidden = false;
        msg.textContent = 'Đang xác thực tài khoản...';

        const res = await postJson('/api/auth/login', { email, password });
        if (!res.success) {
          msg.style.color = '#ba0036';
          msg.textContent = res.message || 'Đăng nhập thất bại.';
          return;
        }
        try { localStorage.setItem('henry-logged-in', '1'); } catch {}
        msg.style.color = '#15803d';
        msg.innerHTML = `${res.message} Đang chuyển đến <a class="text-link" href="profile.html">Hồ sơ của bạn</a>...`;
        setTimeout(() => { location.href = 'profile.html'; }, 600);
      }, true);
    }

    const registerForm = document.getElementById('register-form');
    if (registerForm && registerForm.dataset.boundLive !== '1') {
      registerForm.dataset.boundLive = '1';
      const pwdInput = document.getElementById('register-password');
      if (pwdInput) pwdInput.setAttribute('minlength', '6');
      const help = document.getElementById('password-help');
      if (help) help.textContent = 'Ít nhất 6 ký tự.';

      let phoneField = registerForm.querySelector('input[name="phone"]');
      if (!phoneField) {
        const emailLabel = registerForm.querySelector('input[name="email"]')?.closest('label');
        if (emailLabel) {
          const phoneLabel = document.createElement('label');
          phoneLabel.className = 'field';
          phoneLabel.innerHTML = 'Số điện thoại<input name="phone" type="tel" placeholder="0912 345 678" required>';
          emailLabel.after(phoneLabel);
        }
      }

      registerForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        e.stopImmediatePropagation();
        const confirm = registerForm.elements.confirm;
        confirm.setCustomValidity(confirm.value !== registerForm.elements.password.value ? 'Mật khẩu xác nhận chưa khớp.' : '');
        if (!registerForm.reportValidity()) return;

        const payload = {
          fullName: registerForm.elements.name.value.trim(),
          email: registerForm.elements.email.value.trim(),
          phone: registerForm.elements.phone ? registerForm.elements.phone.value.trim() : '',
          password: registerForm.elements.password.value
        };
        const msg = document.getElementById('auth-message');
        msg.hidden = false;
        msg.textContent = 'Đang tạo tài khoản trên CSDL...';

        const res = await postJson('/api/auth/register', payload);
        if (!res.success) {
          msg.style.color = '#ba0036';
          msg.textContent = res.message;
          return;
        }
        try { localStorage.setItem('henry-logged-in', '1'); } catch {}
        msg.style.color = '#15803d';
        msg.innerHTML = `${res.message} Đang chuyển đến <a class="text-link" href="profile.html">Tài khoản của bạn</a>...`;
        setTimeout(() => { location.href = 'profile.html'; }, 700);
      }, true);
    }
  }

  function initRoomDetailPage() {
    if (!location.pathname.endsWith('room-detail.html') && !location.pathname.toLowerCase().includes('/rooms/detail')) return;
    if (typeof HenryData === 'undefined' || !HenryData.rooms.length) return;

    const params = new URLSearchParams(location.search);
    const roomCode = params.get('room') || '42';
    const room = HenryData.rooms.find(r => r.id === roomCode) || HenryData.rooms[0];
    if (!room) return;

    document.title = `${room.name} | HENRY homestay`;
    const h1 = document.querySelector('h1');
    if (h1) h1.textContent = `${room.name} — ${room.view}`;

    const heroImg = document.querySelector('[data-purpose="hero-gallery"] img') || document.querySelector('.md\\:col-span-2 img');
    if (heroImg) {
      heroImg.src = `assets/images/${room.image}`;
      heroImg.alt = room.name;
    }

    const subH2 = document.querySelector('.lg\\:col-span-7 h2');
    if (subH2) subH2.textContent = `Không gian nghỉ dưỡng ${room.name}`;
    const subP = subH2 ? subH2.nextElementSibling : null;
    if (subP) {
      subP.innerHTML = `Tiếp đón bởi <span class="font-body-md-bold text-on-surface">HENRY Homestay</span> • Tối đa ${room.guests} khách • ${room.bed} • ${room.area} m²`;
    }

    const totalEl = document.getElementById('booking-total');
    const nightsEl = document.getElementById('booking-nights');
    if (totalEl) {
      totalEl.dataset.nightly = String(room.price);
      totalEl.textContent = money(room.price * 2);
      if (nightsEl) nightsEl.textContent = 'cho 2 đêm';
    }

    // Pre-fill dates if in URL
    const form = document.getElementById('search-form');
    if (form) {
      let ciInput = form.querySelector('input[name="checkin"]');
      let coInput = form.querySelector('input[name="checkout"]');
      if (!ciInput) {
        ciInput = document.createElement('input');
        ciInput.type = 'hidden';
        ciInput.name = 'checkin';
        form.appendChild(ciInput);
      }
      if (!coInput) {
        coInput = document.createElement('input');
        coInput.type = 'hidden';
        coInput.name = 'checkout';
        form.appendChild(coInput);
      }

      const today = new Date();
      const iso = d => d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0');
      const defIn = params.get('checkin') || iso(new Date(today.getFullYear(), today.getMonth(), today.getDate() + 1));
      const defOut = params.get('checkout') || iso(new Date(today.getFullYear(), today.getMonth(), today.getDate() + 3));

      if (!ciInput.value) ciInput.value = defIn;
      if (!coInput.value) coInput.value = defOut;

      const updatePriceDisplay = () => {
        const a = ciInput.value || defIn;
        const b = coInput.value || defOut;
        const n = Math.max(1, Math.round((new Date(b + 'T12:00:00') - new Date(a + 'T12:00:00')) / 86400000));
        if (totalEl) totalEl.textContent = money(room.price * n);
        if (nightsEl) nightsEl.textContent = `cho ${n} đêm`;
      };
      document.addEventListener('henry-search', updatePriceDisplay);
      setTimeout(updatePriceDisplay, 150);

      // Override openBookingModal: block if not logged in, otherwise go to booking-detail.html
      window.openBookingModal = function () {
        if (!isUserAuthenticated()) {
          showAuthRequiredModal();
          return;
        }
        const a = ciInput.value || defIn;
        const b = coInput.value || defOut;
        const getCnt = (type, defVal) => {
          const el = document.querySelector(`[data-count="${type}"]`);
          return el ? (parseInt(el.textContent, 10) || 0) : defVal;
        };
        const q = new URLSearchParams({
          room: room.id,
          checkin: a,
          checkout: b,
          adults: String(Math.max(1, getCnt('adults', 2))),
          children: String(getCnt('children', 0)),
          infants: String(getCnt('infants', 0)),
          pets: String(getCnt('pets', 0)),
          mode: 'checkout'
        });
        location.href = 'booking-detail.html?' + q.toString();
      };
      window.proceedToVietQR = window.openBookingModal;
    }
  }

  function initBookingDetailPage() {
    if (document.body.dataset.page !== 'booking') return;
    if (typeof HenryData === 'undefined') return;

    if (!isUserAuthenticated()) {
      showAuthRequiredModal();
      const openPayBtn = document.querySelector('[data-open-dialog="payment-dialog"]');
      if (openPayBtn) {
        openPayBtn.addEventListener('click', (e) => {
          e.preventDefault();
          e.stopImmediatePropagation();
          showAuthRequiredModal();
        }, true);
      }
    }

    const params = new URLSearchParams(location.search);
    const codeParam = params.get('code');
    const roomParam = params.get('room');
    const isCheckout = params.get('mode') === 'checkout' || !!roomParam;

    const existingBooking = codeParam
      ? (HenryData.userBookings || []).find(b => b.bookingCode.toLowerCase() === codeParam.toLowerCase()) || HenryData.activeBooking
      : (!isCheckout ? HenryData.activeBooking : null);

    // Pre-fill logged-in account details in #guest-account
    const u = HenryData.currentUser;
    const guestAccDl = document.getElementById('guest-account');
    if (guestAccDl && u) {
      const dds = guestAccDl.querySelectorAll('dd');
      if (dds.length >= 5) {
        dds[0].textContent = u.fullName || 'Khách hàng';
        dds[1].textContent = u.dateOfBirth ? fmtDateVi(u.dateOfBirth) : 'Chưa cung cấp';
        dds[2].textContent = u.email || '';
        dds[3].textContent = u.phone || 'Chưa cung cấp';
        dds[4].textContent = u.cccd || 'Chưa cung cấp';
      }
    }

    const today = new Date();
    const iso = d => d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0');

    let room = HenryData.rooms.find(r => r.id === (roomParam || (existingBooking && existingBooking.roomCode) || '42')) || HenryData.rooms[0];
    let checkin = params.get('checkin') || (existingBooking ? existingBooking.checkInDate : iso(new Date(today.getFullYear(), today.getMonth(), today.getDate() + 1)));
    let checkout = params.get('checkout') || (existingBooking ? existingBooking.checkOutDate : iso(new Date(today.getFullYear(), today.getMonth(), today.getDate() + 3)));
    let adults = Number(params.get('adults') || (existingBooking ? existingBooking.adults : 2));
    let children = Number(params.get('children') || (existingBooking ? existingBooking.children : 0));
    let infants = Number(params.get('infants') || (existingBooking ? existingBooking.infants : 0));
    let pets = Number(params.get('pets') || (existingBooking ? existingBooking.pets : 0));
    let nights = Math.max(1, Math.round((new Date(checkout + 'T12:00:00') - new Date(checkin + 'T12:00:00')) / 86400000));
    let nightlyPrice = existingBooking && !isCheckout ? existingBooking.pricePerNight : (room ? room.price : 1190000);
    let totalCost = existingBooking && !isCheckout ? existingBooking.totalAmount : (nights * nightlyPrice);

    // Update UI elements on booking-detail.html
    const statusBadge = document.querySelector('.booking-heading .status');
    const codeStrong = document.querySelector('[data-booking-id]');
    if (existingBooking && !isCheckout) {
      if (codeStrong) codeStrong.textContent = existingBooking.bookingCode;
      if (statusBadge) statusBadge.textContent = existingBooking.statusDisplay;
    } else {
      if (codeStrong) codeStrong.textContent = 'Đơn mới (Chờ xác nhận)';
      if (statusBadge) statusBadge.textContent = 'Đang đặt phòng';
    }

    const tripImg = document.querySelector('.trip-room img');
    const tripTitle = document.querySelector('.trip-room h2');
    const tripMeta = document.querySelector('.trip-room h2 + p');
    const tripLink = document.querySelector('.trip-room a.btn-link');
    if (room) {
      if (tripImg) { tripImg.src = `assets/images/${room.image}`; tripImg.alt = room.name; }
      if (tripTitle) tripTitle.textContent = room.name;
      if (tripMeta) tripMeta.textContent = `${room.view} · ${room.area} m² · ${room.bed}`;
      if (tripLink) tripLink.href = `room-detail.html?room=${room.id}`;
    }

    const factDds = document.querySelectorAll('.trip-facts dd');
    if (factDds.length >= 3) {
      factDds[0].innerHTML = `${fmtDateVi(checkin)}<small>Từ 14:00</small>`;
      factDds[1].innerHTML = `${fmtDateVi(checkout)}<small>Trước 12:00</small>`;
      factDds[2].innerHTML = `${nights} đêm · ${adults + children} khách${pets ? ' · ' + pets + ' thú cưng' : ''}<small>1 phòng</small>`;
    }

    // Update receipt lines
    const receiptSub = document.querySelector('.receipt > p.small.muted');
    if (receiptSub) receiptSub.textContent = `${nights} đêm · 1 phòng · ${adults + children} khách`;
    const receiptLineDt = document.querySelector('.receipt-lines dt');
    if (receiptLineDt) receiptLineDt.textContent = `${money(nightlyPrice)} × ${nights} đêm`;

    const pay = HenryData.payment;
    if (pay) {
      const qrImg = document.getElementById('pay-qr');
      if (qrImg && pay.qr) {
        let qrSrc = pay.qr;
        if (!qrSrc.startsWith('/') && !qrSrc.startsWith('http')) qrSrc = '/' + qrSrc;
        qrImg.src = qrSrc;
        qrImg.onerror = function () {
          if (!this.dataset.fallback) {
            this.dataset.fallback = '1';
            this.src = '/assets/images/payment-qr.svg';
          }
        };
      }
      const receiverEl = document.getElementById('pay-receiver');
      if (receiverEl) receiverEl.textContent = pay.receiver || '';
      const accountEl = document.getElementById('pay-account');
      if (accountEl) accountEl.textContent = pay.account || '';
      const bankEl = document.getElementById('pay-bank');
      if (bankEl) bankEl.textContent = pay.bank || '';
      const phone = document.getElementById('pay-phone');
      if (phone && pay.phone) {
        phone.textContent = pay.phone;
        phone.href = 'tel:' + pay.phone.replace(/\D/g, '');
      }
    }

    const rate = HenryData.payment ? (HenryData.payment.depositRate || 30) : 30;
    const renderCosts = () => {
      const mRadio = document.querySelector('[name=pay-method]:checked');
      const mVal = mRadio ? mRadio.value : 'full';
      const nowPay = mVal === 'deposit' ? Math.ceil(totalCost * rate / 100) : totalCost;
      const remPay = Math.max(0, totalCost - nowPay);

      document.querySelectorAll('[data-cost="room"]').forEach(el => el.textContent = money(totalCost));
      document.querySelectorAll('[data-cost="total"]').forEach(el => el.textContent = money(totalCost));

      const paidEl = document.querySelector('[data-cost="paid"]');
      const remEl = document.querySelector('[data-cost="remaining"]');
      const payAmtEl = document.getElementById('pay-amount');
      const openPayBtn = document.querySelector('[data-open-dialog="payment-dialog"]');

      if (existingBooking && !isCheckout) {
        if (paidEl) {
          paidEl.textContent = money(existingBooking.paidAmount);
          const lbl = paidEl.parentElement?.querySelector('span.muted');
          if (lbl) lbl.textContent = 'Đã thanh toán';
        }
        if (remEl) {
          remEl.textContent = money(existingBooking.remainingAmount);
          const lbl = remEl.parentElement?.querySelector('span.muted');
          if (lbl) lbl.textContent = 'Còn lại (cần thanh toán)';
        }
        if (payAmtEl) {
          payAmtEl.textContent = money(existingBooking.remainingAmount > 0 ? existingBooking.remainingAmount : existingBooking.totalAmount);
        }
        if (openPayBtn) {
          if (existingBooking.status === 'CANCELLED') {
            openPayBtn.textContent = 'Đơn đã bị hủy';
            openPayBtn.disabled = true;
          } else if (existingBooking.remainingAmount <= 0) {
            openPayBtn.textContent = `Đã thanh toán đủ (${money(existingBooking.totalAmount)})`;
            openPayBtn.disabled = true;
          } else {
            openPayBtn.textContent = `Thanh toán số tiền còn lại (${money(existingBooking.remainingAmount)})`;
            openPayBtn.disabled = false;
          }
        }
      } else {
        if (paidEl) {
          paidEl.textContent = money(nowPay);
          const lbl = paidEl.parentElement?.querySelector('span.muted');
          if (lbl) lbl.textContent = 'Thanh toán ngay';
        }
        if (remEl) {
          remEl.textContent = money(remPay);
          const lbl = remEl.parentElement?.querySelector('span.muted');
          if (lbl) lbl.textContent = 'Còn lại (thanh toán khi nhận phòng)';
        }
        if (payAmtEl) {
          payAmtEl.textContent = money(nowPay);
        }
        if (openPayBtn) {
          openPayBtn.textContent = 'Bắt đầu thanh toán';
          openPayBtn.disabled = false;
        }
      }
    };
    HenryLive.renderBookingCosts = renderCosts;
    document.querySelectorAll('[name=pay-method]').forEach(r => r.addEventListener('change', renderCosts));
    document.querySelector('[data-open-dialog="payment-dialog"]')?.addEventListener('click', () => setTimeout(renderCosts, 0));
    setTimeout(renderCosts, 50);

    // Handle Confirm Payment button -> Create real booking in SQL DB!
    const confirmBtn = document.getElementById('confirm-payment');
    if (confirmBtn && confirmBtn.dataset.boundLive !== '1') {
      confirmBtn.dataset.boundLive = '1';
      confirmBtn.addEventListener('click', async (e) => {
        e.preventDefault();
        e.stopImmediatePropagation();

        if (!isUserAuthenticated()) {
          document.getElementById('payment-dialog')?.close();
          showAuthRequiredModal();
          return;
        }

        const isOther = document.querySelector('[name=guest-source]:checked')?.value === 'other';
        let gName = u ? u.fullName : 'Khách trải nghiệm';
        let gEmail = u ? u.email : 'khach@henry.example';
        let gPhone = u ? u.phone : '0912345678';
        let gCccd = u ? (u.cccd || '') : '';
        let gDob = u ? u.dateOfBirth : null;

        if (isOther) {
          const otherWrap = document.getElementById('guest-other');
          gName = otherWrap.querySelector('[name=guestName]')?.value.trim() || '';
          gEmail = otherWrap.querySelector('[name=guestEmail]')?.value.trim() || '';
          gPhone = otherWrap.querySelector('[name=guestPhone]')?.value.trim() || '';
          gCccd = otherWrap.querySelector('[name=guestCitizenId]')?.value.trim() || '';
          const d = otherWrap.querySelector('[name=dobDay]')?.value;
          const m = otherWrap.querySelector('[name=dobMonth]')?.value;
          const y = otherWrap.querySelector('[name=dobYear]')?.value;
          if (y && m && d) gDob = `${y}-${String(m).padStart(2, '0')}-${String(d).padStart(2, '0')}`;

          if (!gName || !gEmail || !gPhone) {
            document.getElementById('payment-dialog')?.close();
            Henry.toast('Vui lòng nhập đầy đủ Họ tên, Email và Số điện thoại khách lưu trú.');
            return;
          }
        }

        const payOpt = document.querySelector('[name=pay-method]:checked')?.value === 'deposit' ? 'DEPOSIT' : 'FULL';
        confirmBtn.disabled = true;
        confirmBtn.textContent = 'Đang xử lý đặt phòng...';

        const res = await postJson('/api/bookings/create', {
          roomCode: room ? room.id : '42',
          checkInDate: checkin,
          checkOutDate: checkout,
          adults,
          children,
          infants,
          pets,
          guestName: gName,
          guestPhone: gPhone,
          guestEmail: gEmail,
          guestCccd: gCccd,
          guestDob: gDob,
          paymentOption: payOpt
        });

        confirmBtn.disabled = false;
        confirmBtn.textContent = 'Xác nhận thanh toán';
        document.getElementById('payment-dialog')?.close();

        if (!res.success) {
          if (res.requireAuth) {
            showAuthRequiredModal();
            return;
          }
          Henry.toast(res.message || 'Không thể đặt phòng.');
          return;
        }

        Henry.toast(res.message);
        if (res.booking && res.booking.bookingCode) {
          setTimeout(() => {
            location.href = `booking-detail.html?code=${res.booking.bookingCode}`;
          }, 900);
        }
      }, true);
    }

    // Add Cancel Booking button if viewing an active existing booking
    if (existingBooking && !isCheckout && existingBooking.status !== 'CANCELLED' && existingBooking.status !== 'COMPLETED') {
      const receiptAside = document.querySelector('aside.receipt');
      if (receiptAside && !document.getElementById('cancel-booking-btn')) {
        const cancelBtn = document.createElement('button');
        cancelBtn.type = 'button';
        cancelBtn.id = 'cancel-booking-btn';
        cancelBtn.className = 'btn full';
        cancelBtn.style.marginTop = '10px';
        cancelBtn.style.borderColor = '#ba0036';
        cancelBtn.style.color = '#ba0036';
        cancelBtn.textContent = 'Hủy đơn đặt phòng này';
        cancelBtn.addEventListener('click', async () => {
          if (!confirm(`Bạn có chắc chắn muốn hủy đơn đặt phòng ${existingBooking.bookingCode}?`)) return;
          const res = await postJson('/api/bookings/cancel', { bookingId: existingBooking.id });
          Henry.toast(res.message);
          if (res.success) {
            setTimeout(() => location.reload(), 900);
          }
        });
        receiptAside.appendChild(cancelBtn);
      }
    }
  }

  function initProfilePage() {
    if (document.body.dataset.page !== 'profile') return;
    if (typeof HenryData === 'undefined') return;

    const u = isUserAuthenticated() ? HenryData.currentUser : null;
    const f = document.getElementById('profile-form');
    if (!u) {
      const sumName = document.getElementById('summary-name');
      if (sumName) sumName.textContent = 'Chưa đăng nhập';
      const emailP = document.querySelector('.profile-summary p');
      if (emailP) emailP.textContent = 'Vui lòng đăng nhập tài khoản';
      const statusSpan = document.querySelector('.profile-summary .status');
      if (statusSpan) statusSpan.textContent = 'Khách';
      document.querySelectorAll('.profile-avatar').forEach(el => el.textContent = '?');
      if (f) {
        if (f.elements.name) f.elements.name.value = '';
        if (f.elements.email) f.elements.email.value = '';
      }
      const bookingsPanel = document.getElementById('panel-bookings');
      if (bookingsPanel) {
        bookingsPanel.innerHTML = `<div class="panel"><div class="empty"><h3>Vui lòng đăng nhập để xem đơn đặt phòng</h3><p>Đăng nhập tài khoản HENRY để theo dõi lịch sử đặt phòng của bạn.</p><a class="btn btn-primary" href="login.html">Đăng nhập ngay</a></div></div>`;
      }
      showAuthRequiredModal();
      return;
    }
    if (u && f && f.dataset.boundLive !== '1') {
      f.dataset.boundLive = '1';
      document.getElementById('summary-name').textContent = u.fullName;
      const emailP = document.querySelector('.profile-summary p');
      if (emailP) emailP.textContent = u.email;
      const statusSpan = document.querySelector('.profile-summary .status');
      if (statusSpan) statusSpan.textContent = u.role === 'ADMIN' ? 'Quản trị viên' : u.role === 'STAFF' ? 'Nhân viên' : 'Khách hàng thành viên';

      const initials = u.fullName.trim().split(/\s+/).slice(-2).map(w => w[0].toUpperCase()).join('');
      document.querySelectorAll('.profile-avatar').forEach(el => el.textContent = initials);

      f.elements.name.value = u.fullName || '';
      f.elements.email.value = u.email || '';
      if (f.elements.phone) f.elements.phone.value = u.phone || '';
      if (f.elements.citizenId) f.elements.citizenId.value = u.cccd || '';

      if (u.dateOfBirth) {
        const parts = u.dateOfBirth.slice(0, 10).split('-').map(Number);
        if (parts.length === 3) {
          setTimeout(() => {
            if (f.elements.dobYear) f.elements.dobYear.value = String(parts[0]);
            if (f.elements.dobMonth) f.elements.dobMonth.value = String(parts[1]);
            if (f.elements.dobDay) f.elements.dobDay.value = String(parts[2]);
          }, 50);
        }
      }

      f.addEventListener('submit', async (e) => {
        e.preventDefault();
        e.stopImmediatePropagation();
        let dob = null;
        const y = f.elements.dobYear?.value, m = f.elements.dobMonth?.value, d = f.elements.dobDay?.value;
        if (y && m && d) dob = `${y}-${String(m).padStart(2, '0')}-${String(d).padStart(2, '0')}`;

        const res = await postJson('/api/profile/update', {
          fullName: f.elements.name.value,
          email: f.elements.email.value,
          phone: f.elements.phone.value,
          cccd: f.elements.citizenId.value,
          dateOfBirth: dob
        });
        Henry.toast(res.message);
        if (res.success) {
          document.getElementById('summary-name').textContent = f.elements.name.value;
          const ep = document.querySelector('.profile-summary p');
          if (ep) ep.textContent = f.elements.email.value;
        }
      }, true);
    }

    const secForm = document.getElementById('security-form');
    if (secForm && secForm.dataset.boundLive !== '1') {
      secForm.dataset.boundLive = '1';
      secForm.elements.password.setAttribute('minlength', '6');
      secForm.addEventListener('submit', async (e) => {
        e.preventDefault();
        e.stopImmediatePropagation();
        if (secForm.elements.confirm.value !== secForm.elements.password.value) {
          secForm.elements.confirm.setCustomValidity('Mật khẩu xác nhận chưa khớp.');
          secForm.reportValidity();
          return;
        }
        const res = await postJson('/api/profile/password', {
          currentPassword: secForm.elements.current.value,
          newPassword: secForm.elements.password.value
        });
        Henry.toast(res.message);
        if (res.success) secForm.reset();
      }, true);
    }

    // Render user bookings from database
    const bookingsPanel = document.getElementById('panel-bookings');
    if (bookingsPanel && Array.isArray(HenryData.userBookings)) {
      const list = HenryData.userBookings;
      const itemsHtml = list.length
        ? list.map(b => `
          <article class="booking-mini" style="margin-bottom:16px;padding-bottom:16px;border-bottom:1px solid #eee">
            <img src="assets/images/${b.roomImage}" alt="Phòng ${b.roomName}">
            <div>
              <span class="status">${b.statusDisplay}</span>
              <h3>${b.roomName}</h3>
              <p class="small muted">${fmtDateVi(b.checkInDate)} – ${fmtDateVi(b.checkOutDate)} · ${b.nights} đêm · ${b.adults + b.children} khách</p>
              <p class="small" style="margin-top:6px">Mã đơn: <b>${b.bookingCode}</b> · Tổng tiền: <b>${money(b.totalAmount)}</b> (Đã thanh toán: ${money(b.paidAmount)})</p>
              <a class="btn-link" href="booking-detail.html?code=${b.bookingCode}">Xem chi tiết đơn<span data-icon="arrow">${Henry.icon('arrow')}</span></a>
            </div>
          </article>`).join('')
        : `<div class="empty"><h3>Bạn chưa có đơn đặt phòng nào</h3><p>Hãy chọn căn phòng yêu thích cho kỳ nghỉ tiếp theo của bạn.</p><a class="btn btn-primary" href="rooms.html">Tìm và đặt phòng</a></div>`;

      bookingsPanel.innerHTML = `
        <div class="panel">
          <div class="panel-heading">
            <h2>Đơn đặt phòng của tôi</h2>
            <span class="small muted">${list.length} đơn trong CSDL</span>
          </div>
          ${itemsHtml}
        </div>`;
    }
  }

  function initContactPage() {
    const form = document.querySelector('[data-purpose="inquiry-form-card"] form');
    if (!form || form.dataset.boundLive === '1') return;
    form.dataset.boundLive = '1';
    form.removeAttribute('onsubmit');
    form.addEventListener('submit', async (e) => {
      e.preventDefault();
      const payload = {
        fullName: form.querySelector('[name="name"]')?.value || '',
        phone: form.querySelector('[name="phone"]')?.value || '',
        email: form.querySelector('[name="email"]')?.value || '',
        serviceType: form.querySelector('[name="service"]')?.value || 'room',
        stayDate: form.querySelector('[name="stay_date"]')?.value || null,
        message: form.querySelector('[name="message"]')?.value || ''
      };
      const res = await postJson('/api/contact/submit', payload);
      alert(res.message || 'Cảm ơn bạn! HENRY đã tiếp nhận thông tin.');
      if (res.success) form.reset();
    });
  }

  document.addEventListener('DOMContentLoaded', () => {
    syncHeaderUser();
    initAuthPages();
    initRoomDetailPage();
    initBookingDetailPage();
    initProfilePage();
    initContactPage();
  });

  return { toggleSavedRoom, isUserAuthenticated, showAuthRequiredModal, closeAuthRequiredModal, renderBookingCosts: () => (typeof HenryLive.renderBookingCosts === 'function' ? HenryLive.renderBookingCosts() : null) };
})();
