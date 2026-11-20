document.addEventListener('DOMContentLoaded', () => {
  if (typeof HenryData === 'undefined' || !HenryData.payment) return;
  const m = HenryData.money || ((v) => new Intl.NumberFormat('vi-VN').format(Math.round(v || 0)) + ' ₫');
  const pay = HenryData.payment;
  const b = HenryData.booking || { nightly: 1190000, nights: 2, id: 'HN-281026' };
  const total = (b.nightly || 1190000) * (b.nights || 2);
  const costs = { room: total, total };

  // 1. Populate payment info & QR
  const qrImg = document.getElementById('pay-qr');
  if (qrImg && pay.qr) {
    let qrSrc = pay.qr;
    if (!qrSrc.startsWith('/') && !qrSrc.startsWith('http')) qrSrc = '/' + qrSrc;
    qrImg.src = qrSrc;
    qrImg.onerror = function() {
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

  // 2. Deposit percentage label update
  const rate = pay.depositRate || 30;
  const depositBody = document.querySelector('[name=pay-method][value=deposit]')?.nextElementSibling;
  if (depositBody) {
    const st = depositBody.querySelector('strong');
    if (st) st.textContent = 'Đặt cọc trước ' + rate + '%';
    const sm = depositBody.querySelector('small');
    if (sm) sm.textContent = 'Thanh toán trước ' + rate + '% còn lại thanh toán vào ngày nhận phòng';
  }

  // 3. Calculation & rendering costs
  const method = () => document.querySelector('[name=pay-method]:checked')?.value || 'full';
  function render() {
    if (window.HenryLive && typeof window.HenryLive.renderBookingCosts === 'function') {
      window.HenryLive.renderBookingCosts();
      return;
    }
    const now = method() === 'deposit' ? Math.ceil(total * rate / 100) : total;
    costs.paid = now;
    costs.remaining = total - now;
    document.querySelectorAll('[data-cost]').forEach(el => {
      if (costs[el.dataset.cost] !== undefined) el.textContent = m(costs[el.dataset.cost]);
    });
  }

  document.querySelectorAll('[name=pay-method]').forEach(r => r.addEventListener('change', render));
  render();

  // 4. Dialog triggers
  const openDialogBtn = document.querySelector('[data-open-dialog=payment-dialog]');
  if (openDialogBtn) {
    openDialogBtn.addEventListener('click', () => {
      const paidEl = document.querySelector('[data-cost="paid"]');
      const payAmountEl = document.getElementById('pay-amount');
      if (payAmountEl && paidEl) {
        payAmountEl.textContent = paidEl.textContent;
      }
    });
  }

  // 5. Booking ID & other controls
  if (b && b.id) {
    document.querySelectorAll('[data-booking-id]').forEach(el => {
      if (!el.textContent || el.textContent === 'HN-281026') {
        el.textContent = b.id;
      }
    });
  }

  const dob = ['dobDay', 'dobMonth', 'dobYear'].map(n => document.querySelector('#guest-other [name=' + n + ']'));
  if (dob[0] && dob[1] && dob[2]) {
    const fill = (el, from, to, step) => {
      for (let n = from; step > 0 ? n <= to : n >= to; n += step) el.add(new Option(n, n));
    };
    fill(dob[0], 1, 31, 1);
    fill(dob[1], 1, 12, 1);
    fill(dob[2], new Date().getFullYear(), 1920, -1);
    dob.forEach(el => el.addEventListener('change', () => {
      const [d, mo, y] = dob.map(e => +e.value);
      let msg = '';
      if (d && mo && y) {
        const t = new Date(y, mo - 1, d);
        if (t.getMonth() !== mo - 1 || t.getDate() !== d) msg = 'Ngày sinh không tồn tại.';
      }
      dob[0].setCustomValidity(msg);
    }));
  }

  const account = document.getElementById('guest-account');
  const other = document.getElementById('guest-other');
  if (account && other) {
    document.querySelectorAll('[name=guest-source]').forEach(r => r.addEventListener('change', () => {
      const isOther = document.querySelector('[name=guest-source]:checked')?.value === 'other';
      account.hidden = isOther;
      other.hidden = !isOther;
    }));
  }

  document.getElementById('back-link')?.addEventListener('click', () => {
    if (history.length > 1 && document.referrer) history.back();
    else location.href = 'index.html';
  });

  document.getElementById('copy-booking')?.addEventListener('click', async () => {
    const code = document.querySelector('[data-booking-id]')?.textContent || (b ? b.id : '');
    try {
      await navigator.clipboard.writeText(code);
      if (typeof Henry !== 'undefined' && Henry.toast) Henry.toast('Đã sao chép mã ' + code);
    } catch {
      if (typeof Henry !== 'undefined' && Henry.toast) Henry.toast('Mã đặt phòng: ' + code);
    }
  });

  const confirmBtn = document.getElementById('confirm-payment');
  if (confirmBtn && !confirmBtn.dataset.boundBookingJs) {
    confirmBtn.dataset.boundBookingJs = '1';
    confirmBtn.addEventListener('click', () => {
      if (typeof HenryLive === 'undefined') {
        document.getElementById('payment-dialog')?.close();
        if (typeof Henry !== 'undefined' && Henry.toast) {
          Henry.toast('Đã ghi nhận xác nhận thanh toán. Nhân viên sẽ liên hệ với bạn.');
        }
      }
    });
  }
});
