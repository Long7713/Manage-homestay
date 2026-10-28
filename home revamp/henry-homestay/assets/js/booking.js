document.addEventListener('DOMContentLoaded',()=>{
  const b=HenryData.booking,m=HenryData.money,total=b.nightly*b.nights;
  const costs={room:total,total};
  const method=()=>document.querySelector('[name=pay-method]:checked').value;
  function render(){
    const now=method()==='deposit'?Math.ceil(total*HenryData.payment.depositRate/100):total;
    costs.paid=now;costs.remaining=total-now;
    document.querySelectorAll('[data-cost]').forEach(el=>el.textContent=m(costs[el.dataset.cost]));
  }
  const rate=HenryData.payment.depositRate;
  const depositBody=document.querySelector('[name=pay-method][value=deposit]').nextElementSibling;
  depositBody.querySelector('strong').textContent='Đặt cọc trước '+rate+'%';
  depositBody.querySelector('small').textContent='Thanh toán trước '+rate+'% còn lại thanh toán vào ngày nhận phòng';
  document.querySelectorAll('[name=pay-method]').forEach(r=>r.addEventListener('change',render));render();
  const pay=HenryData.payment;
  document.getElementById('pay-qr').src=pay.qr;
  document.getElementById('pay-receiver').textContent=pay.receiver;
  document.getElementById('pay-account').textContent=pay.account;
  document.getElementById('pay-bank').textContent=pay.bank;
  const phone=document.getElementById('pay-phone');phone.textContent=pay.phone;phone.href='tel:'+pay.phone.replace(/\D/g,'');
  document.querySelector('[data-open-dialog=payment-dialog]').addEventListener('click',()=>{document.getElementById('pay-amount').textContent=m(costs.paid);});
  document.getElementById('confirm-payment').addEventListener('click',()=>{document.getElementById('payment-dialog').close();Henry.toast('Đã ghi nhận xác nhận thanh toán. Nhân viên sẽ liên hệ với bạn.');});
  document.querySelectorAll('[data-booking-id]').forEach(el=>el.textContent=b.id);
  const dob=['dobDay','dobMonth','dobYear'].map(n=>document.querySelector('#guest-other [name='+n+']'));
  const fill=(el,from,to,step)=>{for(let n=from;step>0?n<=to:n>=to;n+=step)el.add(new Option(n,n));};
  fill(dob[0],1,31,1);fill(dob[1],1,12,1);fill(dob[2],new Date().getFullYear(),1920,-1);
  dob.forEach(el=>el.addEventListener('change',()=>{const[d,mo,y]=dob.map(e=>+e.value);let msg='';if(d&&mo&&y){const t=new Date(y,mo-1,d);if(t.getMonth()!==mo-1||t.getDate()!==d)msg='Ngày sinh không tồn tại.';}dob[0].setCustomValidity(msg);}));
  const account=document.getElementById('guest-account'),other=document.getElementById('guest-other');
  document.querySelectorAll('[name=guest-source]').forEach(r=>r.addEventListener('change',()=>{const isOther=document.querySelector('[name=guest-source]:checked').value==='other';account.hidden=isOther;other.hidden=!isOther;}));
  document.getElementById('back-link').addEventListener('click',()=>{if(history.length>1&&document.referrer)history.back();else location.href='index.html';});
  document.getElementById('copy-booking').addEventListener('click',async()=>{try{await navigator.clipboard.writeText(b.id);Henry.toast('Đã sao chép mã '+b.id);}catch{Henry.toast('Mã đặt phòng: '+b.id);}});
});
