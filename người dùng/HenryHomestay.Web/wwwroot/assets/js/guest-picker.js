(function () {
  var picker = document.getElementById('guest-picker');
  if (!picker) return;
  var popup = document.getElementById('guest-popup');
  var summary = document.getElementById('guest-summary');
  var MAX_GUESTS = 16, MAX_PETS = 5;
  var form = document.getElementById('search-form');
  var state = { adults: 2, children: 0, infants: 0, pets: 0 };
  var d = parseInt(picker.dataset.defaultAdults, 10);
  if (d >= 1) state.adults = d;
  var g = parseInt(new URLSearchParams(location.search).get('guests'), 10);
  if (g >= 1 && g <= MAX_GUESTS) state.adults = g;

  function render() {
    var total = state.adults + state.children;
    var text = total + ' khách';
    if (state.infants) text += ', ' + state.infants + ' em bé';
    if (state.pets) text += ', ' + state.pets + ' thú cưng';
    summary.textContent = text;
    if (form) { if (form.elements.guests) form.elements.guests.value = total; document.dispatchEvent(new CustomEvent('henry-search')); }
    Object.keys(state).forEach(function (k) {
      picker.querySelector('[data-count="' + k + '"]').textContent = state[k];
      var minus = picker.querySelector('[data-guest="' + k + '"][data-delta="-1"]');
      var plus = picker.querySelector('[data-guest="' + k + '"][data-delta="1"]');
      minus.disabled = state[k] <= (k === 'adults' ? 1 : 0);
      plus.disabled = k === 'pets' ? state.pets >= MAX_PETS : (k === 'infants' ? state.infants >= 5 : total >= MAX_GUESTS);
    });
  }
  function setOpen(open) {
    if (open) HenryPopup.show(popup); else HenryPopup.hide(popup);
    picker.classList.toggle('gp-active', open);
    picker.setAttribute('aria-expanded', open);
  }

  picker.addEventListener('click', function (e) {
    var btn = e.target.closest('.gp-btn');
    if (btn) {
      e.stopPropagation();
      var k = btn.dataset.guest;
      state[k] = Math.max(k === 'adults' ? 1 : 0, state[k] + Number(btn.dataset.delta));
      // Trẻ em/em bé cần ít nhất một người lớn đi cùng.
      if ((state.children || state.infants) && state.adults < 1) state.adults = 1;
      render();
      return;
    }
    if (popup.contains(e.target)) return;
    setOpen(!HenryPopup.isOpen(popup));
  });
  picker.addEventListener('keydown', function (e) {
    if (e.target === picker && (e.key === 'Enter' || e.key === ' ')) { e.preventDefault(); setOpen(!HenryPopup.isOpen(popup)); }
    if (e.key === 'Escape') setOpen(false);
  });
  document.addEventListener('click', function (e) { if (!picker.contains(e.target)) setOpen(false); });
  render();
})();
