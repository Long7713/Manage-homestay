/* Mở/đóng popup có hiệu ứng (fade + trượt nhẹ), dùng chung cho mọi popup. */
window.HenryPopup = {
  isOpen: function (el) { return !el.hidden && !el.classList.contains('is-closing'); },
  show: function (el) {
    clearTimeout(el._closeTimer);
    el.classList.remove('is-closing');
    el.hidden = false;
  },
  hide: function (el) {
    if (el.hidden) return;
    el.classList.add('is-closing');
    clearTimeout(el._closeTimer);
    el._closeTimer = setTimeout(function () { el.hidden = true; el.classList.remove('is-closing'); }, 160);
  }
};
