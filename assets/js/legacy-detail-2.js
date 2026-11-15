
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
      src: "placeholder",
      alt: "Breathtaking luxury villa in Da Lat nestled within pine tree forest mist, panoramic valley vista with warm interior ambient lights reflecting on a pristine heated infinity swimming pool at golden sunset, ultra clean modern architectural retreat, soft Scandinavian wood tones, cinematic high key hospitality photo"
    },
    {
      src: "placeholder",
      alt: "Minimalist airy master bedroom in Scandinavian sanctuary villa, plush king bed with natural linen sheets, ceiling high glass window overlooking mist covered green mountains, warm morning sunlight ray, oak wooden floors"
    },
    {
      src: "placeholder",
      alt: "Aesthetic open space designer kitchen and dining area in mountain forest sanctuary, polished travertine marble counter island, contemporary ceramic pendant lamps, natural hardwood dining table set for brunch"
    },
    {
      src: "placeholder",
      alt: "Serene spa bathroom with deep stone freestanding bathtub placed against seamless glass wall framed by vibrant green pine trees, gentle steam, warm organic beige tiles, luxury amenities on tray"
    },
    {
      src: "placeholder",
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
    if (!galleryModal.classList.contains('hidden')) {
      if (e.key === 'Escape') closeGallery();
      if (e.key === 'ArrowRight') nextGalleryImage();
      if (e.key === 'ArrowLeft') prevGalleryImage();
    }
  });

  // 4. Booking Process & VietQR Modal
  let selectedPayType = 'deposit';

  function openBookingModal() {
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
    toastText.innerText = msg;
    toast.classList.remove('translate-y-24', 'opacity-0');
    toast.classList.add('translate-y-0', 'opacity-100');
    
    setTimeout(() => {
      toast.classList.remove('translate-y-0', 'opacity-100');
      toast.classList.add('translate-y-24', 'opacity-0');
    }, 2800);
  }

  function copyText(text, successMsg) {
    navigator.clipboard.writeText(text).then(() => {
      showToast(successMsg || 'Đã sao chép');
    }).catch(() => {
      showToast('Đã sao chép: ' + text);
    });
  }
