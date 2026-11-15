/* Entirely fictional room catalogue. Prices are VND / night. */
const HenryData = {
  rooms:[
    {id:'41',name:'Vista View 41',type:'double',guests:4,area:35,price:1099000,image:'photo-02.jpg',view:'Cửa sổ nhìn thành phố',amenities:['wifi','aircon'],bed:'1 giường Queen'},
    {id:'42',name:'Vista View 42',type:'double',guests:4,area:40,price:1190000,image:'photo-03.jpg',view:'Ban công đón nắng',amenities:['wifi','aircon','balcony'],bed:'1 giường Queen'},
    {id:'51',name:'Vista View 51',type:'double',guests:4,area:35,price:1150000,image:'photo-04.jpg',view:'Thành phố về đêm',amenities:['wifi','aircon'],bed:'1 giường Queen'},
    {id:'52',name:'Vista View 52',type:'double',guests:4,area:40,price:1299000,image:'photo-05.jpg',view:'Ban công ngắm hoàng hôn',amenities:['wifi','aircon','balcony'],bed:'1 giường Queen'},
    {id:'71',name:'Studio 71',type:'studio',guests:2,area:40,price:1399000,image:'photo-08.jpg',view:'Góc nhỏ bình yên',amenities:['wifi','aircon','kitchen'],bed:'1 giường King'},
    {id:'81',name:'Family Suite 81',type:'family',guests:6,area:69,price:2099000,image:'photo-07.jpg',view:'Không gian cho cả nhà',amenities:['wifi','aircon','balcony','kitchen'],bed:'2 giường Queen'}
  ],
  booking:{id:'HN-281026',roomId:'42',nights:2,guests:2,nightly:1190000,cleaning:0,discount:0,paid:2380000},
  /* Thông tin nhận thanh toán — trang quản trị sẽ thay đổi các giá trị này. */
  payment:(()=>{const base={qr:'assets/images/payment-qr.svg',receiver:'Võ Văn Việt',account:'0708670042',bank:'MB Bank',phone:'0795443500',depositRate:30};try{return Object.assign(base,JSON.parse(localStorage.getItem('henry-payment'))||{});}catch{return base;}})(),
  money(value){return new Intl.NumberFormat('vi-VN').format(value)+' ₫';}
};
console.assert(HenryData.booking.nightly*HenryData.booking.nights+HenryData.booking.cleaning-HenryData.booking.discount===HenryData.booking.paid,'Booking total must reconcile');
