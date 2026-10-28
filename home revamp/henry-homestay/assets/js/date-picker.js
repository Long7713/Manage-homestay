(function () {
  var popup = document.getElementById('date-popup');
  if (!popup) return;
  var zones = { checkin: document.getElementById('zone-checkin'), checkout: document.getElementById('zone-checkout') };
  var labels = { checkin: document.getElementById('date-checkin'), checkout: document.getElementById('date-checkout') };
  var DOW = ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'];

  function startOfDay(d) { return new Date(d.getFullYear(), d.getMonth(), d.getDate()); }
  function addDays(d, n) { return new Date(d.getFullYear(), d.getMonth(), d.getDate() + n); }
  function same(a, b) { return a && b && a.getTime() === b.getTime(); }
  function fmt(d) { return d.getDate() + ' thg ' + (d.getMonth() + 1) + ', ' + d.getFullYear(); }

  var today = startOfDay(new Date());
  var form = document.getElementById('search-form');
  var q = new URLSearchParams(location.search);
  function parseIso(v) { var m = /^(\d{4})-(\d{2})-(\d{2})$/.exec(v || ''); if (!m) return null; var d = new Date(+m[1], m[2] - 1, +m[3]); return d >= today ? d : null; }
  function iso(d) { return d ? d.getFullYear() + '-' + String(d.getMonth() + 1).padStart(2, '0') + '-' + String(d.getDate()).padStart(2, '0') : ''; }
  var checkin = parseIso(q.get('checkin')) || today;
  var checkout = parseIso(q.get('checkout'));
  if (!checkout || checkout <= checkin) checkout = addDays(checkin, 2);
  function sync() {
    if (!form) return;
    form.elements.checkin.value = iso(checkin);
    form.elements.checkout.value = iso(checkout);
    document.dispatchEvent(new CustomEvent('henry-search'));
  }
  var view = new Date(today.getFullYear(), today.getMonth(), 1);
  var active = null, hover = null;

  function monthHtml(first, cls, prev, next) {
    var y = first.getFullYear(), m = first.getMonth();
    var h = '<div class="dp-month ' + cls + '"><div class="dp-head">' +
      (prev ? '<button type="button" class="dp-nav dp-prev" aria-label="Tháng trước"' + (first <= new Date(today.getFullYear(), today.getMonth(), 1) ? ' disabled' : '') + '>‹</button>' : '') +
      'Tháng ' + (m + 1) + ' năm ' + y +
      (next ? '<button type="button" class="dp-nav dp-next" aria-label="Tháng sau">›</button>' : '') +
      '</div><div class="dp-grid">';
    DOW.forEach(function (d) { h += '<div class="dp-dow">' + d + '</div>'; });
    var offset = (new Date(y, m, 1).getDay() + 6) % 7;
    for (var i = 0; i < offset; i++) h += '<div></div>';
    var days = new Date(y, m + 1, 0).getDate();
    for (var d = 1; d <= days; d++) {
      var date = new Date(y, m, d), c = 'dp-day';
      var end = checkout || (active === 'checkout' && checkin && hover && hover > checkin ? hover : null);
      if (same(date, checkin)) c += ' dp-sel dp-start' + (end ? ' dp-has-range' : '');
      else if (same(date, checkout)) c += ' dp-sel dp-end';
      else if (checkin && end && date > checkin && date < end) c += ' dp-in';
      if (!checkout && checkin && hover && date > checkin && date <= hover && !same(date, hover)) c += ' dp-hover';
      var past = date < today;
      h += '<button type="button" class="' + c + '" data-date="' + date.getTime() + '"' + (past ? ' disabled' : '') + '><span>' + d + '</span></button>';
    }
    return h + '</div></div>';
  }

  function render() {
    var next = new Date(view.getFullYear(), view.getMonth() + 1, 1);
    popup.innerHTML = '<div class="dp-months">' + monthHtml(view, 'dp-m1', true, false) + monthHtml(next, 'dp-m2', false, true) + '</div>' +
      '<div class="dp-foot"><span class="dp-hint">' + (active === 'checkout' ? 'Chọn ngày trả phòng' : 'Chọn ngày nhận phòng') + '</span><button type="button" class="dp-clear">Xóa ngày</button></div>';
    // Trên điện thoại chỉ hiện một tháng nên nút "tháng sau" nằm ở tháng đầu.
    if (window.matchMedia('(max-width:767px)').matches) {
      var head = popup.querySelector('.dp-m1 .dp-head');
      head.insertAdjacentHTML('beforeend', '<button type="button" class="dp-nav dp-next" aria-label="Tháng sau">›</button>');
    }
    labels.checkin.textContent = checkin ? fmt(checkin) : 'Thêm ngày';
    labels.checkout.textContent = checkout ? fmt(checkout) : 'Thêm ngày';
    sync();
  }

  function open(which) {
    active = which;
    HenryPopup.show(popup);
    Object.keys(zones).forEach(function (k) { zones[k].classList.toggle('dp-active', k === which); });
    var ref = (which === 'checkout' && checkout) || checkin;
    if (ref && !popup.dataset.shown) view = new Date(ref.getFullYear(), ref.getMonth(), 1);
    popup.dataset.shown = '1';
    render();
  }
  function close() {
    HenryPopup.hide(popup); active = null; delete popup.dataset.shown;
    Object.keys(zones).forEach(function (k) { zones[k].classList.remove('dp-active'); });
  }

  function pick(date) {
    if (active === 'checkout' && checkin && date > checkin) {
      checkout = date; render(); close(); return;
    }
    checkin = date;
    if (checkout && checkout <= date) checkout = null;
    active = 'checkout';
    Object.keys(zones).forEach(function (k) { zones[k].classList.toggle('dp-active', k === 'checkout'); });
    render();
  }

  Object.keys(zones).forEach(function (k) {
    zones[k].addEventListener('click', function () { open(k); });
    zones[k].addEventListener('keydown', function (e) {
      if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); open(k); }
      if (e.key === 'Escape') close();
    });
  });
  popup.addEventListener('click', function (e) {
    e.stopPropagation();
    var day = e.target.closest('.dp-day');
    if (day) return pick(new Date(Number(day.dataset.date)));
    if (e.target.closest('.dp-prev')) { view = new Date(view.getFullYear(), view.getMonth() - 1, 1); render(); }
    else if (e.target.closest('.dp-next')) { view = new Date(view.getFullYear(), view.getMonth() + 1, 1); render(); }
    else if (e.target.closest('.dp-clear')) { checkin = checkout = null; active = 'checkin'; render(); }
  });
  popup.addEventListener('mouseover', function (e) {
    var day = e.target.closest('.dp-day');
    if (!day || checkout || !checkin) return;
    var h = new Date(Number(day.dataset.date));
    if (same(h, hover)) return;
    hover = h; render();
  });
  document.addEventListener('click', function (e) {
    if (HenryPopup.isOpen(popup) && !popup.contains(e.target) && !e.target.closest('.date-zone')) close();
  });
  document.addEventListener('keydown', function (e) { if (e.key === 'Escape') close(); });
  render();
})();
