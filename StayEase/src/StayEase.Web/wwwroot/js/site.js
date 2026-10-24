(() => {
  const toggle=document.querySelector('.mobile-toggle');
  const nav=document.querySelector('#mainNav');
  if(toggle&&nav) toggle.addEventListener('click',()=>{
    const open=nav.classList.toggle('open');
    toggle.setAttribute('aria-expanded',String(open));
  });
  for(const form of document.querySelectorAll('form')){
    const start=form.querySelector('input[name="checkIn"],input[name="CheckIn"]');
    const end=form.querySelector('input[name="checkOut"],input[name="CheckOut"]');
    if(!start||!end)continue;
    start.addEventListener('change',()=>{
      end.min=start.value;
      if(start.value && end.value && end.value<=start.value){
        const next=new Date(`${start.value}T12:00:00`);
        next.setDate(next.getDate()+1);
        const y=next.getFullYear(),m=String(next.getMonth()+1).padStart(2,'0'),d=String(next.getDate()).padStart(2,'0');
        end.value=`${y}-${m}-${d}`;
      }
    });
    form.addEventListener('submit',event=>{
      if(start.value&&end.value&&start.value>=end.value){
        event.preventDefault();alert('Ngày trả phòng phải sau ngày nhận phòng.');
      }
    });
  }
})();
