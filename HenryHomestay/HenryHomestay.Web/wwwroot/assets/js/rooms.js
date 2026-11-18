document.addEventListener('DOMContentLoaded',()=>{
  const form=document.getElementById('search-form');const filters=document.getElementById('room-filters');const grid=document.getElementById('rooms-grid');const minPrice=document.getElementById('min-price');const maxPrice=document.getElementById('max-price');const digits=el=>Number(el.value.replace(/\D/g,''))||0;
  
  function isRoomAvailable(roomId, checkin, checkout){
    if(!checkin || !checkout || checkout <= checkin) return true;
    const booked = (HenryData.bookedNights && HenryData.bookedNights[roomId]) || [];
    return !booked.some(stayDate => stayDate >= checkin && stayDate < checkout);
  }

  function render(){
    const types=[...filters.querySelectorAll('[name=type]:checked')].map(e=>e.value);const amenities=[...filters.querySelectorAll('[name=amenity]:checked')].map(e=>e.value);const guests=Number(form.elements.guests.value)||1;const saved=Henry.readSaved();
    const checkin=form.elements.checkin.value,checkout=form.elements.checkout.value;
    let rooms=HenryData.rooms.filter(r=>
      isRoomAvailable(r.id, checkin, checkout) &&
      r.price>=digits(minPrice) &&
      (!digits(maxPrice)||r.price<=digits(maxPrice)) &&
      r.guests>=guests &&
      (!types.length||types.includes(r.type)) &&
      amenities.every(a=>r.amenities.includes(a))
    );
    document.getElementById('result-count').textContent=`${rooms.length} phòng phù hợp`;
    const date=d=>new Date(d+'T12:00:00').toLocaleDateString('vi-VN',{day:'2-digit',month:'2-digit'});
    document.getElementById('search-summary').textContent=`${checkin&&checkout?date(checkin)+' – '+date(checkout)+' · ':''}${guests} khách · 1 phòng · HENRY homestay`;
    const querySuffix = (roomId) => {
      const p = new URLSearchParams();
      p.set('room', roomId);
      if(checkin) p.set('checkin', checkin);
      if(checkout) p.set('checkout', checkout);
      if(guests) p.set('guests', String(guests));
      return 'room-detail.html?' + p.toString();
    };
    grid.innerHTML=rooms.length?rooms.map(r=>`<article class="room-card"><div class="room-image"><a href="${querySuffix(r.id)}" aria-label="Xem phòng ${r.name}"><img src="assets/images/${r.image}" alt="Không gian phòng ${r.name}" width="500" height="420"></a><button class="favorite" data-room="${r.id}" data-room-dbid="${r.dbId||0}" aria-label="Lưu phòng ${r.name}" aria-pressed="${saved.includes(r.id)}">${Henry.icon('heart')}</button></div><a href="${querySuffix(r.id)}"><h3>${r.name}</h3></a><p class="meta">${r.view}</p><p class="meta">${r.guests} khách · ${r.bed} · ${r.area} m²</p><div class="room-tags"><span>Wi-Fi</span><span>${r.amenities.includes('balcony')?'Ban công riêng':'Điều hòa'}</span></div><p class="price"><strong>${HenryData.money(r.price)}</strong><span class="muted"> / đêm</span></p></article>`).join(''):`<div class="empty"><h3>Chưa có phòng phù hợp</h3><p>Thử đổi ngày nhận/trả phòng, tăng ngân sách hoặc bỏ bớt tiện nghi để xem thêm phòng.</p><button class="btn" id="empty-reset">Xóa bộ lọc</button></div>`;
    document.getElementById('empty-reset')?.addEventListener('click',reset);
    grid.querySelectorAll('.favorite').forEach(b=>b.addEventListener('click',async()=>{
      let ids=Henry.readSaved();
      const active=!ids.includes(b.dataset.room);
      ids=active?[...ids,b.dataset.room]:ids.filter(id=>id!==b.dataset.room);
      Henry.saveRooms(ids);
      b.setAttribute('aria-pressed',String(active));
      if(window.HenryLive && window.HenryLive.toggleSavedRoom){
        await window.HenryLive.toggleSavedRoom(b.dataset.room);
      }
      Henry.toast(active?'Đã lưu phòng vào danh sách yêu thích.':'Đã bỏ lưu phòng.');
    }));
  }
  function reset(){filters.reset();render();}
  form.addEventListener('submit',e=>{e.preventDefault();if(!form.elements.checkin.value||!form.elements.checkout.value){Henry.toast('Vui lòng chọn ngày nhận và trả phòng.');return;}history.replaceState(null,'','?'+new URLSearchParams(new FormData(form)));render();Henry.toast('Đã cập nhật kết quả phòng trống từ CSDL.');});
  document.addEventListener('henry-search',render);
  [minPrice,maxPrice].forEach(el=>el.addEventListener('input',()=>{const n=digits(el);el.value=n?new Intl.NumberFormat('vi-VN').format(n):'';}));
  document.getElementById('back-link').addEventListener('click',()=>{if(history.length>1&&document.referrer)history.back();else location.href='index.html';});
  filters.addEventListener('input',render);document.getElementById('reset-filters').addEventListener('click',reset);
  document.getElementById('filter-toggle').addEventListener('click',e=>{const open=filters.classList.toggle('is-open');e.currentTarget.setAttribute('aria-expanded',String(open));});render();
});
