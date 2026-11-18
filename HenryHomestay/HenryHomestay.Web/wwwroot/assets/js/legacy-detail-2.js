
  // 1. Favorite Toggle
  let isFavorited = false;
  const favBtn = document.getElementById('favBtn');
  const favIcon = document.getElementById('favIcon');
  const favText = document.getElementById('favText');

  if (favBtn) {
    favBtn.addEventListener('click', () => {
      isFavorited = !isFavorited;
      if (isFavorited) {
        favIcon.style.fontVariationSettings = "'FILL' 1";
        favText.innerText = 'Đã lưu';
        showToast('Đã lưu biệt thự vào danh sách yêu thích!');
      } else {
        favIcon.style.fontVariationSettings = "'FILL' 0";
        favText.innerText = 'Lưu';
        showToast('Đã xóa khỏi danh sách yêu thích');
      }
    });
  }

  // Share action
  const shareBtn = document.getElementById('shareBtn');
  if (shareBtn) {
    shareBtn.addEventListener('click', () => {
      if (navigator.share) {
        navigator.share({
          title: 'Sanctuary Forest Villa - View Thung Lũng & Bể Bơi Vô Cực',
          url: window.location.href
        }).catch(() => {});
      } else {
        copyText(window.location.href, 'Đã sao chép liên kết phòng vào bộ nhớ tạm');
      }
    });
  }

  // 2. Fullscreen Photo Gallery State & Navigation
  const galleryImages = [
    {
      src: "assets/images/photo-14.jpg",
      alt: "Breathtaking luxury villa in Da Lat nestled within pine tree forest mist, panoramic valley vista with warm interior ambient lights reflecting on a pristine heated infinity swimming pool at golden sunset, ultra clean modern architectural retreat, soft Scandinavian wood tones, cinematic high key hospitality photo"
    },
    {
      src: "assets/images/photo-15.jpg",
      alt: "Minimalist airy master bedroom in Scandinavian sanctuary villa, plush king bed with natural linen sheets, ceiling high glass window overlooking mist covered green mountains, warm morning sunlight ray, oak wooden floors"
    },
    {
      src: "assets/images/photo-16.jpg",
      alt: "Aesthetic open space designer kitchen and dining area in mountain forest sanctuary, polished travertine marble counter island, contemporary ceramic pendant lamps, natural hardwood dining table set for brunch"
    },
    {
      src: "assets/images/photo-17.jpg",
      alt: "Serene spa bathroom with deep stone freestanding bathtub placed against seamless glass wall framed by vibrant green pine trees, gentle steam, warm organic beige tiles, luxury amenities on tray"
    },
    {
      src: "assets/images/photo-18.jpg",
      alt: "Spacious wooden balcony terrace overlooking pine valley at dusk with glowing fire pit bowl, outdoor designer lounge chairs with knit wool throws, warm cozy evening atmosphere"
    }
  ];

  let currentGalleryIndex = 0;
  const totalPhotosCount = 28;

  function openGallery(index) {
    currentGalleryIndex = index || 0;
    updateGalleryView();
    const modal = document.getElementById('galleryModal');
    modal.classList.remove('hidden');
    modal.classList.add('flex');
    document.body.style.overflow = 'hidden';
  }

  function closeGallery() {
    const modal = document.getElementById('galleryModal');
    modal.classList.add('hidden');
    modal.classList.remove('flex');
    document.body.style.overflow = '';
  }

  function updateGalleryView() {
    const counter = document.getElementById('galleryCounter');
    const imgElement = document.getElementById('activeGalleryImage');
    
    // Cycle or pick from array
    const imageInfo = galleryImages[currentGalleryIndex % galleryImages.length];
    counter.innerText = `${currentGalleryIndex + 1} / ${totalPhotosCount}`;
    imgElement.src = imageInfo.src;
    imgElement.setAttribute('data-alt', imageInfo.alt);
  }

  function nextGalleryImage() {
    currentGalleryIndex = (currentGalleryIndex + 1) % totalPhotosCount;
    updateGalleryView();
  }

  function prevGalleryImage() {
    currentGalleryIndex = (currentGalleryIndex - 1 + totalPhotosCount) % totalPhotosCount;
    updateGalleryView();
  }

  // Keyboard navigation for gallery & modal dismiss
  window.addEventListener('keydown', (e) => {
    const galleryModal = document.getElementById('galleryModal');
    if (galleryModal && !galleryModal.classList.contains('hidden')) {
      if (e.key === 'Escape') closeGallery();
      if (e.key === 'ArrowRight') nextGalleryImage();
      if (e.key === 'ArrowLeft') prevGalleryImage();
    }
    if (e.key === 'Escape') closeAuthRequiredModal();
  });

  function isUserAuthenticated() {
    if (document.body && document.body.dataset.auth === 'guest') return false;
    const hasAuthCookie = document.cookie.split(';').some(c => c.trim().startsWith('HenryUserId='));
    const hasLocalFlag = localStorage.getItem('henry-logged-in') === '1';
    const isAuthed = location.protocol.startsWith('http') ? hasAuthCookie : (hasAuthCookie || hasLocalFlag);
    if (!isAuthed) {
      if (typeof HenryData !== 'undefined') HenryData.currentUser = null;
      return false;
    }
    return typeof HenryData !== 'undefined' && !!HenryData.currentUser;
  }

  function showAuthRequiredModal() {
    showToast('Vui lòng đăng nhập hoặc đăng ký tài khoản để đặt phòng!');
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

  // 4. Booking Process & VietQR Modal
  let selectedPayType = 'deposit';

  function openBookingModal() {
    if (!isUserAuthenticated()) {
      showAuthRequiredModal();
      return;
    }
    document.getElementById('bookingStep1').classList.remove('hidden');
    document.getElementById('bookingStep2').classList.add('hidden');
    document.getElementById('bookingStep3').classList.add('hidden');
    
    const modal = document.getElementById('bookingModal');
    modal.classList.remove('hidden');
    modal.classList.add('flex');
    document.body.style.overflow = 'hidden';
  }

  function closeBookingModal() {
    const modal = document.getElementById('bookingModal');
    modal.classList.add('hidden');
    modal.classList.remove('flex');
    document.body.style.overflow = '';
  }

  function updatePayOption(type) {
    selectedPayType = type;
    const amountEl = document.getElementById('displayAmountToPay');
    const qrAmountEl = document.getElementById('qrTransferAmount');
    
    if (type === 'deposit') {
      amountEl.innerText = '3.500.000 ₫';
      qrAmountEl.innerText = '3.500.000 ₫';
    } else {
      amountEl.innerText = '7.000.000 ₫';
      qrAmountEl.innerText = '7.000.000 ₫';
    }
  }

  function proceedToVietQR() {
    if (!isUserAuthenticated()) {
      showAuthRequiredModal();
      return;
    }
    document.getElementById('bookingStep1').classList.add('hidden');
    document.getElementById('bookingStep2').classList.remove('hidden');
    document.getElementById('bookingStep2').classList.add('flex');
  }

  function backToStep1() {
    document.getElementById('bookingStep2').classList.add('hidden');
    document.getElementById('bookingStep2').classList.remove('flex');
    document.getElementById('bookingStep1').classList.remove('hidden');
  }

  function completeBookingFlow() {
    document.getElementById('bookingStep2').classList.add('hidden');
    document.getElementById('bookingStep2').classList.remove('flex');
    document.getElementById('bookingStep3').classList.remove('hidden');
    document.getElementById('bookingStep3').classList.add('flex');
  }

  // Toast Helper
  function showToast(msg) {
    const toast = document.getElementById('toastMessage');
    const toastText = document.getElementById('toastText');
    if (!toast || !toastText) return;
    toastText.innerText = msg;
    toast.classList.remove('translate-y-24', 'opacity-0');
    toast.classList.add('translate-y-0', 'opacity-100');
    
    setTimeout(() => {
      toast.classList.remove('translate-y-0', 'opacity-100');
      toast.classList.add('translate-y-24', 'opacity-0');
    }, 3200);
  }

  function copyText(text, successMsg) {
    navigator.clipboard.writeText(text).then(() => {
      showToast(successMsg || 'Đã sao chép');
    }).catch(() => {
      showToast('Đã sao chép: ' + text);
    });
  }
