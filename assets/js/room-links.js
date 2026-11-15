/* Mỗi thẻ phòng ở trang chủ mở room-detail.html?room=<mã phòng>; trang chi tiết đọc tham số này. */
(function () {
  var cards = [].slice.call(document.querySelectorAll('[data-purpose="room-card"]'));
  document.querySelectorAll('[data-purpose="available-rooms-section"] h4').forEach(function (h) {
    var card = h.closest('.rounded-xl');
    if (card) cards.push(card);
  });

  cards.forEach(function (card) {
    var title = card.querySelector('h3, h4');
    var match = title && title.textContent.match(/(\d+)\s*$/);
    if (!match) return;
    var url = 'room-detail.html?room=' + match[1];
    var name = title.textContent.trim();

    function go() { location.href = url; }
    card.style.cursor = 'pointer';
    card.setAttribute('role', 'link');
    card.setAttribute('tabindex', '0');
    card.setAttribute('aria-label', 'Chi tiết ' + name);
    card.addEventListener('click', go);
    card.addEventListener('keydown', function (e) {
      if (e.target === card && (e.key === 'Enter' || e.key === ' ')) { e.preventDefault(); go(); }
    });
  });
})();
