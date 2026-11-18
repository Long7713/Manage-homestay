/* Connect the supplied pages without replacing their visual components. */
document.addEventListener('DOMContentLoaded',()=>{
  const hasAuthCookie=document.cookie.split(';').some(c=>c.trim().startsWith('HenryUserId='));
  const hasLocalFlag=localStorage.getItem('henry-logged-in')==='1';
  const isAuthed=location.protocol.startsWith('http')?hasAuthCookie:(hasAuthCookie||hasLocalFlag);
  if(!isAuthed||document.body.dataset.auth==='guest'){
    if(typeof HenryData!=='undefined')HenryData.currentUser=null;
  }

  const header=document.querySelector('header')||document.createElement('header');
  const logo=header.querySelector('a');if(logo)logo.href='index.html';
  header.querySelectorAll('nav a').forEach(a=>{
    const label=a.textContent.trim();
    if(label==='Phòng nghỉ'){a.remove();return;}
    a.href=label==='Trang chủ'?'index.html':label==='Ẩm thực'?'food.html':label==='Liên hệ'?'contact.html':'#contact';
  });
  const footer=document.querySelector('footer');if(footer)footer.id='contact';
  document.querySelectorAll('a').forEach(a=>{
    if(a.textContent.trim()==='Đặt ngay'||a.getAttribute('aria-label')?.startsWith('Chi tiết')){
      const card=a.closest('[data-purpose="room-card"], .rounded-xl');
      const title=card?.querySelector('h3, h4');
      const match=title?.textContent.match(/(\d+)\s*$/);
      a.href=match?('room-detail.html?room='+match[1]):'room-detail.html';
    }
  });
  document.querySelectorAll('button').forEach(b=>{
    if(b.textContent.trim()==='Tìm phòng'){
      b.addEventListener('click',()=>{
        const ci=document.getElementById('zone-checkin')?.dataset.value||'';
        const co=document.getElementById('zone-checkout')?.dataset.value||'';
        const adults=parseInt(document.querySelector('[data-count="adults"]')?.textContent||'2',10)||2;
        const children=parseInt(document.querySelector('[data-count="children"]')?.textContent||'0',10)||0;
        const p=new URLSearchParams();
        if(ci)p.set('checkin',ci);
        if(co)p.set('checkout',co);
        p.set('guests',String(adults+children));
        location.href='rooms.html'+(p.toString()?'?'+p.toString():'');
      });
    }
  });

  const userMenuWrap=header.querySelector('[data-purpose="user-menu"]');
  const mobileTrigger=header.querySelector('[data-purpose="mobile-menu-trigger"]');
  const currentUser=(isAuthed&&document.body.dataset.auth!=='guest'&&typeof HenryData!=='undefined'&&HenryData.currentUser)?HenryData.currentUser:null;

  if(userMenuWrap){
    if(currentUser){
      const initials=currentUser.fullName?currentUser.fullName.trim().split(/\s+/).slice(-2).map(w=>w[0].toUpperCase()).join(''):'TV';
      const guestBtns=userMenuWrap.querySelector('[data-purpose="guest-auth-buttons"]');
      let pill=userMenuWrap.querySelector('[data-purpose="user-account-pill"]');
      if(!pill){
        pill=document.createElement('div');
        pill.setAttribute('data-purpose','user-account-pill');
        pill.className='hidden sm:flex items-center gap-2.5 px-3.5 py-1.5 rounded-full border border-stone-300 bg-stone-50 hover:bg-white hover:border-brand-goldAccent/60 transition-all cursor-pointer shadow-sm';
        pill.innerHTML=`<svg class="w-4 h-4 text-stone-600" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path d="M4 6h16M4 12h16M4 18h16" stroke-linecap="round" stroke-linejoin="round" stroke-width="2"></path></svg><div class="w-7 h-7 rounded-full bg-brand-goldAccent/20 text-brand-brownDark flex items-center justify-center font-bold text-xs ring-1 ring-brand-goldAccent/40" data-purpose="user-avatar">${initials}</div><span class="text-sm font-medium text-stone-800 pr-1" data-purpose="user-name">${currentUser.fullName}</span>`;
        if(guestBtns)guestBtns.replaceWith(pill);
        else userMenuWrap.prepend(pill);
      }
      pill.setAttribute('role','button');
      pill.tabIndex=0;
      pill.setAttribute('aria-label','Menu tài khoản');
      pill.setAttribute('aria-expanded','false');
      const menu=document.createElement('nav');
      menu.className='legacy-user-menu';
      menu.hidden=true;
      menu.setAttribute('aria-label','Tài khoản');
      menu.innerHTML=`<a href="profile.html">Thông tin cá nhân (${currentUser.fullName})</a><a href="profile.html#bookings">Đơn đặt phòng của tôi</a><a href="profile.html#saved">Phòng đã lưu</a><a href="login.html" data-action="logout">Đăng xuất</a>`;
      header.append(menu);
      const place=()=>{const h=header.getBoundingClientRect(),a=(pill.offsetParent?pill:mobileTrigger||pill).getBoundingClientRect();menu.style.top=(a.bottom-h.top+8)+'px';menu.style.right='auto';menu.style.left=Math.max(8,Math.min(a.right-h.left-menu.offsetWidth,h.width-menu.offsetWidth-8))+'px';};
      const toggle=()=>{const open=!HenryPopup.isOpen(menu);if(open){HenryPopup.show(menu);place();}else HenryPopup.hide(menu);pill.setAttribute('aria-expanded',String(open));};
      pill.addEventListener('click',toggle);
      pill.addEventListener('keydown',e=>{if(e.key==='Enter'||e.key===' '){e.preventDefault();toggle();}});
      mobileTrigger?.addEventListener('click',toggle);
      document.addEventListener('click',e=>{if(!header.contains(e.target)){HenryPopup.hide(menu);pill.setAttribute('aria-expanded','false');}});
      document.addEventListener('keydown',e=>{if(e.key==='Escape'){HenryPopup.hide(menu);pill.setAttribute('aria-expanded','false');}});
    }else{
      if(!userMenuWrap.querySelector('[data-purpose="guest-auth-buttons"]')){
        const guestBtns=document.createElement('div');
        guestBtns.setAttribute('data-purpose','guest-auth-buttons');
        guestBtns.className='flex items-center gap-2 sm:gap-2.5';
        guestBtns.innerHTML='<a class="px-4 py-2 rounded-full border border-stone-300 bg-white hover:bg-stone-50 hover:border-brand-goldAccent text-brand-brownDark hover:text-brand-goldAccent text-xs sm:text-sm font-semibold transition-all shadow-sm" data-purpose="login-button" href="login.html">Đăng nhập</a><a class="px-4 py-2 rounded-full bg-brand-brownDark hover:bg-brand-goldAccent text-white text-xs sm:text-sm font-semibold transition-all shadow-sm" data-purpose="register-button" href="register.html">Đăng ký</a>';
        const oldPill=userMenuWrap.querySelector('[data-purpose="user-account-pill"]')||userMenuWrap.querySelector('div');
        if(oldPill)oldPill.replaceWith(guestBtns);
        else userMenuWrap.prepend(guestBtns);
      }
      if(mobileTrigger){
        const menu=document.createElement('nav');
        menu.className='legacy-user-menu';
        menu.hidden=true;
        menu.setAttribute('aria-label','Điều hướng di động');
        menu.innerHTML='<a href="index.html">Trang chủ</a><a href="food.html">Ẩm thực</a><a href="contact.html">Liên hệ</a><a href="login.html">Đăng nhập</a><a href="register.html">Đăng ký</a>';
        header.append(menu);
        const place=()=>{const h=header.getBoundingClientRect(),a=mobileTrigger.getBoundingClientRect();menu.style.top=(a.bottom-h.top+8)+'px';menu.style.right='16px';menu.style.left='auto';};
        const toggle=()=>{const open=!HenryPopup.isOpen(menu);if(open){HenryPopup.show(menu);place();}else HenryPopup.hide(menu);mobileTrigger.setAttribute('aria-expanded',String(open));};
        mobileTrigger.addEventListener('click',toggle);
        document.addEventListener('click',e=>{if(!header.contains(e.target)){HenryPopup.hide(menu);mobileTrigger.setAttribute('aria-expanded','false');}});
        document.addEventListener('keydown',e=>{if(e.key==='Escape'){HenryPopup.hide(menu);mobileTrigger.setAttribute('aria-expanded','false');}});
      }
    }
  }
  // Only allow booking when logged in; otherwise show login/register requirement modal.
  const handleBookingAttempt=()=>{
    const hasCookie=document.cookie.split(';').some(c=>c.trim().startsWith('HenryUserId='));
    const hasLocal=localStorage.getItem('henry-logged-in')==='1';
    const authed=(location.protocol.startsWith('http')?hasCookie:(hasCookie||hasLocal))&&typeof HenryData!=='undefined'&&!!HenryData.currentUser;
    if(!authed){
      if(typeof window.showAuthRequiredModal==='function')window.showAuthRequiredModal();
      return;
    }
    location.href='booking-detail.html';
  };
  if(typeof window.openBookingModal==='function')window.openBookingModal=handleBookingAttempt;
  if(typeof window.proceedToVietQR==='function')window.proceedToVietQR=handleBookingAttempt;
});
