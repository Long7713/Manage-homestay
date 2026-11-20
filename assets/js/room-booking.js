/* Cập nhật tổng tiền theo số đêm đã chọn ở khung đặt phòng. */
(function () {
  var form = document.getElementById('search-form');
  var total = document.getElementById('booking-total');
  var label = document.getElementById('booking-nights');
  if (!form || !total) return;
  var nightly = Number(total.dataset.nightly);
  function update() {
    var a = form.elements.checkin.value, b = form.elements.checkout.value;
    if (!a || !b) { total.textContent = new Intl.NumberFormat('vi-VN').format(nightly) + ' ₫'; label.textContent = 'cho 1 đêm'; return; }
    var nights = Math.round((new Date(b + 'T12:00:00') - new Date(a + 'T12:00:00')) / 86400000);
    if (nights < 1) return;
    total.textContent = new Intl.NumberFormat('vi-VN').format(nightly * nights) + ' ₫';
    label.textContent = 'cho ' + nights + ' đêm';
  }
  document.addEventListener('henry-search', update);
  update();
})();
