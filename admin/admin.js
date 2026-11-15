/* Trang quản trị HENRY (giao diện mẫu). Dữ liệu lưu trong localStorage của trình duyệt, chưa có máy chủ. */
(function () {
  var $ = function (s, r) { return (r || document).querySelector(s); };
  var esc = function (s) { return String(s == null ? '' : s).replace(/[&<>"]/g, function (c) { return { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;' }[c]; }); };
  var money = function (n) { return new Intl.NumberFormat('vi-VN').format(Math.round(n)) + ' ₫'; };
  var pad = function (n) { return String(n).padStart(2, '0'); };
  var iso = function (d) { return d.getFullYear() + '-' + pad(d.getMonth() + 1) + '-' + pad(d.getDate()); };
  var parse = function (s) { var p = s.split('-').map(Number); return new Date(p[0], p[1] - 1, p[2]); };
  var fmt = function (s) { return s ? s.split('-').reverse().join('/') : '—'; };
  var nightsBetween = function (a, b) { return Math.round((parse(b) - parse(a)) / 864e5); };
  var TODAY = iso(new Date());

  var ROOM_TYPES = ['Phòng đôi', 'Studio', 'Phòng gia đình'];
  var AMENITIES = { wifi: 'Wi-Fi', aircon: 'Điều hòa', balcony: 'Ban công riêng', kitchen: 'Bếp nhỏ' };
  var STATUS_CLS = { 'Chờ xác nhận': 'amber', 'Đã xác nhận': 'blue', 'Đã nhận phòng': 'green', 'Hoàn thành': 'gray', 'Đã hủy': 'red' };
  var STATUSES = Object.keys(STATUS_CLS);
  var CAL_CLS = { 'Chờ xác nhận': 's-wait', 'Đã xác nhận': 's-ok', 'Đã nhận phòng': 's-in', 'Hoàn thành': 's-done' };

  /* ---------- Dữ liệu ---------- */
  var KEY = 'henry-admin-v1', PAY_KEY = 'henry-payment';
  function seed() {
    var t = new Date();
    var d = function (n) { return iso(new Date(t.getFullYear(), t.getMonth(), t.getDate() + n)); };
    var pets = ['42', '52', '71'];
    var rooms = HenryData.rooms.map(function (r) {
      return { id: r.id, name: r.name, type: { double: 'Phòng đôi', studio: 'Studio', family: 'Phòng gia đình' }[r.type], desc: r.view + '. ' + r.bed + ', ' + r.area + ' m².', amenities: r.amenities.slice(), maxGuests: r.guests, pets: pets.indexOf(r.id) >= 0, price: r.price, image: r.image, visible: true };
    });
    var customers = [
      { id: 'c1', name: 'Nguyễn Quốc Huy', phone: '0912 345 678', email: 'quochuy@example.com', cccd: '079201001234', registered: d(-120), locked: false },
      { id: 'c2', name: 'Trần Thị Mai', phone: '0987 654 321', email: 'maitran@example.com', cccd: '079195004567', registered: d(-80), locked: false },
      { id: 'c3', name: 'Lê Hoàng Nam', phone: '0903 111 222', email: 'namle@example.com', cccd: '079198007890', registered: d(-45), locked: false },
      { id: 'c4', name: 'Phạm Minh Anh', phone: '0938 222 333', email: 'minhanh@example.com', cccd: '079200003210', registered: d(-20), locked: false },
      { id: 'c5', name: 'Đỗ Thùy Linh', phone: '0977 888 999', email: 'thuylinh@example.com', cccd: '079197006543', registered: d(-5), locked: true }
    ];
    var o = function (id, c, room, a, b, ad, ch, inf, pt, method, status, created, extra) {
      return Object.assign({ id: id, customerId: c, roomId: room, checkin: d(a), checkout: d(b), adults: ad, children: ch, infants: inf, pets: pt, method: method, status: status, createdAt: d(created), paid: 0, refund: 'none', cancelledAt: '' }, extra || {});
    };
    var orders = [
      o('HN-270001', 'c1', '42', 1, 3, 2, 0, 0, 0, 'deposit', 'Chờ xác nhận', 0),
      o('HN-270002', 'c2', '41', -2, 1, 2, 1, 0, 0, 'full', 'Đã nhận phòng', -6),
      o('HN-270003', 'c3', '52', 5, 7, 2, 0, 1, 1, 'deposit', 'Đã xác nhận', -3),
      o('HN-270004', 'c4', '71', -8, -6, 2, 0, 0, 0, 'full', 'Hoàn thành', -14),
      o('HN-270005', 'c1', '81', 9, 12, 4, 2, 0, 0, 'deposit', 'Đã hủy', -4, { cancelledAt: d(0) }),
      o('HN-270006', 'c5', '51', 2, 4, 2, 0, 0, 0, 'full', 'Chờ xác nhận', 0),
      o('HN-270007', 'c2', '42', 12, 14, 2, 0, 0, 0, 'full', 'Đã xác nhận', -1)
    ];
    var tx = [], n = 0;
    orders.forEach(function (x) {
      var room = rooms.filter(function (r) { return r.id === x.roomId; })[0];
      var total = nightsBetween(x.checkin, x.checkout) * room.price;
      var amount = x.method === 'deposit' ? Math.ceil(total * 0.3) : total;
      if (x.status === 'Chờ xác nhận') { tx.push({ id: 'GD-' + (++n), orderId: x.id, amount: amount, method: x.method, time: x.createdAt + ' 09:30', status: 'Chờ xác nhận' }); return; }
      x.paid = amount;
      tx.push({ id: 'GD-' + (++n), orderId: x.id, amount: amount, method: x.method, time: x.createdAt + ' 10:15', status: 'Đã xác nhận' });
      if (x.status === 'Đã hủy') x.refund = 'pending';
    });
    var staff = [
      { id: 's1', name: 'Nguyễn Văn An', phone: '0901 234 567', email: 'an.nguyen@henry.example', role: 'Quản lý', joined: d(-400), locked: false },
      { id: 's2', name: 'Lê Thị Bích', phone: '0902 345 678', email: 'bich.le@henry.example', role: 'Lễ tân', joined: d(-210), locked: false },
      { id: 's3', name: 'Trần Quốc Cường', phone: '0903 456 789', email: 'cuong.tran@henry.example', role: 'Kế toán', joined: d(-150), locked: false },
      { id: 's4', name: 'Phạm Thu Hà', phone: '0904 567 890', email: 'ha.pham@henry.example', role: 'Buồng phòng', joined: d(-60), locked: true }
    ];
    return { rooms: rooms, customers: customers, orders: orders, tx: tx, staff: staff, seq: 100 };
  }
  var S;
  try { S = JSON.parse(localStorage.getItem(KEY)); } catch (e) { S = null; }
  if (!S || !S.rooms) S = seed();
  if (!S.staff) S.staff = seed().staff;
  function save() { try { localStorage.setItem(KEY, JSON.stringify(S)); } catch (e) { toast('Trình duyệt không cho lưu dữ liệu.'); } }

  function payCfg() {
    var base = { qr: 'assets/images/payment-qr.svg', receiver: 'Võ Văn Việt', account: '0708670042', bank: 'MB Bank', phone: '0795443500', depositRate: 30 };
    try { return Object.assign(base, JSON.parse(localStorage.getItem(PAY_KEY)) || {}); } catch (e) { return base; }
  }
  var room = function (id) { return S.rooms.filter(function (r) { return r.id === id; })[0]; };
  var cust = function (id) { return S.customers.filter(function (c) { return c.id === id; })[0] || { name: '—', phone: '', email: '', cccd: '' }; };
  var order = function (id) { return S.orders.filter(function (o) { return o.id === id; })[0]; };
  var total = function (o) { var r = room(o.roomId); return nightsBetween(o.checkin, o.checkout) * (r ? r.price : 0); };
  var methodLabel = function (o) { return o.method === 'deposit' ? 'Đặt cọc trước ' + payCfg().depositRate + '%' : 'Thanh toán toàn bộ'; };
  var lbl = function (text, cls) { return '<span class="lbl ' + cls + '">' + esc(text) + '</span>'; };
  var pendingTx = function () { return S.tx.filter(function (t) { return t.status === 'Chờ xác nhận'; }); };

  /* ---------- Tiện ích giao diện ---------- */
  var toastTimer;
  function toast(msg) { var t = $('#toast'); t.textContent = msg; t.hidden = false; clearTimeout(toastTimer); toastTimer = setTimeout(function () { t.hidden = true; }, 3500); }
  var modal = $('#modal');
  function openModal(title, body, buttons) {
    $('#m-title').textContent = title; $('#m-body').innerHTML = body;
    var foot = $('#m-foot'); foot.innerHTML = '';
    (buttons || []).concat([{ label: 'Đóng', cls: '', fn: null }]).forEach(function (b) {
      var el = document.createElement('button'); el.type = 'button'; el.className = 'btn ' + (b.cls || ''); el.textContent = b.label;
      el.addEventListener('click', function () { if (!b.fn) { modal.close(); return; } if (b.fn() !== false) modal.close(); });
      foot.appendChild(el);
    });
    if (!modal.open) modal.showModal();
  }
  $('#m-close').addEventListener('click', function () { modal.close(); });
  modal.addEventListener('click', function (e) { if (e.target === modal) modal.close(); });

  /* ---------- Điều hướng ---------- */
  var ICON = {
    rooms: '<path d="M3 21V8l9-5 9 5v13M9 21v-7h6v7"/>', orders: '<path d="M8 7V3m8 4V3M5 11h14M5 5h14a2 2 0 012 2v12a2 2 0 01-2 2H5a2 2 0 01-2-2V7a2 2 0 012-2z"/>',
    payments: '<rect x="2" y="5" width="20" height="14" rx="2"/><path d="M2 10h20"/>', customers: '<circle cx="9" cy="8" r="4"/><path d="M2 21v-2a5 5 0 015-5h4a5 5 0 015 5v2M17 11a3 3 0 100-6"/>',
    settings: '<circle cx="12" cy="12" r="3"/><path d="M12 2v3M12 19v3M2 12h3M19 12h3M5 5l2 2M17 17l2 2M19 5l-2 2M7 17l-2 2"/>'
  };
  var ROUTES = { rooms: 'Quản lý phòng', orders: 'Quản lý đơn đặt phòng', payments: 'Quản lý thanh toán', customers: 'Quản lý khách hàng', staff: 'Quản lý nhân viên', settings: 'Cấu hình hệ thống' };
  var tab = { rooms: 'list', payments: 'list' }, calMonth = new Date(new Date().getFullYear(), new Date().getMonth(), 1);
  var oFilter = { q: '', status: '', date: '', room: '' };

  function renderMenu(cur) {
    $('#side-menu').innerHTML = Object.keys(ROUTES).map(function (k) {
      var badge = k === 'payments' && pendingTx().length ? '<span class="badge-n">' + pendingTx().length + '</span>' : '';
      return '<li><a href="#' + k + '" class="' + (k === cur ? 'active' : '') + '">' + ROUTES[k] + badge + '</a></li>';
    }).join('');
  }
  function route() {
    var k = location.hash.slice(1); if (!ROUTES[k]) k = 'rooms';
    $('#page-title').textContent = ROUTES[k];
    renderMenu(k); $('#sidebar').classList.remove('open');
    ({ rooms: viewRooms, orders: viewOrders, payments: viewPayments, customers: viewCustomers, staff: viewStaff, settings: viewSettings })[k]();
  }
  function refresh() { save(); route(); }
  window.addEventListener('hashchange', route);
  $('#menu-btn').addEventListener('click', function () { $('#sidebar').classList.toggle('open'); });

  /* ---------- Quản lý phòng ---------- */
  function occupancy(r) {
    var os = S.orders.filter(function (o) { return o.roomId === r.id && o.status !== 'Đã hủy' && o.status !== 'Hoàn thành'; });
    if (os.some(function (o) { return o.status === 'Đã nhận phòng'; })) return ['teal', 'Đang hoạt động (có người ở)'];
    if (os.length) return ['amber', 'Đã có lịch đặt'];
    return ['gray', 'Chưa ai đặt'];
  }
  function viewRooms() {
    var v = $('#view');
    var head = '<div class="tabs"><button data-tab="list" class="' + (tab.rooms === 'list' ? 'on' : '') + '">Danh sách phòng</button><button data-tab="cal" class="' + (tab.rooms === 'cal' ? 'on' : '') + '">Lịch đặt phòng</button></div>';
    var body = tab.rooms === 'list' ? roomsList() : roomsCalendar();
    v.innerHTML = '<div class="box"><div class="box-body">' + head + body + '</div></div>';
    v.querySelectorAll('[data-tab]').forEach(function (b) { b.onclick = function () { tab.rooms = b.dataset.tab; viewRooms(); }; });
    v.querySelectorAll('[data-act]').forEach(function (b) { b.onclick = function () { roomAct(b.dataset.act, b.dataset.id); }; });
    var add = $('#room-add'); if (add) add.onclick = function () { roomForm(null); };
    var prev = $('#cal-prev'), next = $('#cal-next');
    if (prev) { prev.onclick = function () { calMonth = new Date(calMonth.getFullYear(), calMonth.getMonth() - 1, 1); viewRooms(); }; next.onclick = function () { calMonth = new Date(calMonth.getFullYear(), calMonth.getMonth() + 1, 1); viewRooms(); }; }
    v.querySelectorAll('[data-order]').forEach(function (c) { c.onclick = function () { orderDetail(c.dataset.order); }; });
  }
  function roomsList() {
    var rows = S.rooms.map(function (r, i) {
      var oc = occupancy(r);
      return '<tr><td>' + (i + 1) + '</td><td><img src="../assets/images/' + esc(r.image) + '" alt=""></td><td class="l"><b>' + esc(r.name) + '</b><br><span class="muted small">' + esc(r.type) + '</span></td><td>' + r.maxGuests + '</td><td>' + (r.pets ? 'Có' : 'Không') + '</td><td>' + money(r.price) + '</td>' +
        '<td>' + lbl(r.visible ? 'Đang hiển thị' : 'Đã ẩn', r.visible ? 'green' : 'gray') + '<br><div style="margin-top:4px">' + lbl(oc[1], oc[0]) + '</div></td>' +
        '<td><div class="actions"><button class="btn btn-primary btn-sm" data-act="view" data-id="' + r.id + '">Chi tiết</button><button class="btn btn-warn btn-sm" data-act="edit" data-id="' + r.id + '">Sửa</button><button class="btn btn-sm ' + (r.visible ? 'btn-danger' : 'btn-success') + '" data-act="toggle" data-id="' + r.id + '">' + (r.visible ? 'Ẩn' : 'Hiện') + '</button></div></td></tr>';
    }).join('');
    return '<div class="toolbar"><span class="grow muted">' + S.rooms.length + ' phòng</span><button class="btn btn-primary" id="room-add">+ Thêm phòng</button></div><div class="tbl-wrap"><table class="tbl"><thead><tr><th>STT</th><th>Hình ảnh</th><th class="l">Tên phòng</th><th>Số người tối đa</th><th>Thú cưng</th><th>Giá / ngày</th><th>Trạng thái</th><th>Tác vụ</th></tr></thead><tbody>' + rows + '</tbody></table></div>';
  }
  function roomsCalendar() {
    var y = calMonth.getFullYear(), m = calMonth.getMonth(), days = new Date(y, m + 1, 0).getDate();
    var th = '<th>Phòng</th>'; for (var d = 1; d <= days; d++) th += '<th>' + d + '</th>';
    var rows = S.rooms.map(function (r) {
      var tds = '';
      for (var d = 1; d <= days; d++) {
        var day = y + '-' + pad(m + 1) + '-' + pad(d);
        var o = S.orders.filter(function (x) { return x.roomId === r.id && x.status !== 'Đã hủy' && x.checkin <= day && day < x.checkout; })[0];
        tds += o ? '<td class="b ' + CAL_CLS[o.status] + '" data-order="' + o.id + '" title="' + esc(o.id + ' · ' + cust(o.customerId).name + ' · ' + o.status) + '">' + '' + '</td>' : '<td></td>';
      }
      return '<tr><td class="room">' + esc(r.name) + (r.visible ? '' : ' <span class="muted">(ẩn)</span>') + '</td>' + tds + '</tr>';
    }).join('');
    return '<div class="cal-nav"><button class="btn" id="cal-prev">‹</button><b>Tháng ' + (m + 1) + ' / ' + y + '</b><button class="btn" id="cal-next">›</button></div><div class="tbl-wrap"><table class="cal"><thead><tr>' + th + '</tr></thead><tbody>' + rows + '</tbody></table></div>' +
      '<div class="legend"><span><i style="background:var(--amber)"></i>Chờ xác nhận</span><span><i style="background:var(--blue)"></i>Đã xác nhận</span><span><i style="background:var(--green)"></i>Đã nhận phòng</span><span><i style="background:#999"></i>Hoàn thành</span><span class="muted">Bấm vào ô màu để xem đơn.</span></div>';
  }
  function roomAct(act, id) {
    var r = room(id);
    if (act === 'edit') return roomForm(r);
    if (act === 'toggle') { r.visible = !r.visible; refresh(); return toast(r.visible ? 'Đã hiển thị phòng.' : 'Đã ẩn phòng khỏi website.'); }
    var upcoming = S.orders.filter(function (o) { return o.roomId === id && o.status !== 'Đã hủy' && o.checkout > TODAY; }).sort(function (a, b) { return a.checkin < b.checkin ? -1 : 1; });
    var oc = occupancy(r);
    openModal('Chi tiết ' + r.name,
      '<div class="form-grid"><div class="full"><img src="../assets/images/' + esc(r.image) + '" alt="" style="width:100%;max-height:220px;object-fit:cover;border-radius:4px"></div></div><dl class="dl" style="margin-top:14px">' +
      '<div><dt>Tên phòng</dt><dd>' + esc(r.name) + '</dd></div><div><dt>Loại phòng</dt><dd>' + esc(r.type) + '</dd></div><div class="full" style="grid-column:1/-1"><dt>Mô tả</dt><dd>' + esc(r.desc) + '</dd></div>' +
      '<div><dt>Tiện ích</dt><dd>' + (r.amenities.map(function (a) { return AMENITIES[a] || a; }).join(', ') || '—') + '</dd></div><div><dt>Số người tối đa</dt><dd>' + r.maxGuests + '</dd></div>' +
      '<div><dt>Cho phép thú cưng</dt><dd>' + (r.pets ? 'Có' : 'Không') + '</dd></div><div><dt>Giá / ngày</dt><dd>' + money(r.price) + '</dd></div>' +
      '<div><dt>Hiển thị</dt><dd>' + (r.visible ? 'Đang hiển thị' : 'Đã ẩn') + '</dd></div><div><dt>Tình trạng</dt><dd>' + esc(oc[1]) + '</dd></div></dl>' +
      '<h4 class="sec-title">Lịch đặt sắp tới</h4>' + (upcoming.length ? '<table class="tbl"><thead><tr><th>Mã đơn</th><th>Nhận</th><th>Trả</th><th>Trạng thái</th></tr></thead><tbody>' + upcoming.map(function (o) { return '<tr><td>' + o.id + '</td><td>' + fmt(o.checkin) + '</td><td>' + fmt(o.checkout) + '</td><td>' + lbl(o.status, STATUS_CLS[o.status]) + '</td></tr>'; }).join('') + '</tbody></table>' : '<p class="muted">Chưa có đơn đặt nào.</p>'),
      [{ label: 'Sửa phòng', cls: 'btn-warn', fn: function () { setTimeout(function () { roomForm(r); }, 0); } }]);
  }
  function roomForm(r) {
    var isNew = !r; r = r || { name: '', type: ROOM_TYPES[0], desc: '', amenities: ['wifi', 'aircon'], maxGuests: 2, pets: false, price: 1000000, image: 'photo-01.jpg', visible: true };
    openModal(isNew ? 'Thêm phòng mới' : 'Sửa ' + r.name,
      '<form id="room-form" class="form-grid">' +
      '<div class="fld"><label class="t">Tên phòng</label><input name="name" required maxlength="60" value="' + esc(r.name) + '"></div>' +
      '<div class="fld"><label class="t">Loại phòng</label><select name="type">' + ROOM_TYPES.map(function (t) { return '<option' + (t === r.type ? ' selected' : '') + '>' + t + '</option>'; }).join('') + '</select></div>' +
      '<div class="fld full"><label class="t">Mô tả</label><textarea name="desc" rows="3">' + esc(r.desc) + '</textarea></div>' +
      '<div class="fld full"><label class="t">Tiện ích</label><div class="checks">' + Object.keys(AMENITIES).map(function (k) { return '<label><input type="checkbox" name="am" value="' + k + '"' + (r.amenities.indexOf(k) >= 0 ? ' checked' : '') + '>' + AMENITIES[k] + '</label>'; }).join('') + '</div></div>' +
      '<div class="fld"><label class="t">Số người tối đa</label><input name="maxGuests" type="number" min="1" max="20" required value="' + r.maxGuests + '"></div>' +
      '<div class="fld"><label class="t">Cho phép thú cưng</label><select name="pets"><option value="1"' + (r.pets ? ' selected' : '') + '>Có</option><option value="0"' + (r.pets ? '' : ' selected') + '>Không</option></select></div>' +
      '<div class="fld"><label class="t">Giá / ngày (₫)</label><input name="price" type="number" min="0" step="1000" required value="' + r.price + '"></div>' +
      '<div class="fld"><label class="t">Hiển thị trên website</label><select name="visible"><option value="1"' + (r.visible ? ' selected' : '') + '>Đang hiển thị</option><option value="0"' + (r.visible ? '' : ' selected') + '>Ẩn</option></select></div></form>',
      [{ label: isNew ? 'Thêm phòng' : 'Lưu thay đổi', cls: 'btn-primary', fn: function () {
        var f = $('#room-form'); if (!f.reportValidity()) return false;
        var fd = new FormData(f);
        var data = { name: fd.get('name').trim(), type: fd.get('type'), desc: fd.get('desc').trim(), amenities: fd.getAll('am'), maxGuests: +fd.get('maxGuests'), pets: fd.get('pets') === '1', price: +fd.get('price'), visible: fd.get('visible') === '1' };
        if (isNew) { S.seq++; S.rooms.push(Object.assign({ id: 'R' + S.seq, image: 'photo-01.jpg' }, data)); } else Object.assign(r, data);
        refresh(); toast(isNew ? 'Đã thêm phòng.' : 'Đã lưu thay đổi.');
      } }]);
  }

  /* ---------- Quản lý đơn đặt phòng ---------- */
  function viewOrders() {
    var f = oFilter;
    var list = S.orders.filter(function (o) {
      return (!f.q || o.id.toLowerCase().indexOf(f.q.toLowerCase()) >= 0) && (!f.status || o.status === f.status) && (!f.date || o.checkin === f.date) && (!f.room || o.roomId === f.room);
    }).sort(function (a, b) { return a.checkin < b.checkin ? 1 : -1; });
    var rows = list.map(function (o) {
      var t = total(o);
      return '<tr><td><b>' + o.id + '</b></td><td class="l">' + esc(cust(o.customerId).name) + '<br><span class="muted small">' + esc(cust(o.customerId).phone) + '</span></td><td>' + esc(room(o.roomId) ? room(o.roomId).name : '—') + '</td><td>' + fmt(o.checkin) + '</td><td>' + fmt(o.checkout) + '</td><td>' + money(t) + '</td><td class="small">Đã trả ' + money(o.paid) + '<br>Còn lại ' + money(Math.max(0, t - o.paid)) + '</td><td>' + lbl(o.status, STATUS_CLS[o.status]) +
        (o.status === 'Đã hủy' && o.refund === 'pending' ? '<br><span class="lbl amber" style="margin-top:4px">Chờ hoàn tiền</span>' : '') + '</td>' +
        '<td><div class="actions"><button class="btn btn-primary btn-sm" data-act="view" data-id="' + o.id + '">Chi tiết</button>' + (o.status === 'Đã hủy' || o.status === 'Hoàn thành' ? '' : '<button class="btn btn-warn btn-sm" data-act="edit" data-id="' + o.id + '">Sửa</button>') + '</div></td></tr>';
    }).join('');
    $('#view').innerHTML = '<div class="box"><div class="box-body"><div class="toolbar">' +
      '<input id="f-q" placeholder="Tìm theo mã đơn..." value="' + esc(f.q) + '">' +
      '<select id="f-status"><option value="">Trạng thái: tất cả</option>' + STATUSES.map(function (s) { return '<option' + (f.status === s ? ' selected' : '') + '>' + s + '</option>'; }).join('') + '</select>' +
      '<label class="muted small">Ngày nhận phòng <input id="f-date" type="date" value="' + f.date + '"></label>' +
      '<select id="f-room"><option value="">Phòng: tất cả</option>' + S.rooms.map(function (r) { return '<option value="' + r.id + '"' + (f.room === r.id ? ' selected' : '') + '>' + esc(r.name) + ' (' + r.id + ')</option>'; }).join('') + '</select>' +
      '<button class="btn" id="f-reset">Đặt lại</button><span class="grow"></span><span class="muted">' + list.length + ' đơn</span></div>' +
      '<div class="tbl-wrap"><table class="tbl"><thead><tr><th>Mã đơn</th><th class="l">Khách</th><th>Phòng</th><th>Nhận phòng</th><th>Trả phòng</th><th>Tổng tiền</th><th>Thanh toán</th><th>Trạng thái</th><th>Tác vụ</th></tr></thead><tbody>' + (rows || '<tr><td colspan="9" class="empty">Không có đơn nào phù hợp.</td></tr>') + '</tbody></table></div></div></div>';
    var bind = function (id, key, ev) { $(id).addEventListener(ev, function (e) { f[key] = e.target.value; viewOrders(); if (key === 'q') { var q = $('#f-q'); q.focus(); q.setSelectionRange(q.value.length, q.value.length); } }); };
    bind('#f-q', 'q', 'input'); bind('#f-status', 'status', 'change'); bind('#f-date', 'date', 'change'); bind('#f-room', 'room', 'change');
    $('#f-reset').onclick = function () { oFilter = { q: '', status: '', date: '', room: '' }; viewOrders(); };
    $('#view').querySelectorAll('[data-act]').forEach(function (b) { b.onclick = function () { b.dataset.act === 'view' ? orderDetail(b.dataset.id) : orderEdit(b.dataset.id); }; });
  }
  function refundInfo(o) {
    var amount = money(o.paid);
    if (o.status !== 'Đã hủy') return '';
    if (o.paid <= 0) return '<div class="alert ok">Đơn đã hủy, chưa có khoản thanh toán nào cần hoàn.</div>';
    if (o.refund === 'done') return '<div class="alert ok">Đã hoàn ' + amount + ' cho khách (đã xác nhận trên hệ thống).</div>';
    if (o.refund === 'pending') return '<div class="alert warn"><b>Hủy trước 24h nhận phòng → hoàn tiền cọc.</b> Cần hoàn ' + amount + ' cho khách.</div>';
    return '<div class="alert err"><b>Hủy sau 24h nhận phòng → không hoàn tiền cọc.</b> Khoản ' + amount + ' không được hoàn.</div>';
  }
  function orderDetail(id) {
    var o = order(id), c = cust(o.customerId), r = room(o.roomId), t = total(o), n = nightsBetween(o.checkin, o.checkout);
    var body = '<div style="margin-bottom:10px">' + lbl(o.status, STATUS_CLS[o.status]) + '</div>' + refundInfo(o) +
      '<h4 class="sec-title">Thông tin khách</h4><dl class="dl"><div><dt>Họ và tên</dt><dd>' + esc(c.name) + '</dd></div><div><dt>Số điện thoại</dt><dd>' + esc(c.phone) + '</dd></div><div><dt>Email</dt><dd>' + esc(c.email) + '</dd></div><div><dt>Số CCCD</dt><dd>' + esc(c.cccd) + '</dd></div></dl>' +
      '<h4 class="sec-title">Lưu trú</h4><dl class="dl"><div><dt>Ngày nhận phòng</dt><dd>' + fmt(o.checkin) + ' (từ 14:00)</dd></div><div><dt>Ngày trả phòng</dt><dd>' + fmt(o.checkout) + ' (trước 12:00)</dd></div><div><dt>Số đêm</dt><dd>' + n + '</dd></div><div><dt>Phòng đã đặt</dt><dd>' + esc(r ? r.name : '—') + '</dd></div>' +
      '<div><dt>Người lớn</dt><dd>' + o.adults + '</dd></div><div><dt>Trẻ em</dt><dd>' + o.children + '</dd></div><div><dt>Em bé</dt><dd>' + o.infants + '</dd></div><div><dt>Thú cưng</dt><dd>' + o.pets + '</dd></div></dl>' +
      '<h4 class="sec-title">Thanh toán</h4><dl class="dl"><div><dt>Tổng tiền</dt><dd>' + money(t) + '</dd></div><div><dt>Hình thức thanh toán</dt><dd>' + methodLabel(o) + '</dd></div><div><dt>Số tiền đã trả</dt><dd>' + money(o.paid) + '</dd></div><div><dt>Số tiền còn lại</dt><dd>' + money(Math.max(0, t - o.paid)) + '</dd></div></dl>' +
      '<p class="muted small" style="margin-top:12px">Mã đơn ' + o.id + ' · tạo ngày ' + fmt(o.createdAt) + '</p>';
    var btns = [];
    if (o.status === 'Chờ xác nhận') btns.push({ label: 'Đến xác nhận thanh toán', cls: 'btn-primary', fn: function () { tab.payments = 'confirm'; location.hash = 'payments'; route(); } });
    if (o.status === 'Đã xác nhận') btns.push({ label: 'Khách nhận phòng', cls: 'btn-success', fn: function () { o.status = 'Đã nhận phòng'; refresh(); toast('Đã chuyển sang Đã nhận phòng.'); } });
    if (o.status === 'Đã nhận phòng') btns.push({ label: 'Hoàn thành', cls: 'btn-success', fn: function () { o.status = 'Hoàn thành'; refresh(); toast('Đơn đã hoàn thành.'); } });
    if (o.status === 'Đã hủy' && o.refund === 'pending') btns.push({ label: 'Xác nhận đã hoàn tiền', cls: 'btn-success', fn: function () { o.refund = 'done'; S.tx.push({ id: 'GD-' + (++S.seq), orderId: o.id, amount: -o.paid, method: 'refund', time: TODAY + ' ' + new Date().toTimeString().slice(0, 5), status: 'Đã hoàn tiền' }); refresh(); toast('Đã xác nhận hoàn tiền.'); } });
    if (o.status !== 'Đã hủy' && o.status !== 'Hoàn thành') {
      btns.push({ label: 'Sửa đơn', cls: 'btn-warn', fn: function () { setTimeout(function () { orderEdit(o.id); }, 0); } });
      btns.push({ label: 'Hủy đơn', cls: 'btn-danger', fn: function () { return cancelOrder(o); } });
    }
    openModal('Đơn ' + o.id, body, btns);
  }
  function cancelOrder(o) {
    if (!confirm('Xác nhận khách đã liên hệ hủy đơn ' + o.id + '?')) return false;
    var hours = (parse(o.checkin).getTime() + 14 * 36e5 - Date.now()) / 36e5;
    o.status = 'Đã hủy'; o.cancelledAt = TODAY;
    o.refund = o.paid > 0 ? (hours >= 24 ? 'pending' : 'no') : 'none';
    S.tx.forEach(function (t) { if (t.orderId === o.id && t.status === 'Chờ xác nhận') t.status = 'Đã hủy'; });
    refresh(); toast(o.refund === 'pending' ? 'Đã hủy đơn. Cần hoàn tiền cho khách.' : 'Đã hủy đơn.');
  }
  function orderEdit(id) {
    var o = order(id);
    openModal('Sửa đơn ' + o.id,
      '<form id="order-form" class="form-grid"><div class="fld"><label class="t">Ngày nhận phòng</label><input type="date" name="checkin" required value="' + o.checkin + '"></div><div class="fld"><label class="t">Ngày trả phòng</label><input type="date" name="checkout" required value="' + o.checkout + '"></div>' +
      '<div class="fld full"><label class="t">Phòng</label><select name="room">' + S.rooms.filter(function (r) { return r.visible || r.id === o.roomId; }).map(function (r) { return '<option value="' + r.id + '"' + (r.id === o.roomId ? ' selected' : '') + '>' + esc(r.name) + ' · ' + money(r.price) + '/đêm · tối đa ' + r.maxGuests + ' người</option>'; }).join('') + '</select></div></form>' +
      '<p class="muted small">Hệ thống sẽ kiểm tra trùng lịch: nếu ngày hoặc phòng mới đã có đơn khác thì không thể đổi.</p><div id="order-err"></div>',
      [{ label: 'Lưu thay đổi', cls: 'btn-primary', fn: function () {
        var f = $('#order-form'); if (!f.reportValidity()) return false;
        var ci = f.checkin.value, co = f.checkout.value, rid = f.room.value, err = $('#order-err');
        var fail = function (m) { err.innerHTML = '<div class="alert err">' + m + '</div>'; return false; };
        if (co <= ci) return fail('Ngày trả phòng phải sau ngày nhận phòng.');
        var changedRoom = rid !== o.roomId, r = room(rid);
        if (ci < TODAY && ci !== o.checkin) return fail('Không thể đổi sang ngày nhận phòng đã qua.');
        var clash = S.orders.filter(function (x) { return x.id !== o.id && x.roomId === rid && x.status !== 'Đã hủy' && x.checkin < co && ci < x.checkout; })[0];
        if (clash) return fail(changedRoom ? 'Không thể đổi phòng: ' + esc(r.name) + ' đã có đơn <b>' + clash.id + '</b> (' + fmt(clash.checkin) + ' – ' + fmt(clash.checkout) + ') trùng ngày nhận phòng.' : 'Không thể đổi ngày: ngày mới trùng lịch với đơn <b>' + clash.id + '</b> (' + fmt(clash.checkin) + ' – ' + fmt(clash.checkout) + ').');
        if (o.adults + o.children > r.maxGuests) return fail(esc(r.name) + ' chỉ nhận tối đa ' + r.maxGuests + ' người, đơn này có ' + (o.adults + o.children) + ' khách.');
        if (o.pets > 0 && !r.pets) return fail(esc(r.name) + ' không cho phép mang thú cưng.');
        o.checkin = ci; o.checkout = co; o.roomId = rid; refresh(); toast('Đã cập nhật đơn ' + o.id + '.');
      } }]);
  }

  /* ---------- Quản lý thanh toán ---------- */
  function txMethod(t) { return t.method === 'refund' ? 'Hoàn tiền' : t.method === 'deposit' ? 'Tiền cọc (còn lại thu trực tiếp)' : 'Thanh toán toàn bộ'; }
  function viewPayments() {
    var pend = pendingTx(), cur = tab.payments === 'confirm' ? 'confirm' : 'list';
    var head = '<div class="tabs"><button data-tab="list" class="' + (cur === 'list' ? 'on' : '') + '">Danh sách giao dịch</button><button data-tab="confirm" class="' + (cur === 'confirm' ? 'on' : '') + '">Xác nhận thanh toán' + (pend.length ? ' (' + pend.length + ')' : '') + '</button></div>';
    var body;
    if (cur === 'list') {
      var rows = S.tx.slice().reverse().map(function (t) {
        return '<tr><td>' + t.id + '</td><td><a href="#orders" data-order="' + t.orderId + '">' + t.orderId + '</a></td><td><b style="color:' + (t.amount < 0 ? 'var(--red)' : 'inherit') + '">' + money(t.amount) + '</b></td><td>' + txMethod(t) + '</td><td>' + fmtTime(t.time) + '</td><td>' + lbl(t.status, t.status === 'Đã xác nhận' ? 'green' : t.status === 'Chờ xác nhận' ? 'amber' : t.status === 'Đã hoàn tiền' ? 'teal' : 'gray') + '</td></tr>';
      }).join('');
      body = '<div class="tbl-wrap"><table class="tbl"><thead><tr><th>Mã GD</th><th>Mã đơn</th><th>Số tiền</th><th>Phương thức thanh toán</th><th>Thời gian</th><th>Trạng thái</th></tr></thead><tbody>' + (rows || '<tr><td colspan="6" class="empty">Chưa có giao dịch.</td></tr>') + '</tbody></table></div>';
    } else {
      var prow = pend.map(function (t) {
        var o = order(t.orderId);
        return '<tr><td><a href="#orders" data-order="' + t.orderId + '">' + t.orderId + '</a></td><td class="l">' + esc(cust(o.customerId).name) + '<br><span class="muted small">' + esc(cust(o.customerId).phone) + '</span></td><td><b>' + money(t.amount) + '</b></td><td>' + txMethod(t) + '</td><td>' + fmtTime(t.time) + '</td><td><button class="btn btn-success btn-sm" data-confirm="' + t.id + '">Xác nhận đã nhận tiền</button></td></tr>';
      }).join('');
      body = '<p class="muted">Các đơn đang chờ khách gọi điện xác nhận đã chuyển khoản. Sau khi kiểm tra tiền đã về tài khoản, bấm xác nhận để đơn chuyển sang <b>Đã xác nhận</b>.</p><div class="tbl-wrap"><table class="tbl"><thead><tr><th>Mã đơn</th><th class="l">Khách</th><th>Số tiền</th><th>Phương thức</th><th>Thời gian</th><th>Tác vụ</th></tr></thead><tbody>' + (prow || '<tr><td colspan="6" class="empty">Không có đơn nào đang chờ xác nhận thanh toán.</td></tr>') + '</tbody></table></div>';
    }
    var v = $('#view'); v.innerHTML = '<div class="box"><div class="box-body">' + head + body + '</div></div>';
    v.querySelectorAll('[data-tab]').forEach(function (b) { b.onclick = function () { tab.payments = b.dataset.tab; viewPayments(); }; });
    v.querySelectorAll('[data-confirm]').forEach(function (b) { b.onclick = function () {
      var t = S.tx.filter(function (x) { return x.id === b.dataset.confirm; })[0], o = order(t.orderId);
      t.status = 'Đã xác nhận'; o.paid = t.amount; o.status = 'Đã xác nhận'; refresh(); toast('Đã xác nhận thanh toán đơn ' + o.id + '.');
    }; });
    v.querySelectorAll('a[data-order]').forEach(function (a) { a.onclick = function (e) { e.preventDefault(); orderDetail(a.dataset.order); }; });
  }
  function fmtTime(s) { var p = s.split(' '); return fmt(p[0]) + (p[1] ? ' ' + p[1] : ''); }

  /* ---------- Quản lý khách hàng ---------- */
  function viewCustomers() {
    var rows = S.customers.map(function (c, i) {
      var n = S.orders.filter(function (o) { return o.customerId === c.id; }).length;
      return '<tr><td>' + (i + 1) + '</td><td class="l"><b>' + esc(c.name) + '</b></td><td>' + esc(c.phone) + '</td><td class="l">' + esc(c.email) + '</td><td>' + fmt(c.registered) + '</td><td>' + n + '</td><td>' + lbl(c.locked ? 'Đã khóa' : 'Hoạt động', c.locked ? 'red' : 'green') + '</td><td><div class="actions"><button class="btn btn-primary btn-sm" data-act="history" data-id="' + c.id + '">Lịch sử đặt phòng</button><button class="btn btn-sm ' + (c.locked ? 'btn-success' : 'btn-danger') + '" data-act="lock" data-id="' + c.id + '">' + (c.locked ? 'Mở khóa' : 'Khóa') + '</button></div></td></tr>';
    }).join('');
    var v = $('#view');
    v.innerHTML = '<div class="box"><div class="box-body"><div class="toolbar"><span class="grow muted">' + S.customers.length + ' tài khoản khách</span></div><div class="tbl-wrap"><table class="tbl"><thead><tr><th>STT</th><th class="l">Tên</th><th>Số điện thoại</th><th class="l">Email</th><th>Ngày đăng ký</th><th>Số đơn đã đặt</th><th>Trạng thái</th><th>Tác vụ</th></tr></thead><tbody>' + rows + '</tbody></table></div></div></div>';
    v.querySelectorAll('[data-act]').forEach(function (b) { b.onclick = function () {
      var c = cust(b.dataset.id);
      if (b.dataset.act === 'lock') {
        if (!confirm((c.locked ? 'Mở khóa' : 'Khóa') + ' tài khoản ' + c.name + '?')) return;
        c.locked = !c.locked; refresh(); return toast(c.locked ? 'Đã khóa tài khoản.' : 'Đã mở khóa tài khoản.');
      }
      var os = S.orders.filter(function (o) { return o.customerId === c.id; }).sort(function (a, b2) { return a.checkin < b2.checkin ? 1 : -1; });
      openModal('Lịch sử đặt phòng — ' + c.name, os.length ? '<table class="tbl"><thead><tr><th>Mã đơn</th><th>Phòng</th><th>Nhận</th><th>Trả</th><th>Tổng tiền</th><th>Trạng thái</th></tr></thead><tbody>' + os.map(function (o) { return '<tr><td>' + o.id + '</td><td>' + esc(room(o.roomId).name) + '</td><td>' + fmt(o.checkin) + '</td><td>' + fmt(o.checkout) + '</td><td>' + money(total(o)) + '</td><td>' + lbl(o.status, STATUS_CLS[o.status]) + '</td></tr>'; }).join('') + '</tbody></table>' : '<p class="empty">Khách chưa có đơn đặt phòng nào.</p>');
    }; });
  }

  /* ---------- Quản lý nhân viên ---------- */
  var STAFF_ROLES = ['Quản lý', 'Lễ tân', 'Kế toán', 'Buồng phòng', 'Bảo vệ'];
  function viewStaff() {
    var rows = S.staff.map(function (m, i) {
      return '<tr><td>' + (i + 1) + '</td><td class="l"><b>' + esc(m.name) + '</b></td><td>' + esc(m.role) + '</td><td>' + esc(m.phone) + '</td><td class="l">' + esc(m.email) + '</td><td>' + fmt(m.joined) + '</td><td>' + lbl(m.locked ? 'Đã khóa' : 'Đang làm việc', m.locked ? 'red' : 'green') + '</td><td><div class="actions"><button class="btn btn-warn btn-sm" data-act="edit" data-id="' + m.id + '">Sửa</button><button class="btn btn-sm ' + (m.locked ? 'btn-success' : 'btn-danger') + '" data-act="lock" data-id="' + m.id + '">' + (m.locked ? 'Mở khóa' : 'Khóa') + '</button></div></td></tr>';
    }).join('');
    var v = $('#view');
    v.innerHTML = '<div class="box"><div class="box-body"><div class="toolbar"><span class="grow muted">' + S.staff.length + ' nhân viên</span><button class="btn btn-primary" id="staff-add">+ Thêm nhân viên</button></div><div class="tbl-wrap"><table class="tbl"><thead><tr><th>STT</th><th class="l">Họ và tên</th><th>Chức vụ</th><th>Số điện thoại</th><th class="l">Email</th><th>Ngày vào làm</th><th>Trạng thái</th><th>Tác vụ</th></tr></thead><tbody>' + (rows || '<tr><td colspan="8" class="empty">Chưa có nhân viên.</td></tr>') + '</tbody></table></div></div></div>';
    $('#staff-add').onclick = function () { staffForm(null); };
    v.querySelectorAll('[data-act]').forEach(function (b) { b.onclick = function () {
      var m = S.staff.filter(function (x) { return x.id === b.dataset.id; })[0];
      if (b.dataset.act === 'edit') return staffForm(m);
      if (!confirm((m.locked ? 'Mở khóa' : 'Khóa') + ' tài khoản nhân viên ' + m.name + '?')) return;
      m.locked = !m.locked; refresh(); toast(m.locked ? 'Đã khóa tài khoản nhân viên.' : 'Đã mở khóa tài khoản nhân viên.');
    }; });
  }
  function staffForm(m) {
    var isNew = !m; m = m || { name: '', phone: '', email: '', role: STAFF_ROLES[1], joined: TODAY };
    openModal(isNew ? 'Thêm nhân viên' : 'Sửa ' + m.name,
      '<form id="staff-form" class="form-grid"><div class="fld"><label class="t">Họ và tên</label><input name="name" required maxlength="80" value="' + esc(m.name) + '"></div>' +
      '<div class="fld"><label class="t">Chức vụ</label><select name="role">' + STAFF_ROLES.map(function (r) { return '<option' + (r === m.role ? ' selected' : '') + '>' + r + '</option>'; }).join('') + '</select></div>' +
      '<div class="fld"><label class="t">Số điện thoại</label><input name="phone" type="tel" required pattern="[0-9+() .-]{8,20}" value="' + esc(m.phone) + '"></div>' +
      '<div class="fld"><label class="t">Email</label><input name="email" type="email" required value="' + esc(m.email) + '"></div>' +
      '<div class="fld"><label class="t">Ngày vào làm</label><input name="joined" type="date" required value="' + m.joined + '"></div></form>',
      [{ label: isNew ? 'Thêm nhân viên' : 'Lưu thay đổi', cls: 'btn-primary', fn: function () {
        var f = $('#staff-form'); if (!f.reportValidity()) return false;
        var data = { name: f.name.value.trim(), role: f.role.value, phone: f.phone.value.trim(), email: f.email.value.trim(), joined: f.joined.value };
        if (isNew) { S.seq++; S.staff.push(Object.assign({ id: 's' + S.seq, locked: false }, data)); } else Object.assign(m, data);
        refresh(); toast(isNew ? 'Đã thêm nhân viên.' : 'Đã lưu thay đổi.');
      } }]);
  }

  /* ---------- Cấu hình hệ thống ---------- */
  function viewSettings() {
    var p = payCfg(), qr = p.qr.indexOf('data:') === 0 ? p.qr : '../' + p.qr;
    $('#view').innerHTML = '<form id="set-form"><div class="box"><div class="box-body"><h4 class="sec-title">Tài khoản ngân hàng nhận tiền</h4><p class="muted small">Hiển thị trong popup “Bắt đầu thanh toán” của khách.</p><div class="form-grid">' +
      '<div class="fld full"><label class="t">Mã QR chuyển khoản</label><div style="display:flex;gap:16px;align-items:center;flex-wrap:wrap"><img class="qr-prev" id="qr-prev" src="' + esc(qr) + '" alt="Mã QR"><div><input type="file" id="qr-file" accept="image/png,image/jpeg,image/webp,image/svg+xml"><p class="muted small">PNG, JPG, WebP hoặc SVG, tối đa 1 MB.</p></div></div></div>' +
      '<div class="fld"><label class="t">Tên người nhận</label><input name="receiver" required maxlength="60" value="' + esc(p.receiver) + '"></div><div class="fld"><label class="t">Số tài khoản</label><input name="account" required inputmode="numeric" pattern="[0-9]{6,20}" value="' + esc(p.account) + '"></div>' +
      '<div class="fld"><label class="t">Ngân hàng</label><input name="bank" required maxlength="60" value="' + esc(p.bank) + '"></div><div class="fld"><label class="t">Số điện thoại xác nhận thanh toán</label><input name="phone" type="tel" required pattern="[0-9+() .-]{8,20}" value="' + esc(p.phone) + '"></div></div>' +
      '<h4 class="sec-title">Đặt cọc</h4><div class="form-grid"><div class="fld"><label class="t">Tỷ lệ cọc tối thiểu (%)</label><input name="depositRate" type="number" min="1" max="100" step="1" required value="' + p.depositRate + '"><p class="muted small">Mặc định 30%. Áp dụng cho lựa chọn “Đặt cọc trước” khi khách thanh toán.</p></div></div></div>' +
      '<div class="m-foot" style="justify-content:flex-start"><button class="btn btn-primary" type="submit">Lưu cấu hình</button><button class="btn" type="button" id="set-reset">Khôi phục mặc định</button></div></div></form>';
    var qrData = null;
    $('#qr-file').onchange = function (e) {
      var f = e.target.files[0]; if (!f) return;
      if (f.size > 1048576) { toast('Ảnh quá lớn (tối đa 1 MB).'); e.target.value = ''; return; }
      var rd = new FileReader(); rd.onload = function () { qrData = rd.result; $('#qr-prev').src = qrData; }; rd.readAsDataURL(f);
    };
    $('#set-form').onsubmit = function (e) {
      e.preventDefault(); var fd = new FormData(e.target);
      var cfg = { qr: qrData || p.qr, receiver: fd.get('receiver').trim(), account: fd.get('account').trim(), bank: fd.get('bank').trim(), phone: fd.get('phone').trim(), depositRate: +fd.get('depositRate') };
      try { localStorage.setItem(PAY_KEY, JSON.stringify(cfg)); toast('Đã lưu cấu hình.'); } catch (err) { toast('Không lưu được (ảnh có thể quá lớn).'); }
    };
    $('#set-reset').onclick = function () { if (!confirm('Khôi phục cấu hình mặc định?')) return; try { localStorage.removeItem(PAY_KEY); } catch (err) {} viewSettings(); toast('Đã khôi phục mặc định.'); };
  }

  $('#logout-btn').addEventListener('click', function () { if (confirm('Đăng xuất khỏi hệ thống quản lý?')) location.href = '../login.html'; });
  route();
})();
